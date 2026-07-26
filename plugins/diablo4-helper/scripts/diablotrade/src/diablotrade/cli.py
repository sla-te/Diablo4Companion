"""Command line entry point.

    diablotrade learn   <SEARCH_ID> [--unique UUID]
    diablotrade filter  <SEARCH_ID> --attrs A,B,C,D [--min-matches N]
    diablotrade groups  --attrs A,B,C,D --min-matches N
    diablotrade actions <PAGE_PATH>

`SEARCH_ID` is the trailing segment of a diablo.trade/listings/items/<id> URL.
Build the search once in the site UI (broad: just the item, no affix filter),
then let these commands do the filtering.
"""

from __future__ import annotations

import argparse
import sys
from collections.abc import Sequence

from . import metadata
from .actions import discover_action_ids
from .client import Client, DiabloTradeError
from .filters import as_or_groups, rank
from .models import Listing


def _load(client: Client, search_id: str, unique: str | None) -> list[Listing]:
    listings = client.search_listings(search_id)
    if unique:
        listings = [x for x in listings if x.unique_equipment_id == unique]
    return listings


def _cmd_learn(args: argparse.Namespace) -> int:
    client = Client(cookie=args.cookie)
    listings = _load(client, args.search_id, args.unique)
    if not listings:
        print("No listings matched.")
        return 1
    stats = metadata.frequency(listings)
    print(f"{len(listings)} listings, {len(stats)} distinct attributes\n")
    print(f"{'attributeId':38} {'seen':>5} {'pct':>5}  example  name")
    for stat in stats:
        flag = "  <- guaranteed" if stat.is_guaranteed else ""
        name = stat.name or ""
        values = ",".join(str(v) for v in stat.example_values)
        print(
            f"{stat.attribute_id:38} {stat.count:>5} {stat.pct:>4.0f}%  "
            f"{values:<8} {name}{flag}"
        )
    if len(listings) < 20:
        print(
            f"\nNote: only {len(listings)} listings sampled. "
            "'guaranteed' is suggestive at this size, not proof."
        )
    return 0


def _cmd_filter(args: argparse.Namespace) -> int:
    wanted = [a.strip() for a in args.attrs.split(",") if a.strip()]
    if args.min_matches > len(wanted):
        print(
            f"--min-matches {args.min_matches} exceeds the {len(wanted)} attrs given",
            file=sys.stderr,
        )
        return 2
    client = Client(cookie=args.cookie)
    listings = _load(client, args.search_id, args.unique)
    matches = rank(listings, wanted, minimum=args.min_matches)
    print(
        f"{len(matches)} of {len(listings)} listings carry at least "
        f"{args.min_matches} of {len(wanted)} attributes\n"
    )
    for match in matches:
        listing = match.listing
        item = listing.item
        print(
            f"[{match.score}/{len(wanted)}] {listing.name}  "
            f"power={item.power} ga={item.greater_affix_count} price={listing.price}"
        )
        print(f"          id {listing.id}")
        for attribute_id in sorted(match.matched):
            print(f"          + {attribute_id} = {item.value_of(attribute_id)}")
        print()
    return 0


def _cmd_groups(args: argparse.Namespace) -> int:
    """Print the same rule as OR-groups, for rebuilding it in the site UI."""
    wanted = [a.strip() for a in args.attrs.split(",") if a.strip()]
    groups = as_or_groups(args.min_matches, wanted)
    print(
        f"'at least {args.min_matches} of {len(wanted)}' as AND-of-OR groups "
        f"({len(groups)} groups, all must hold):\n"
    )
    for index, group in enumerate(groups, start=1):
        print(f"  {index}. " + "  OR  ".join(group))
    return 0


def _normalize_page_path(value: str) -> str:
    """Accept "/listings/create", "listings/create" or a full URL.

    Git Bash rewrites a leading-slash argument into a Windows path before the
    process ever sees it (MSYS path conversion), so "listings/create" without
    the slash is the form that survives every shell.
    """
    if "://" in value:
        return value
    tail = value.replace("\\", "/")
    for marker in ("/listings/", "/api/"):
        index = tail.find(marker)
        if index > 0:  # a mangled absolute path still contains the real suffix
            tail = tail[index:]
            break
    return tail if tail.startswith("/") else "/" + tail


def _cmd_actions(args: argparse.Namespace) -> int:
    client = Client(cookie=args.cookie)
    ids = discover_action_ids(client, _normalize_page_path(args.page_path))
    print(f"{len(ids)} Server Action ids referenced by {args.page_path}:\n")
    for action_id in ids:
        print(f"  {action_id}")
    print(
        "\nThese are build-specific and unlabelled. Which one posts a listing "
        "cannot be determined statically - capture one real submission in "
        "devtools to find out. See docs/posting.md."
    )
    return 0


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="diablotrade",
        description=__doc__,
        formatter_class=argparse.RawDescriptionHelpFormatter,
    )
    parser.add_argument("--cookie", help="session cookie, only needed for writes")
    sub = parser.add_subparsers(dest="command", required=True)

    learn = sub.add_parser("learn", help="report attribute frequency for a search")
    learn.add_argument("search_id")
    learn.add_argument("--unique", help="restrict to this uniqueEquipmentId")
    learn.set_defaults(func=_cmd_learn)

    filter_cmd = sub.add_parser("filter", help="rank listings by attribute matches")
    filter_cmd.add_argument("search_id")
    filter_cmd.add_argument("--unique", help="restrict to this uniqueEquipmentId")
    filter_cmd.add_argument("--attrs", required=True, help="comma-separated UUIDs")
    filter_cmd.add_argument("--min-matches", type=int, default=3)
    filter_cmd.set_defaults(func=_cmd_filter)

    groups = sub.add_parser("groups", help="print the rule as OR-groups for the site UI")
    groups.add_argument("--attrs", required=True, help="comma-separated UUIDs or names")
    groups.add_argument("--min-matches", type=int, required=True)
    groups.set_defaults(func=_cmd_groups)

    actions_cmd = sub.add_parser("actions", help="list a page's Server Action ids")
    actions_cmd.add_argument("page_path", help="e.g. /listings/create")
    actions_cmd.set_defaults(func=_cmd_actions)

    return parser


def main(argv: Sequence[str] | None = None) -> int:
    args = build_parser().parse_args(argv)
    try:
        result: int = args.func(args)
        return result
    except DiabloTradeError as exc:
        print(f"error: {exc}", file=sys.stderr)
        return 1
    except ValueError as exc:
        print(f"error: {exc}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
