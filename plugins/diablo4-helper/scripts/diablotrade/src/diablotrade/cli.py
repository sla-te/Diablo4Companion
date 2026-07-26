"""Command line entry point.

    diablotrade learn   <SEARCH_ID> [--unique UUID]
    diablotrade filter  <SEARCH_ID> --attrs A,B,C,D [--min-matches N]
    diablotrade enchant <SEARCH_ID> --attrs A,B,C,D
    diablotrade groups  --attrs A,B,C,D --min-matches N
    diablotrade aspects <SEARCH_ID> [--min-roll R] [--max-price G]
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
from .filters import as_or_groups, is_negotiable, plan_enchants, rank
from .models import SEARCH_ID_CAP, Listing


def _load(client: Client, search_id: str, unique: str | None = None) -> list[Listing]:
    """Hydrate a saved search, warning loudly if it came back truncated.

    A capped search under-reports without saying so, and every ranking built on
    it inherits that silence - the cheapest match may simply not be in the 500.
    """
    search = client.get_search(search_id)
    if search.is_truncated:
        print(
            f"warning: search {search_id} returned the {SEARCH_ID_CAP}-id cap, "
            + "so results are TRUNCATED and totals are a floor, not a count. "
            + "The endpoint has no pagination; narrow the filter instead.",
            file=sys.stderr,
        )
    listings = list(client.get_listings(search.listing_ids))
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
            + f"{values:<8} {name}{flag}"
        )
    if len(listings) < 20:
        print(
            f"\nNote: only {len(listings)} listings sampled. "
            + "'guaranteed' is suggestive at this size, not proof."
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
        + f"{args.min_matches} of {len(wanted)} attributes\n"
    )
    for match in matches:
        listing = match.listing
        item = listing.item
        print(
            f"[{match.score}/{len(wanted)}] {listing.name}  "
            + f"power={item.power} ga={item.greater_affix_count} price={listing.price}"
        )
        print(f"          id {listing.id}")
        for attribute_id in sorted(match.matched):
            print(f"          + {attribute_id} = {item.value_of(attribute_id)}")
        print()
    return 0


def _cmd_enchant(args: argparse.Namespace) -> int:
    """Rank listings by what they are worth AFTER one Occultist enchant.

    Unlike `filter`, this prices the enchant rather than assuming it is free:
    overwriting a junk affix costs nothing, overwriting the only spare roll when
    it is a Greater Affix costs that GA, and an item with no junk affix at all
    gets flagged for inspection because the payload cannot say whether it has a
    free slot or is full.
    """
    wanted = [a.strip() for a in args.attrs.split(",") if a.strip()]
    client = Client(cookie=args.cookie)
    listings = _load(client, args.search_id, args.unique)
    plans = plan_enchants(listings, wanted, closable_only=not args.include_unfixable)

    print(
        f"{len(plans)} of {len(listings)} listings reach {len(wanted)} of "
        + f"{len(wanted)} within one enchant\n"
    )
    for plan in plans[: args.limit]:
        listing = plan.listing
        if plan.gap == 0:
            note = "complete, no enchant needed"
        elif plan.slot_state_unknown:
            note = "CHECK BY HAND: no junk affix - free slot, or stuck"
        elif plan.costs_a_greater_affix:
            note = "enchant costs a GREATER AFFIX"
        else:
            note = f"enchant away one of {len(plan.sacrifice)} junk affixes"
        price = "negotiable" if is_negotiable(listing) else (listing.price or "?")
        print(
            f"[{plan.score}/{len(wanted)}] {price:>12}  "
            + f"ga={listing.item.greater_affix_count}  {note}"
        )
        for attribute_id in sorted(plan.missing):
            print(
                f"          needs {metadata.KNOWN_ATTRIBUTES.get(attribute_id, attribute_id)}"
            )
        print(f"          https://diablo.trade/listings/{listing.id}")
    return 0


def _cmd_groups(args: argparse.Namespace) -> int:
    """Print the same rule as OR-groups, for rebuilding it in the site UI."""
    wanted = [a.strip() for a in args.attrs.split(",") if a.strip()]
    groups = as_or_groups(args.min_matches, wanted)
    print(
        f"'at least {args.min_matches} of {len(wanted)}' as AND-of-OR groups "
        + f"({len(groups)} groups, all must hold):\n"
    )
    for index, group in enumerate(groups, start=1):
        print(f"  {index}. " + "  OR  ".join(group))
    return 0


def _cmd_aspects(args: argparse.Namespace) -> int:
    """Rank aspect carriers by BASE roll, so slots stay comparable.

    Buying an aspect is a Codex play: you salvage the item, the Codex keeps the
    best base roll it has seen, and you re-imprint from it. So the only number
    that matters is the base roll, not the inflated one the item displays.
    """
    client = Client(cookie=args.cookie)
    listings = _load(client, args.search_id)
    priced = [x for x in listings if x.raw_price and x.raw_price > 0]
    if args.max_price:
        priced = [x for x in priced if (x.raw_price or 0) <= args.max_price]

    ranked: list[tuple[float, Listing]] = []
    skipped = 0
    for listing in priced:
        base = listing.item.base_aspect_roll
        if base is None:
            skipped += 1
            continue
        if args.min_roll is None or base >= args.min_roll:
            ranked.append((base, listing))
    ranked.sort(key=lambda pair: (-pair[0], pair[1].raw_price or 0))

    print(f"{len(listings)} listings, {len(priced)} priced, {len(ranked)} ranked")
    if skipped:
        print(
            f"{skipped} skipped: slot is one- OR two-handed under a single label, "
            + "so the base roll cannot be derived. Not guessed at."
        )
    print()
    for base, listing in ranked[: args.limit]:
        item = listing.item
        raw = item.aspect_roll
        scale = item.aspect_scale or 1.0
        shown = f"{raw:g} shown /{scale:g}x" if scale != 1.0 else f"{raw:g}"
        print(
            f"  base {base:>6.1f}  {listing.price or 'n/a':>8}  "
            + f"{item.equipment_type or '?':16} ({shown})"
        )
        print(f"      https://diablo.trade/listings/{listing.id}")
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
        + "cannot be determined statically - capture one real submission in "
        + "devtools to find out. See docs/posting.md."
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

    enchant = sub.add_parser(
        "enchant", help="rank listings by their value after one enchant"
    )
    enchant.add_argument("search_id")
    enchant.add_argument("--unique", help="restrict to this uniqueEquipmentId")
    enchant.add_argument("--attrs", required=True, help="comma-separated UUIDs")
    enchant.add_argument(
        "--include-unfixable",
        action="store_true",
        help="also show listings one enchant cannot complete",
    )
    enchant.add_argument("--limit", type=int, default=15)
    enchant.set_defaults(func=_cmd_enchant)

    groups = sub.add_parser(
        "groups", help="print the rule as OR-groups for the site UI"
    )
    groups.add_argument("--attrs", required=True, help="comma-separated UUIDs or names")
    groups.add_argument("--min-matches", type=int, required=True)
    groups.set_defaults(func=_cmd_groups)

    aspects = sub.add_parser("aspects", help="rank aspect carriers by base roll")
    aspects.add_argument("search_id")
    aspects.add_argument("--min-roll", type=float, help="base roll floor")
    aspects.add_argument("--max-price", type=int, help="raw gold ceiling")
    aspects.add_argument("--limit", type=int, default=10)
    aspects.set_defaults(func=_cmd_aspects)

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
