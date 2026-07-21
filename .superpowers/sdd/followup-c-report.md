# Follow-up C report: locale-independent weapon subtype detection

## 14-locale alignment verification (Part 1, step 1)

Ran a throwaway Python check (not committed) against all 14 `D4Companion/Data/ItemTypes.*.json`
files, comparing each locale to `ItemTypes.enUS.json` (432 entries) on: entry count, `Type`
field identical at every index, `Rarerity` field identical at every index, and no duplicate
`Name` values within any single locale.

Result: **10 of 14 locales are fully index-aligned**, 4 are not.

- Aligned (432/432, Type+Rarerity identical at every index, 0 duplicate names): deDE, esES,
  esMX, itIT, jaJP, koKR, ruRU, zhCN, zhTW (plus enUS itself as the reference).
- **Not aligned**: frFR (420 entries, 12 short), plPL (424, 8 short), ptBR (424, 8 short),
  trTR (258, 174 short). Each is missing entries partway through the list (not just
  trailing), which shifts every later index out of alignment. The missing entries in
  frFR/plPL/ptBR sit right after the weapon block (around the unsuffixed "Wand" entries);
  trTR diverges almost immediately (index 7).

Per the brief's instruction to stop rather than ship something subtly wrong if any locale
failed alignment: I did not adopt a blanket "trust the whole array" design. Instead
`OcrHandler.InitItemTypeData` verifies alignment (`WeaponTypeResolver.IsIndexAligned`) for
whichever locale is currently loaded, every time it loads, and only uses index-based
classification when that specific locale passes. Locales that fail (frFR, plPL, ptBR, trTR)
transparently fall back to the pre-existing per-name-text parse - i.e. those 4 locales are
unaffected by this change (no better, no worse, no crash) until their ItemTypes JSON is fixed
upstream. This is captured as a permanent regression test
(`WeaponSubtypeLocaleAlignmentTests`), not just a one-off script, so a future data update that
breaks alignment for a currently-good locale is caught immediately instead of silently
regressing.

No duplicate `Name` values were found in any of the 14 locales (verified for enUS previously;
re-verified here for the other 13).

## Part 1: locale-independent weapon subtype detection

- `D4Companion.Helpers/WeaponTypeResolver.cs`: added `BuildSubtypeIndex` (classifies each
  reference-locale entry by position, reusing the existing `FromItemTypeName`) and
  `IsIndexAligned` (count + per-index Type equality check). Both take
  `IEnumerable<(string Name, string Type)>` tuples rather than `ItemTypeInfo` directly, so
  `D4Companion.Helpers` does not need a new project reference to `D4Companion.Entities`
  (it currently has none).
- `D4Companion.Services/OcrHandler.cs` `InitItemTypeData`: when the selected locale is not
  enUS, it loads the enUS reference list (`LoadItemTypes`, a small extracted helper), checks
  alignment, and if aligned builds `_itemTypeMapNameToId` using the enUS-derived subtype at
  the same index instead of parsing the loaded locale's Name text. For enUS itself, and for
  any locale that fails alignment, behavior is byte-for-byte identical to before (calls
  `WeaponTypeResolver.FromItemTypeName` on the locale's own Name).
- `FromItemTypeName` itself is unchanged; all 9 existing `WeaponTypeResolverTests` cases still
  pass, no test deleted or altered.
- Duplicate-key hazard: `_itemTypeMapNameToId` is still keyed on `itemType.Name` from the
  *loaded* locale (never the enUS reference), so the existing "432/432 unique names"
  invariant is what protects `ToDictionary` from throwing, exactly as before. Verified for
  all 14 locales (see above), not just enUS.

New tests, all passing: `D4Companion.Tests/WeaponSubtypeLocaleAlignmentTests.cs`
- Confirms all 14 `ItemTypes.*.json` files exist.
- `IsIndexAligned` returns true for the 9 confirmed-aligned locales, false for the 4
  confirmed-misaligned ones (regression-locks the finding above).
- No duplicate names in any of the 9 aligned locales.
- `DeDeLocale_ResolvesWeaponSubtypesByIndex_MirroringOcrHandler`: reproduces
  `InitItemTypeData`'s exact logic against the real `ItemTypes.deDE.json`/`ItemTypes.enUS.json`
  files and asserts real deDE strings (e.g. `"Legendärer Zweihandstreitkolben (Wuchtwaffe)"` ->
  `WeaponBludgeoning`, `"Legendäre Stangenwaffe (Hiebwaffe)"` (Polearm) -> `WeaponSlicing`,
  `"Legendäre Axt (Hiebwaffe)"` (1H Axe) -> `WeaponOneHand`, unsuffixed names -> plain
  `weapon`). This is testable without OCR or a browser and is the actual proof the fix works
  for a non-English locale.

## Part 2a: D4Builds weapon-subtype provenance

- `D4Companion.Entities/D4BuildsBuild.cs`: added `WeaponBludgeoning`, `WeaponSlicing`,
  `WeaponOneHand` lists on `D4BuildsBuildVariant` alongside the existing `Weapon` list.
- `D4Companion.Services/BuildsManagerD4Builds.cs` (lines ~536-543 as named in the brief,
  confirmed at those lines before editing): the five `GetAllAffixes(...)` calls that used to
  all funnel into one `Weapon` list now route `"BludgeoningWeapon"` ->
  `WeaponBludgeoning`, `"SlashingWeapon"` -> `WeaponSlicing`, `"Dual-WieldWeapon1"` +
  `"Dual-WieldWeapon2"` -> `WeaponOneHand`, `"Weapon"` stays in `Weapon`. Only these five
  lines changed; no Selenium setup, WebDriver lifecycle, CDP, or `Thread.Sleep` call was
  touched.
- `D4Companion.Services/BuildAdapters/D4BuildsBuildAdapter.cs` `ToCanonical`: added three
  `AddSlot` calls for the new lists, mapping to `ItemTypeConstants.WeaponBludgeoning` /
  `WeaponSlicing` / `WeaponOneHand`.

### Old cache file behavior (verified with a test, not asserted from memory)

Added `ToCanonical_OldShapeCachedJson_DeserializesAndKeepsPlainWeaponAffixes`: deserializes a
JSON string shaped like a pre-change cached build (only `Name` and `Weapon`, no
`WeaponBludgeoning`/`WeaponSlicing`/`WeaponOneHand` keys at all) via
`System.Text.Json.JsonSerializer.Deserialize<D4BuildsBuildVariant>`. Result: **no exception**,
missing properties default to their initializer (`new()`, i.e. empty list), the existing
`Weapon` affix data survives untouched, and the adapter produces no
`WeaponBludgeoning`/`WeaponSlicing`/`WeaponOneHand` canonical items (empty list ->
`AddSlot` returns early). So: no crash, no data loss for old cached builds. They simply do not
retroactively gain the subtype split - that requires re-downloading from D4Builds, since the
distinction only exists in the page's DOM structure at scrape time, not in the previously
cached JSON.

Also added `ToCanonical_WeaponSubtypeLists_MapToDistinctCanonicalSlots` proving the new lists
land on distinct canonical slots with the plain `Weapon` list still separate.

## Part 2b: D2Core weapon-class mapping

Full arm enumeration confirmed by reading the entire switch in
`D4Companion.Services/BuildsManagerD2Core.cs` (lines ~317-330): `Dagger`, `Glaive`, `Mace`,
`Mace2H`, `Polearm`, `Quarterstaff`, `Scythe`, `Scythe2H`, `Staff`, `Sword`, `Sword2H`, `Wand`
- 12 arms total, not fewer.

Updated only `D4Companion.Services/BuildAdapters/D2CoreBuildAdapter.cs` `ResolveSlot` (this is
what actually produces `CanonicalItem.Slot`, which flows to the projector and the resulting
preset). Classified:

- `Mace2H` -> `WeaponBludgeoning`
- `Sword2H`, `Polearm` -> `WeaponSlicing`
- `Mace`, `Sword` -> `WeaponOneHand`

Grounding: `Mace`/`Mace2H` and `Sword`/`Sword2H` are paired class names within this same
switch (`X` / `X2H`), so handedness is structurally evidenced, not guessed; damage type
(mace=blunt, sword=edged) mirrors the pre-existing, already-verified
`WeaponTypeResolver.MaxrollPrefixMap` (`2HMace`->Bludgeoning, `2HSword`->Slicing,
`1HMace`/`1HSword`->OneHand). `Polearm` is two-handed/slashing per the brief's explicit,
separately-confirmed statement (same as `WeaponTypeResolver`'s own `TwoHandedWithoutPrefix`
handling).

**Left as plain `Weapon` - ambiguous, not guessed**: `Dagger`, `Glaive`, `Quarterstaff`,
`Scythe`, `Scythe2H`, `Staff`, `Wand`. None of these has a paired `X`/`X2H` class name in the
switch to evidence handedness, and none is referenced anywhere else in the codebase (no
`MaxrollPrefixMap` entry, no brief statement) to confirm damage type. In particular `Scythe`
does have a `Scythe2H` counterpart suggesting a pairable pattern, but I chose not to extend
the sword/edge-weapon inference to it without a source in the repo to check against, per the
brief's explicit "do not guess" instruction - flagging it here rather than silently deciding.
If real game data confirms these, extending `ResolveSlot` is a small follow-up.

`BuildsManagerD2Core.cs`'s own local switch (lines ~304-357) was intentionally **not**
modified: its `itemType` local variable is assigned per-case but never read afterward except
to decide the `continue`/warning path for genuinely unknown item types (the item's actual
type information downstream comes entirely from `canonicalItem.Slot`, already resolved by the
adapter). The invariant comment there ("adapter and this manager must skip identical item
types") only requires the two switches recognize the same *set* of cases, which is unchanged
- I verified this by reading the full switch and confirming no case labels were added,
removed, or renamed.

Updated `D4Companion.Tests/D2CoreBuildAdapterTests.cs`
`ToCanonical_AllKnownItemTypes_ResolveToExpectedSlot` to expect the new subtype slots for
`Mace`/`Mace2H`/`Sword`/`Sword2H`/`Polearm` and left the other 7 arms expecting plain `Weapon`,
matching the classification above. This is a deliberate, documented change to reflect the new
intended behavior, not a silent test deletion.

## AffixManager.IsTypeMatch / backward compatibility

Not modified. Confirmed by reading `D4Companion.Services/AffixManager.cs` lines ~605-627: the
symmetric supertype match (plain `weapon` matches any subtype and vice versa, only two
different subtypes fail to match) is exactly as described and untouched. This is what keeps
existing `Config/AffixPresets-v2.json` entries typed plain `"weapon"` matching correctly
against the new subtype-typed entries produced by Part 1/2, with no version field or
migration needed.

## Line-number mismatches found vs. the brief

- `OcrHandler.cs` `_itemTypeMapNameToId` construction: brief said lines 730-750; actual
  location in this worktree was lines 713-753 (`InitItemTypeData` method, dictionary build at
  ~744-749). Same code, different line numbers.
- `BuildsManagerD4Builds.cs` weapon `GetAllAffixes` calls: brief said 536-541; actual was
  536-543 (six lines, not five - includes the `Distinct()` call and the `RangedWeapon` line
  immediately after). Content matched otherwise.
- `BuildsManagerD2Core.cs` weapon-class switch: brief said "near lines 300-353"; actual switch
  body is lines 304-357 (`CreatePresetFromD2CoreBuild`), with the specific weapon-class cases
  at 317-330. Close, not exact.
- deDE example: brief said index 27 is `"Legendare Axt (Hiebwaffe)"` (typo, missing umlaut) -
  actual string at index 27 is `"Legendäre Axt (Hiebwaffe)"`. Confirmed the underlying
  index-alignment claim is correct; only the brief's transcription of the umlaut was off.

## What I could not verify

- Real in-game/official confirmation of handedness or damage type for Dagger, Glaive,
  Quarterstaff, Scythe, Scythe2H, Staff, Wand (D2Core weapon classes) - no such reference
  exists anywhere in this codebase. Left as plain `Weapon`, as stated above.
- Whether frFR/plPL/ptBR/trTR's `ItemTypes.*.json` files will ever be fixed upstream to
  restore alignment - out of scope for this change; the fallback path means those locales
  are simply unaffected (not regressed) either way.

## Verification run

- Build: `dotnet build D4Companion.sln -c Release` -> Build succeeded, 1 warning (CA1416,
  `ScreenCaptureHandler.cs:293`, pre-existing), 0 errors.
- Tests: `dotnet test D4Companion.Tests/D4Companion.Tests.csproj -c Release` -> 106 passed,
  10 failed, 116 total. The 10 failures are exactly the pre-existing `*ScorerTest` set
  (`DefaultRatioScorerTest`, `DefaultRatioScorerTestzhCN`, `PartialRatioScorerTest`,
  `PartialTokenAbbreviationScorerTest`, `PartialTokenSetScorerTest`,
  `PartialTokenSortScorerTest`, `TokenAbbreviationScorerTest`, `TokenSetScorerTest`,
  `TokenSortScorerTest`, `WeightedRatioScorerTest`), confirmed by name. No previously-passing
  test broke.
