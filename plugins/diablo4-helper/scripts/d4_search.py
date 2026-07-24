#!/usr/bin/env python3
# /// script
# requires-python = ">=3.12"
# ///
"""D4-scoped tavily-dynamic-search helper.

The tavily-dynamic-search pattern (search/extract inside a Python process so raw
guide HTML never enters the model's context) pre-configured with Diablo 4's
authoritative domains and D4-aware result filtering. Only what this prints
crosses into context - typically a few hundred tokens of triaged signal instead
of hundreds of KB of guide boilerplate.

Run via `uv run` (bare python3 is blocked on this machine):

    uv run python3 d4_search.py "season 14 whirlwind gear priority"
    uv run python3 d4_search.py "how to craft grandfather" --extract
    uv run python3 d4_search.py "current season name" --official

Flags:
    --extract        Pull and filter the top result pages (default: triage only).
    --official       Restrict to Blizzard news/forums (best for "is this current").
    --max-results N  Result count (default 8).
    --top N          With --extract, how many pages to fetch (default 3).

Requires the `tvly` CLI on PATH (Tavily). Prints nothing sensitive.
"""

import argparse
import json
import subprocess
import sys

type Result = dict[str, object]

# Authoritative D4 sources, mirrored from references/sources.md.
BUILD_ITEM_DOMAINS = [
    "maxroll.gg",
    "mobalytics.gg",
    "icy-veins.com",
    "diablo4.wiki.fextralife.com",
    "game8.co",
    "news.blizzard.com",
]
OFFICIAL_DOMAINS = ["news.blizzard.com", "us.forums.blizzard.com"]

# Lines worth keeping when filtering an extracted page.
D4_KEYWORDS = [
    "season",
    "patch",
    "hotfix",
    "unique",
    "mythic",
    "aspect",
    "temper",
    "masterwork",
    "cube",
    "horadric",
    "charm",
    "talisman",
    "glyph",
    "pit",
    "affix",
    "stat",
    "priority",
    "greater",
    "ancestral",
    "craft",
    "material",
    "drop",
    "farm",
    "vulnerable",
    "multiplier",
    "damage",
    "resistance",
]


def run_tvly(args: list[str], timeout: int) -> bytes | None:
    try:
        return subprocess.check_output(["tvly", *args], stderr=subprocess.DEVNULL, timeout=timeout)
    except FileNotFoundError:
        sys.exit("tvly CLI not found. Install: curl -fsSL https://cli.tavily.com/install.sh | bash && tvly login")
    except subprocess.TimeoutExpired:
        return None
    except subprocess.CalledProcessError:
        return None


def search(query: str, domains: list[str], max_results: int, with_raw: bool) -> list[Result]:
    args = ["search", query, "--max-results", str(max_results), "--json", "--include-domains", ",".join(domains)]
    if with_raw:
        args += ["--include-raw-content", "markdown"]
    raw = run_tvly(args, timeout=45)
    if not raw:
        return []
    try:
        parsed: object = json.loads(raw)
    except json.JSONDecodeError:
        return []
    results = parsed.get("results", []) if isinstance(parsed, dict) else []
    return [r for r in results if isinstance(r, dict)] if isinstance(results, list) else []


def _score(r: Result) -> float:
    s = r.get("score", 0)
    return float(s) if isinstance(s, (int, float)) else 0.0


def _str(r: Result, key: str) -> str:
    v = r.get(key, "")
    return v if isinstance(v, str) else ""


def triage(results: list[Result]) -> None:
    for i, r in enumerate(sorted(results, key=_score, reverse=True)):
        print(f"[{i}] [{_score(r):.2f}] {_str(r, 'title')[:95]}")
        print(f"    {_str(r, 'url')}")
        snippet = _str(r, "content").strip().replace("\n", " ")
        if snippet:
            print(f"    {snippet[:200]}")
        print()


def extract_and_filter(results: list[Result], top: int) -> None:
    ranked = sorted(results, key=_score, reverse=True)[:top]
    for r in ranked:
        url = _str(r, "url")
        content = _str(r, "raw_content")
        if not content:
            raw = run_tvly(["extract", url, "--json"], timeout=45)
            if raw:
                try:
                    parsed: object = json.loads(raw)
                except json.JSONDecodeError:
                    parsed = None
                res = parsed.get("results", []) if isinstance(parsed, dict) else []
                first = res[0] if isinstance(res, list) and res else None
                if isinstance(first, dict):
                    content = _str(first, "raw_content")
        if not content:
            continue
        print(f"#### {_str(r, 'title')[:95]}")
        print(f"URL: {url}\n")
        for line in content.split("\n"):
            s = line.strip()
            if len(s) > 40 and any(k in s.lower() for k in D4_KEYWORDS):
                print(f"  {s[:300]}")
        print("\n---\n")


def main() -> None:
    ap = argparse.ArgumentParser(description="D4-scoped tavily search helper")
    ap.add_argument("query")
    ap.add_argument("--extract", action="store_true")
    ap.add_argument("--official", action="store_true")
    ap.add_argument("--max-results", type=int, default=8)
    ap.add_argument("--top", type=int, default=3)
    a = ap.parse_args()
    query: str = a.query
    official: bool = a.official
    extract: bool = a.extract
    max_results: int = a.max_results
    top: int = a.top

    domains = OFFICIAL_DOMAINS if official else BUILD_ITEM_DOMAINS
    results = search(query, domains, max_results, with_raw=extract)
    if not results:
        print("No results. Try reformulating, dropping --official, or widening the query.")
        return
    if extract:
        extract_and_filter(results, top)
    else:
        triage(results)


if __name__ == "__main__":
    main()
