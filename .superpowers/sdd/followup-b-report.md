# Follow-up B: fix manual-add aspect fan-out

## What changed

1. `D4Companion.Interfaces/IAffixManager.cs`: `AddAspect(AspectInfo, string)` ->
   `AddAspect(AspectInfo aspectInfo, string itemType, bool isAnyType = false)`.
   Optional parameter defaulting to `false`, so no other implementer or caller needed
   to change behaviour.
2. `D4Companion.Services/AffixManager.cs`, `AddAspect`: takes the new `isAnyType`
   parameter, sets `IsAnyType = isAnyType` on the created `ItemAffix`, and switches the
   dedup check to match on `Id` alone (ignoring `Type`) when `isAnyType` is true, since
   an any-type entry has no per-slot identity to dedup against.
3. `D4Companion/ViewModels/AffixViewModel.cs`, `SetAspectExecute`: replaced the ten
   `_affixManager.AddAspect(aspectInfo.Model, ItemTypeConstants.<Slot>)` calls with one
   call: `_affixManager.AddAspect(aspectInfo.Model, ItemTypeConstants.Weapon, isAnyType: true)`.
   `Weapon` is used as the placeholder `Type` for the same reason `BuildPresetProjector`
   uses it for D4Builds/Mobalytics imports (`SlotIsKnown = false`): it drives the
   mainhand icon and is otherwise inert once `IsAnyType` is set.
4. `D4Companion.Tests/BuildsManagerMaxrollTests.cs`: updated the `AffixManagerStub`'s
   `AddAspect` signature to match the new interface member (build-breaking otherwise;
   no behaviour change, the stub still throws).
5. Added `D4Companion.Tests/AffixManagerAddAspectTests.cs` (6 new tests, see below).

## Line-number check

The prompt's line numbers were approximate. Actual, before edit: `SetAspectExecute` was
lines 853-868 in `AffixViewModel.cs` (ten `AddAspect` calls at 857-866), not 856-867.
Off by a few lines but same method, no other mismatch found.

## Investigation findings (points 1-4)

1. **AffixManager.AddAspect could not express `IsAnyType` before this change.** It only
   ever set `Id`, `Type`, `Color` on the new `ItemAffix`; `IsAnyType` defaulted to
   `false` from the entity. Confirmed no caller of `AddAspect` anywhere in the repo
   passed slot-provenance-free data before this change - the importers
   (`BuildPresetProjector.AddAspects`) never call `AddAspect` at all, they build
   `ItemAffix` objects directly and set `IsAnyType = !item.SlotIsKnown` themselves.
   Extended with an optional parameter (default `false`) rather than changing the
   existing signature, per the instruction.

2. **AffixManager.GetAspect already matches IsAnyType entries, confirmed by reading it.**
   It tries (1) exact/weapon-supertype slot match, then (2)
   `preset.ItemAspects.FirstOrDefault(a => a.Id.Equals(aspectId) && a.IsAnyType)`
   regardless of the stored `Type`, then (3) an off-slot fallback. Step 2 is exactly
   what resolves the new single entry on every slot. Verified end-to-end with a test
   (`AddAspect_IsAnyType_ResolvesOnEverySlotViaGetAspect`) rather than trusting the
   reading.

3. **RemoveAspect and other mutators.** `RemoveAspect` already does
   `preset.ItemAspects.RemoveAll(a => a.Id.Equals(itemAffix.Id))` - it matches by `Id`
   alone, with no assumption about how many entries share that `Id`. It needed no
   change: it already correctly removes ten legacy per-slot entries (old behaviour) and
   now correctly removes the single any-type entry (new behaviour). Both are covered by
   tests. `SetAspectColorExecute`/`ItemAffixVM` color-set logic also matches by `Id`
   alone across `_selectedAspects`, so it is unaffected either way.
   `FilterSelectedAspects` (the dedup view filter) dedups by `Id` against the first
   matching entry in the backing list, also `Type`-independent, so it keeps working for
   both legacy multi-entry presets and the new single-entry case; it simply becomes a
   no-op for aspects added after this fix, as the prompt anticipated.

   One related but out-of-scope gap found by inspection, not touched: in
   `ScreenProcessHandler.cs` (`SetMultiBuildMode`, around line 1831) the "multi-build"
   comparison overlay matches aspects by exact `Id` + `Type` with no `IsAnyType`
   fallback, unlike the parallel affix-matching path a few lines above it (1794-1799)
   which does have the fallback. This predates this change and already affects
   IsAnyType aspects coming from the D4Builds/Mobalytics importers, so it is a
   pre-existing asymmetry in a feature this task did not ask me to touch, not a
   regression from this fix. Flagging for a separate follow-up.

4. **Existing user presets with ten manually-added entries are not touched or migrated.**
   `AddAspect`'s new `isAnyType` branch only changes behaviour for future calls with
   `isAnyType: true`; old ten-entries-per-aspect data on disk is read, displayed, and
   removed exactly as before (verified by
   `RemoveAspect_StillRemovesAllLegacyPerSlotEntries`). No version field was added to
   `AffixPresets-v2.json` and none was needed, since nothing in the read or write path
   depends on one.

## Test note

`AffixManager`'s only dependencies are `ILogger<AffixManager>` (satisfied with
`NullLogger<AffixManager>.Instance`) and `ISettingsManager` (a 3-member interface, given
a trivial in-memory fake) - lighter than expected, so a real `AffixManager` instance was
constructed rather than skipping the test. One footgun found and worked around: the
constructor unconditionally loads `Config/AffixPresets-v2.json` from the working
directory if present, and `AddAspect` writes it back via `SaveAffixPresets`, so a preset
saved by one test was being loaded by the next `AffixManager` instance and silently
duplicating the "test-preset" name, corrupting assertions. Fixed with a `[SetUp]` that
deletes that file before each test. `Data/Aspects.*.json` (also loaded unconditionally,
with no try/catch, unlike some sibling `Init*Data` methods) is already present in the
test output directory via existing project wiring, so no fixture setup was needed for
that part.

## Could not verify

- Runtime UI behaviour (clicking "add aspect" in the actual running app) was not
  exercised; verification is at the `AffixManager`/`AffixViewModel` unit level plus the
  full solution build/test gate.
