## Task 7 report: Mobalytics adapter (Wave B)

### Status: Complete

### Discrepancy from instructions (report, not silently worked around)

The task said to read the brief at `/mnt/c/Users/root/code/d4c-wt7/.superpowers/sdd/task-7-brief.md`. That path does not exist in the wt7 worktree - the `.superpowers/` directory was never checked out or created there. The brief only exists in the main worktree at `/mnt/c/Users/root/code/Diablo4Companion/.superpowers/sdd/task-7-brief.md`. I read it there (read-only, no writes to that worktree) since no other copy exists anywhere under `/mnt/c/Users/root/code`. Same story for this report file's target directory: `.superpowers/sdd/` did not exist in wt7, so I created it before writing this report.

### Verification of the brief's assumed names (the brief itself flagged `AffixText` as uncertain)

Checked every name against the real source rather than transcribing blindly:

- `D4Companion.Entities/MobalyticsBuild.cs`: `MobalyticsBuildVariant` has exactly the ten per-slot properties the brief assumed (`Helm, Chest, Gloves, Pants, Boots, Amulet, Ring, Weapon, Ranged, Offhand`, all `List<MobalyticsAffix>`), plus `Aspect`, `Uniques`, `Runes` (all `List<string>`) and `ParagonBoards` (`List<ParagonBoard>`). All confirmed present and correctly named.
- `MobalyticsAffix`: `AffixText`, `IsGreater`, `IsImplicit`, `IsTempered` all exist exactly as named in the brief. The brief's flagged uncertainty about `AffixText` turned out to be unfounded - the name is correct.
- `D4Companion.Entities/Canonical/CanonicalBuild.cs`: `CanonicalItem.Slot`, `SlotIsKnown`, `Affixes`, `AspectIds`, `UniqueIds`, `RuneIds`; `CanonicalVariant.Name`, `Items`, `ParagonBoards` (`List<ParagonBoard>`, type matches source); `CanonicalAffix.Id`, `IsGreater`, `IsImplicit`, `IsTempered` - all match the brief's code exactly.
- `D4Companion.Constants/ItemTypeConstants.cs`: `Helm, Chest, Gloves, Pants, Boots, Amulet, Ring, Weapon, Ranged, Offhand` all present as string constants.
- Read `ConvertBuildVariants` in `BuildsManagerMobalytics.cs` (lines 519-656): confirmed this is exactly the bug being fixed - lines 611-623 today fan every resolved aspect id out onto all ten `ItemTypeConstants` slots unconditionally. The new adapter's single `SlotIsKnown = false` item replaces that fan-out.
- Read `D4Companion.Services/BuildPresetProjector.cs` and `D4Companion.Tests/BuildPresetProjectorTests.cs`: confirmed the projector sets `Type = item.Slot` regardless of `SlotIsKnown`, and drives `IsAnyType` purely off `!item.SlotIsKnown`. There is already a projector test (`Project_UnknownSlotAspect_EmitsSingleAnyTypeEntry`) using `Slot = ItemTypeConstants.Weapon` with `SlotIsKnown = false`, confirming `Weapon` as the placeholder slot value on the unslotted item is an established, intentional pattern, not an arbitrary guess.

No property-name mismatches found. The brief's code was accurate as written; I typed it into the file myself after independently verifying each name against the real entities rather than trusting the brief.

### Files touched

- Created `D4Companion.Services/BuildAdapters/MobalyticsBuildAdapter.cs` (as specified, brief's design followed exactly - pure function, no DI, no `IAffixManager`, no fuzzy matching, `CanonicalAffix.Id` holds raw `AffixText`).
- Created `D4Companion.Tests/MobalyticsBuildAdapterTests.cs` - 6 tests, no fixture existed so these construct `MobalyticsBuildVariant` objects directly in-test: variant/build name passthrough, aspects landing on a single unslotted item (regression guard for the original all-ten-slots defect), uniques/runes sharing that same unslotted item, slotted affix flag passthrough, empty slot lists producing no item, paragon boards passthrough.

Did not touch `InitDevTools`, `DownloadMobalyticsBuild`, the Profiles chain, Selenium setup, Chrome lifecycle, or any sleep. Did not touch `ConvertBuildVariants` itself - read-only for verification, per instructions.

### Build / test results

- `dotnet build D4Companion.sln -c Release`: **Build succeeded**, 1 warning (pre-existing `CA1416` at `ScreenCaptureHandler.cs:293`), 0 errors. No new warnings.
- `dotnet test D4Companion.Tests` filtered to `MobalyticsBuildAdapterTests`: 6/6 passed.
- Full suite: 40 passed, 10 failed, 50 total. All 10 failures are `WeightedRatioScorerTest` in `FuzzierSharpTests.cs` (pre-existing, matches the gate's stated known-red baseline exactly). No new failures introduced.

### Commit

Committed in wt7 on branch `wt/task-7`. See git log for SHA.
