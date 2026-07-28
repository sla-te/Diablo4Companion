"""Generate the tempering-affixes reference from D4Companion's shipped game data.

The source is `D4Companion/Data/Affixes.<locale>.json`, which D4Companion builds
from the extracted client data in DiabloTools/d4data. Every field used here comes
straight out of the game files, so the generated reference is authoritative in a
way scraped guide text is not.

Regenerate after a D4Companion data update:

    uv run python3 plugins/diablo4-helper/scripts/gen_temper_reference.py

Deliberately reads only `IsTemperingAvailable`, `Description`,
`AllowedForPlayerClass` and `AffixAttributes[].LocalisationId`. Two neighbouring
fields are NOT used and should not be added without new evidence:

* `AllowedItemLabels` - an opaque id space (observed max 71) that does not index
  into `ItemTypes.<locale>.json` (rings live at index 288+). D4Companion declares
  the field but never resolves it. The key lives in josdemmers/D4DataParser.
* `Category` - 7 values that superficially resemble Tuning Prism groups, but 614
  of 893 affixes sit in category 0, which holds both Attack Speed and Total
  Armor. Those belong to different prisms, so the mapping does not hold.
"""

from __future__ import annotations

import argparse
import json
import sys
from collections.abc import Callable
from pathlib import Path
from typing import TypedDict, cast


class AffixAttribute(TypedDict, total=False):
    """One stat entry on an affix. Only the fields this script reads."""

    LocalisationId: str


class Affix(TypedDict, total=False):
    """One affix record. Only the fields this script reads."""

    IsTemperingAvailable: bool
    Description: str
    IdName: str
    AllowedForPlayerClass: list[int]
    AffixAttributes: list[AffixAttribute]


# Index into AllowedForPlayerClass. Taken from D4Companion's own
# AffixViewModel.cs, which tests these positions by name.
CLASS_NAMES: tuple[str, ...] = (
    "Sorcerer",
    "Druid",
    "Barbarian",
    "Rogue",
    "Necromancer",
    "Spiritborn",
    "Paladin",
    "Warlock",
)

DEFAULT_SOURCE = Path("D4Companion/Data/Affixes.enUS.json")
DEFAULT_OUTPUT = Path("plugins/diablo4-helper/skills/diablo4-helper/references/tempering-affixes.md")


def repo_root() -> Path:
    """Resolve the repository root from this script's location."""
    return Path(__file__).resolve().parents[3]


def stat_keys(affix: Affix) -> str:
    """Join the internal localisation ids that identify an affix's stat."""
    ids = [attr.get("LocalisationId", "") for attr in affix.get("AffixAttributes") or [] if attr.get("LocalisationId")]
    return ", ".join(f"`{i}`" for i in ids) if ids else "-"


MULTI_CLASS = "Two or more classes (not all)"
EMPTY_MASK = "Class mask empty in the data - verify in game"


def class_bits(affix: Affix) -> list[int]:
    """Return the indices of classes an affix is available to.

    Raises if the mask is not the expected width. A silent shape change - a
    ninth class shipping - would otherwise either crash on an index or, worse,
    quietly misgroup every all-class affix.
    """
    mask = affix.get("AllowedForPlayerClass") or []
    if len(mask) != len(CLASS_NAMES):
        msg = (
            f"AllowedForPlayerClass has {len(mask)} entries, expected "
            f"{len(CLASS_NAMES)}. Update CLASS_NAMES from AffixViewModel.cs."
        )
        raise ValueError(msg)
    return [i for i, bit in enumerate(mask) if bit == 1]


def classes_of(affix: Affix) -> str:
    """Return the class names an affix is available to, slash-joined."""
    return " / ".join(CLASS_NAMES[i] for i in class_bits(affix))


def internal_id(affix: Affix) -> str:
    """Return the affix's internal IdName, backticked for a markdown cell."""
    return f"`{affix.get('IdName', '')}`"


def group_of(affix: Affix) -> str:
    """Return the section an affix belongs to.

    Decided on the mask, not on the rendered string, so that widening
    CLASS_NAMES cannot silently reclassify anything.

    Single-class affixes get their own section because that is how a player
    reads this - "what can my class temper". Everything reachable by more than
    one class but not all collapses into one section, which would otherwise
    fragment into a dozen one-row tables.
    """
    bits = class_bits(affix)
    if not bits:
        return EMPTY_MASK
    if len(bits) == len(CLASS_NAMES):
        return "All classes"
    if len(bits) == 1:
        return CLASS_NAMES[bits[0]]
    return MULTI_CLASS


def render(affixes: list[Affix], source: Path) -> str:
    """Render the reference markdown."""
    temperable = [a for a in affixes if a.get("IsTemperingAvailable")]
    groups: dict[str, list[Affix]] = {}
    for affix in temperable:
        groups.setdefault(group_of(affix), []).append(affix)

    # All classes first, then single classes in CLASS_NAMES order, then the rest.
    def sort_key(name: str) -> tuple[int, int, str]:
        if name == "All classes":
            return (0, 0, "")
        if name in CLASS_NAMES:
            return (1, CLASS_NAMES.index(name), "")
        return (2, 0, name)

    out: list[str] = [
        "# Temperable affixes (generated)",
        "",
        "**Do not hand-edit.** Regenerate with",
        "`uv run python3 plugins/diablo4-helper/scripts/gen_temper_reference.py`.",
        "",
        "`[Certain]` Every row is an affix whose `IsTemperingAvailable` flag is set in",
        f"`{source.as_posix()}`, which D4Companion builds from the extracted client",
        "data in [DiabloTools/d4data](https://github.com/DiabloTools/d4data) (MIT).",
        "This is game data, not scraped guide text - if a guide disagrees about",
        "whether an affix can be tempered, this file wins.",
        "",
        f"{len(temperable)} temperable affixes out of {len(affixes)} total.",
        "",
        "## What this file does and does not answer",
        "",
        "- **Answers:** whether an affix can appear as a tempering option at all, and",
        "  which classes can roll it.",
        "- **Does not answer:** which tempering *manual* grants it, which item slots",
        "  accept it, or its value range. The `#` in each row is the game's own",
        "  placeholder - the source data carries no numeric ranges.",
        "- **Does not answer:** Tuning Prism category. See the header comment in the",
        "  generator for why the `Category` field cannot be trusted for that.",
        "",
        "Remember the budget from `SKILL.md`: an item carries exactly **one**",
        "tempering affix. `Tempers: X/Y` counts reroll attempts, not slots.",
        "",
        f"A `{EMPTY_MASK}` section holds affixes whose class mask is all zeroes.",
        "`[Unverified]` **Do not read that as either universal or unobtainable.**",
        "D4Companion's own UI (`AffixViewModel.cs`) buckets an all-zero mask with",
        "all-one and shows it to everyone, but the internal id of at least one such",
        "affix names a specific class and tier, which contradicts that. The id is",
        "printed alongside each row so the contradiction is visible rather than",
        "hidden behind a verdict. Confirm in game before acting on one.",
        "",
        "The stat key is the affix's internal `LocalisationId`. Use it to correlate",
        "with diablo.trade attribute ids and with build-planner exports, which name",
        "stats inconsistently in prose but agree on these keys.",
        "",
    ]

    for name in sorted(groups, key=sort_key):
        rows = sorted(groups[name], key=lambda a: a.get("Description", ""))
        # The collapsed multi-class table has to name its classes; every other
        # section's heading already does. The empty-mask section shows the
        # internal id instead, because that id is the only evidence available
        # about who can actually roll the affix.
        extra: tuple[str, Callable[[Affix], str]] | None = (
            ("Classes", classes_of)
            if name == MULTI_CLASS
            else ("Internal id", internal_id)
            if name == EMPTY_MASK
            else None
        )
        out.append(f"## {name} ({len(rows)})")
        out.append("")
        header = ["Affix", *([extra[0]] if extra else []), "Stat key"]
        out.append("| " + " | ".join(header) + " |")
        # Spaces around the dashes: a bare `|---|` delimiter is not a valid
        # compact-style table row and trips markdownlint MD060.
        out.append("|" + " --- |" * len(header))
        for affix in rows:
            cells = [
                affix.get("Description") or "",
                *([extra[1](affix)] if extra else []),
                stat_keys(affix),
            ]
            # Escape every cell, not just the description - a future stat key or
            # id containing a pipe would otherwise shift the whole row. Strip
            # too: some descriptions carry trailing spaces in the source data,
            # which break compact table style (MD060).
            out.append("| " + " | ".join(c.strip().replace("|", "\\|") for c in cells) + " |")
        out.append("")

    return "\n".join(out)


def main() -> int:
    """Entry point."""
    root = repo_root()
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--source",
        type=Path,
        default=root / DEFAULT_SOURCE,
        help=f"Affix data to read (default: {DEFAULT_SOURCE})",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=root / DEFAULT_OUTPUT,
        help=f"Reference file to write (default: {DEFAULT_OUTPUT})",
    )
    args = parser.parse_args()
    source = cast("Path", args.source)
    output = cast("Path", args.output)

    if not source.is_file():
        print(f"source not found: {source}", file=sys.stderr)
        return 1

    affixes = cast("list[Affix]", json.loads(source.read_text(encoding="utf-8")))
    rel = source.relative_to(root) if source.is_relative_to(root) else source
    # newline="" suppresses platform translation, so regenerating on Linux and
    # on Windows produces byte-identical output.
    with output.open("w", encoding="utf-8", newline="") as handle:
        _ = handle.write(render(affixes, rel))
    print(f"wrote {output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
