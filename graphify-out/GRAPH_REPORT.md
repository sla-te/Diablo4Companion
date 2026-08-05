# Graph Report - Diablo4Companion  (2026-08-05)

## Corpus Check
- 453 files · ~3,046,756 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 3296 nodes · 5458 edges · 227 communities (199 shown, 28 thin omitted)
- Extraction: 96% EXTRACTED · 4% INFERRED · 0% AMBIGUOUS · INFERRED: 205 edges (avg confidence: 0.76)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `5920ba4e`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- UserControl
- UserControl
- BuildsManagerMobalytics
- AffixViewModel
- WeaponTypeResolver
- .Project
- OverlayHandler.cs
- BuildsManagerD4Builds
- UserControl
- ItemAffix
- UserControl
- SystemPresetManager
- BuildsManagerD2Core
- AffixManager
- FileNameToImagePathConverter.cs
- D4Companion.Entities
- BuildsManagerInfinityBuilds
- UserControl
- UserControl
- ImportAffixPresetViewModel
- UserControl
- IAffixManager
- D4Companion.Interfaces
- BoolConverter
- .Resolve
- ScreenCaptureHandler
- ScreenProcessMessages.cs
- SetAffixViewModel
- ScreenProcessHandler
- MaxrollBuildJson.cs
- UserControl
- OcrHandler
- ImportAffixPresetView
- SettingsManager
- FuzzierSharpTests
- SetAffixColorViewModel
- SettingsViewModel
- AddTradeItemViewModel
- ISystemPresetManager
- MainWindowViewModel
- UserControl
- MobalyticsProfileJson.cs
- UserControl
- UserControl
- TradeItem
- LoggingViewModel
- UserControl
- Image
- SigilInfo
- AffixManagerMessages.cs
- DebugViewModel
- MetroWindow
- OcrResultAffix
- WeaponGroupComparerTests
- SigilInfoVM.cs
- UserControl
- UserControl
- InfinityBuildsBuild
- OverlayHandlerMessages.cs
- MobalyticsDownloadViewModel
- .FindBestAspectMatch
- UserControl
- UserControl
- MobalyticsBuild
- TradeViewModel
- D2CoreDownloadViewModel
- MainWindowViewModel
- UserControl
- UserControl
- MultiBuildConfigViewModel
- AffixViewModelMessages.cs
- D4BuildsDownloadViewModel
- InfinityBuildsDownloadViewModel
- LiveOcrTooltipTests
- UserControl
- UserControl
- UserControl
- MultiBooleanToVisibilityConverter
- D2CoreBuild
- MaxrollBuild
- ScreenCaptureMessages.cs
- UserControl
- UserControl
- App
- SetPresetNameViewModel
- ISettingsManager
- KeyBindingMessages.cs
- SettingsMessages.cs
- D4Companion.Services.csproj
- .ConvertToItemType
- AffixManagerAddAspectTests
- UserControl
- UserControl
- UserControl
- UserControl
- AffixView
- UniqueInfo
- D4Companion.Updater.Interfaces
- D4Companion.csproj
- DownloadManager
- TradeItemWanted
- IComparer
- .ButtonDone_Click
- AspectInfo
- SelectAffixColorViewModel
- Diablo IV Companion
- D4Companion.Messages
- RegexTests
- Diablo IV Companion
- AffixInfoWanted
- .UpdateAffixPresets
- SetAffixTypeColorViewModel
- D4Companion.Helpers.csproj
- AffixConfigViewModel
- ValueChangedMessage
- D4Companion.Tests
- HttpClientHandler
- AffixIdToDescriptionConverterForOcrResults
- D4Companion.sln
- AffixDuplicateToSolidBrushConverter
- RenamePresetNameViewModel
- UserControl
- HotkeyConfigViewModel
- OverlayConfigViewModel
- SigilConfigViewModel
- App
- IValueConverter
- AffixIdToRuneDescriptionConverter
- AffixIdToRuneNameConverter
- D4Companion.Converters
- AffixTypeToFgSolidBrushConverter
- AspectIdToDescriptionConverter
- AspectIdToNameConverter
- BooleanToVisibilityConverter
- ColorToSolidBrushConverter
- FileNameToFileNameNoExtConverter
- FlagToImagePathConverter
- InverseBooleanConverter
- InverseBooleanToVisibilityConverter
- LanguageReadyBoolToOpacityConverter
- Resources
- RankToVisibilityConverter
- RuneIdToDescriptionConverter
- RuneIdToNameConverter
- SigilIdToDescriptionConverter
- SigilIdToNameConverter
- SystemPresetStatusToHealthConverter
- UniqueIdToDescriptionConverter
- UniqueIdToNameConverter
- WeaponTypeToGroupLabelConverter
- AffixGlobal.cs
- D4Companion.Entities.csproj
- AspectConfigViewModel
- DrawGraphicsAffixesMulti
- LocExtension.cs
- UniqueConfigViewModel
- TextBlock
- Window
- .AddRange
- TranslationSource
- ReleaseManagerMessages.cs
- ObjectPool
- Frequently-asked-questions.md
- settings.json
- Follow-up C report: locale-independent weapon subtype detection
- OcrResult
- RuneInfoVM.cs
- D4Companion.Constants
- HamburgerMenuControl
- NotifyIcon
- HOcrClasses.cs
- Inventory.cs
- .Resolve
- ItemTypeDescriptor.cs
- NativeMethods.json
- HotkeysConfigViewModel
- TextBoxFilterAffixWatermark
- AffixLanguage.cs
- BuildImportWebsite.cs
- IOverlayHandler.cs
- IScreenProcessHandler
- .HotkeyManager_HotkeyAlreadyRegistered
- flash
- FlipView1st
- How-to-create-a-new-System-Preset.md
- Frequently-asked-questions-(esES).md
- How-to-create-a-new-System-Preset-(esES).md
- .FindItemAspect
- AffixPreset
- D4Companion - Claude Code guidance
- RuneConfigViewModel
- TooltipDataReadyMessage
- .WordRegex
- Task 7 report: Mobalytics adapter (Wave B)
- HandleWindowHandleUpdatedMessage
- Import
- Follow-up B: fix manual-add aspect fan-out
- AspectInfoVM.cs
- .Resolve
- ObservableObject
- Import
- EventArgs
- Table of Contents
- Índice
- DestroyGraphics
- ItemTooltipDescriptor
- ItemAspectLocationDescriptor
- ItemSocketLocationDescriptor
- SettingsView.xaml.cs
- How-to-create-translations.md
- How-to-create-translations-(esES).md

## God Nodes (most connected - your core abstractions)
1. `UserControl` - 138 edges
2. `D4Companion.Entities` - 129 edges
3. `AffixViewModel` - 122 edges
4. `UserControl` - 104 edges
5. `ImportAffixPresetViewModel` - 102 edges
6. `AffixManager` - 81 edges
7. `D4Companion.Interfaces` - 79 edges
8. `IAffixManager` - 76 edges
9. `ItemAffix` - 74 edges
10. `AffixManagerStub` - 58 edges

## Surprising Connections (you probably didn't know these)
- `AffixViewModel` --references--> `AffixLanguage`  [EXTRACTED]
  D4Companion/ViewModels/AffixViewModel.cs → D4Companion.Entities/AffixLanguage.cs
- `AffixViewModel` --references--> `BuildImportWebsite`  [EXTRACTED]
  D4Companion/ViewModels/AffixViewModel.cs → D4Companion.Entities/BuildImportWebsite.cs
- `MainWindowViewModel` --references--> `IOverlayHandler`  [EXTRACTED]
  D4Companion/ViewModels/MainWindowViewModel.cs → D4Companion.Interfaces/IOverlayHandler.cs
- `IAffixManager` --references--> `AffixInfo`  [EXTRACTED]
  D4Companion.Interfaces/IAffixManager.cs → D4Companion.Entities/AffixInfo.cs
- `AffixManager` --references--> `AffixInfo`  [EXTRACTED]
  D4Companion.Services/AffixManager.cs → D4Companion.Entities/AffixInfo.cs

## Import Cycles
- None detected.

## Communities (227 total, 28 thin omitted)

### Community 0 - "UserControl"
Cohesion: 0.02
Nodes (131): ActualHeight, ActualWidth, AddAffixPresetNameCommand, AffixLanguages, AspectsFiltered, BuildImportWebsites, AffixTypeToBgSolidBrushConverter, AspectIdToNameConverter (+123 more)

### Community 1 - "UserControl"
Cohesion: 0.02
Nodes (84): AddD2CoreBuildCommand, AddD4BuildsBuildCommand, AddInfinityBuildsBuildCommand, AddMaxrollBuildCommand, AddMobalyticsBuildCommand, BuildIdD2Core, BuildIdD4Builds, BuildIdMaxroll (+76 more)

### Community 2 - "BuildsManagerMobalytics"
Cohesion: 0.05
Nodes (47): List, MobalyticsBuildBoardBoardJson, MobalyticsBuildBoardGlyphJson, MobalyticsBuildBuildVariantGenericBuilderJson, MobalyticsBuildBuildVariantParagonJson, MobalyticsBuildContentDataChildrenVariantJson, MobalyticsBuildContentDataJson, MobalyticsBuildDataBuildVariantJson (+39 more)

### Community 3 - "AffixViewModel"
Cohesion: 0.03
Nodes (9): bool, ICommand, IDialogCoordinator, ILogger, int, ListCollectionView, ObservableCollection, string (+1 more)

### Community 4 - "WeaponTypeResolver"
Cohesion: 0.05
Nodes (27): ItemTypeInfo, Dictionary, IEnumerable, Name, string, Type, WeaponTypeResolver, MaxrollBuildAdapter (+19 more)

### Community 5 - ".Project"
Cohesion: 0.09
Nodes (17): List, ParagonBoard, CanonicalAffix, CanonicalBuild, CanonicalItem, CanonicalVariant, IBuildPresetProjector, List (+9 more)

### Community 6 - "OverlayHandler.cs"
Cohesion: 0.29
Nodes (14): DrawGraphics(), DrawGraphicsAffixes(), DrawGraphicsAspects(), DrawGraphicsAspectsMulti(), DrawGraphicsMissingAffixes(), DrawGraphicsParagon(), DrawGraphicsParagonCollapsed(), DrawGraphicsParagonWarning() (+6 more)

### Community 7 - "BuildsManagerD4Builds"
Cohesion: 0.06
Nodes (19): List, D4buildsAffix, D4BuildsBuild, D4BuildsBuildVariant, D4buildsParagonBoard, List, IBuildsManagerD4Builds, List (+11 more)

### Community 8 - "UserControl"
Cohesion: 0.04
Nodes (56): AppLanguages, CommunitySystemPresets, LanguageReadyBoolToOpacityConverter, SystemPresetStatusToHealthConverter, DownloadSystemPresetCommand, IsCheckForUpdatesEnabled, IsControllerModeEnabled, IsDebugModeEnabled (+48 more)

### Community 9 - "ItemAffix"
Cohesion: 0.05
Nodes (4): Color, ItemAffix, List, AffixManagerStub

### Community 10 - "UserControl"
Cohesion: 0.04
Nodes (53): AffixAreaHeightOffsetBottom, AffixAreaHeightOffsetTop, AffixAspectAreaWidthOffset, AspectAreaHeightOffsetTop, AspectAreaMaxHeight, AffixIdToDescriptionConverterForOcrResults, ExportDebugImagesCommand, IsDebugInfoEnabled (+45 more)

### Community 11 - "SystemPresetManager"
Cohesion: 0.05
Nodes (25): List, Assets, Release, Task, IHttpClientHandler, List, IReleaseManager, Dictionary (+17 more)

### Community 12 - "BuildsManagerD2Core"
Cohesion: 0.07
Nodes (30): Dictionary, List, D2CoreBuildDataJson, D2CoreBuildDataRootJson, D2CoreBuildDataString, D2CoreBuildDataVariantJson, D2CoreBuildDataVariantParagonJson, D2CoreBuildJson (+22 more)

### Community 13 - "AffixManager"
Cohesion: 0.05
Nodes (6): RuneInfo, Dictionary, ILogger, List, AffixManager, AspectMatchKind

### Community 14 - "FileNameToImagePathConverter.cs"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, FileNameToImagePathConverter

### Community 15 - "D4Companion.Entities"
Cohesion: 0.10
Nodes (8): D4Companion.Tests, D4Companion.Entities, D4Companion.Comparers, D4Companion.Services, D4Companion, D4Companion.Helpers, ItemType, SystemPresetDefaults

### Community 16 - "BuildsManagerInfinityBuilds"
Cohesion: 0.08
Nodes (23): Dictionary, List, InfinityBuildsBuildJson, InfinityBuildsBuildParagonJson, InfinityBuildsBuildParagonSlotJson, InfinityBuildsBuildVariantAffixJson, InfinityBuildsBuildVariantGearJson, InfinityBuildsBuildVariantJson (+15 more)

### Community 17 - "UserControl"
Cohesion: 0.05
Nodes (38): AffixesRunesFiltered, DataContext.AddAffixCommand, DataContext.AddAffixRuneCommand, EditMode, IdName, IsItemTypeRune, ItemTypes, SelectedItemType (+30 more)

### Community 18 - "UserControl"
Cohesion: 0.05
Nodes (38): AddAffixCommand, AffixCounterFeet, AffixCounterHands, AffixCounterHead, AffixCounterLegs, AffixCounterNeck, AffixCounterOffHand, AffixCounterRanged (+30 more)

### Community 19 - "ImportAffixPresetViewModel"
Cohesion: 0.06
Nodes (9): MaxrollBuildsLoadedMessage, Color, ICommand, IDialogCoordinator, ILogger, int, ObservableCollection, string (+1 more)

### Community 20 - "UserControl"
Cohesion: 0.05
Nodes (36): HotkeysConfigDoneCommand, KeyBindingConfigSwitchOverlay, KeyBindingConfigSwitchOverlay.IsEnabled, KeyBindingConfigSwitchOverlay.ToString, KeyBindingConfigSwitchOverlayCommand, KeyBindingConfigSwitchPreset, KeyBindingConfigSwitchPreset.IsEnabled, KeyBindingConfigSwitchPreset.ToString (+28 more)

### Community 21 - "IAffixManager"
Cohesion: 0.06
Nodes (6): ParagonBoardInfo, ParagonGlyphInfo, List, IAffixManager, Test, BuildsManagerMaxrollTests

### Community 22 - "D4Companion.Interfaces"
Cohesion: 0.13
Nodes (8): D4Companion.Interfaces, D4Companion.Views.Dialogs, D4Companion.Messages, D4Companion.ViewModels.Dialogs, D4Companion.ViewModels.Entities, D4Companion.Localization, D4Companion.Extensions, D4Companion.ViewModels

### Community 23 - "BoolConverter"
Cohesion: 0.07
Nodes (21): JsonSerializerOptions, Type, Utf8JsonReader, Utf8JsonWriter, BoolConverter, JsonSerializerOptions, Type, Utf8JsonReader (+13 more)

### Community 24 - ".Resolve"
Cohesion: 0.16
Nodes (8): IEnumerable, List, MissingAffixResolver, Test, TestCase, AffixManagerAspectTests, Test, MissingAffixResolverTests

### Community 25 - "ScreenCaptureHandler"
Cohesion: 0.09
Nodes (16): Bitmap, BitmapSource, HWND, int, ScreenCapture, IScreenCaptureHandler, ApplicationLoadedMessage, Bitmap (+8 more)

### Community 26 - "ScreenProcessMessages.cs"
Cohesion: 0.11
Nodes (19): Bitmap, ScreenProcessItemAffixAreasReadyMessage, ScreenProcessItemAffixAreasReadyMessageParams, ScreenProcessItemAffixLocationsReadyMessage, ScreenProcessItemAffixLocationsReadyMessageParams, ScreenProcessItemAspectAreaReadyMessage, ScreenProcessItemAspectAreaReadyMessageParams, ScreenProcessItemAspectLocationReadyMessage (+11 more)

### Community 27 - "SetAffixViewModel"
Cohesion: 0.08
Nodes (10): List, AffixAttribute, AffixInfo, ICommand, ListCollectionView, ObservableCollection, string, SetAffixViewModel (+2 more)

### Community 28 - "ScreenProcessHandler"
Cohesion: 0.11
Nodes (11): Bitmap, bool, Color, Dictionary, ILogger, int, object, string (+3 more)

### Community 29 - "MaxrollBuildJson.cs"
Cohesion: 0.14
Nodes (18): Dictionary, List, MaxrollBuildDataItemAspectJson, MaxrollBuildDataItemExplicitJson, MaxrollBuildDataItemImplicitJson, MaxrollBuildDataItemJson, MaxrollBuildDataItemTemperedJson, MaxrollBuildDataJson (+10 more)

### Community 30 - "UserControl"
Cohesion: 0.08
Nodes (24): IsItemPowerLimitEnabled, IsOverlayIconVisible, IsSocketDetectionEnabled, ItemPowerLimit, OverlayConfigDoneCommand, OverlayIconPosX, OverlayIconPosY, OverlayMarkerModes (+16 more)

### Community 31 - "OcrHandler"
Cohesion: 0.11
Nodes (7): ConcurrentDictionary, Dictionary, ILogger, List, string, OcrHandler, Language

### Community 32 - "ImportAffixPresetView"
Cohesion: 0.14
Nodes (14): BuildIdMobalytics, BuildIdorUrlD2Core, BuildIdorUrlD4Builds, BuildIdorUrlMaxroll, BuildUrlInfinityBuilds, TextBoxBuildIdD2Core, TextBoxBuildIdD4Builds, TextBoxBuildIdInfinityBuilds (+6 more)

### Community 33 - "SettingsManager"
Cohesion: 0.11
Nodes (15): SettingsManager, string, Test, MaxrollReimportTests, Exception, Func, IDisposable, List (+7 more)

### Community 34 - "FuzzierSharpTests"
Cohesion: 0.14
Nodes (6): Dictionary, List, OneTimeSetUp, SetUp, Test, FuzzierSharpTests

### Community 35 - "SetAffixColorViewModel"
Cohesion: 0.10
Nodes (17): Colors, Key, SelectedColor, SetAffixColorDoneCommand, Color, ICommand, IEnumerable, KeyValuePair (+9 more)

### Community 36 - "SettingsViewModel"
Cohesion: 0.11
Nodes (10): AppLanguage, SystemPresetExtractedMessage, SystemPresetInfoUpdatedMessage, bool, ICommand, IDialogCoordinator, ILogger, int (+2 more)

### Community 37 - "AddTradeItemViewModel"
Cohesion: 0.11
Nodes (7): bool, ICommand, ListCollectionView, ObservableCollection, string, AddTradeItemViewModel, PropertyChangedEventArgs

### Community 38 - "ISystemPresetManager"
Cohesion: 0.10
Nodes (12): SystemPreset, List, ISystemPresetManager, ICommand, ObservableCollection, ControllerConfigViewModel, string, ControllerImageVM (+4 more)

### Community 39 - "MainWindowViewModel"
Cohesion: 0.12
Nodes (6): ICommand, IDialogCoordinator, ILogger, string, MainWindowViewModel, HotkeyEventArgs

### Community 40 - "UserControl"
Cohesion: 0.28
Nodes (5): D4Companion.Views, DebugView, LoggingView, TradeView, UserControl

### Community 41 - "MobalyticsProfileJson.cs"
Cohesion: 0.20
Nodes (19): List, MobalyticsProfileApolloJson, MobalyticsProfileGraphqlJson, MobalyticsProfileGraphqlQueryJson, MobalyticsProfileGraphqlQueryStateDataMgpJson, MobalyticsProfileGraphqlQueryStateDataMgpProfileDataJson, MobalyticsProfileGraphqlQueryStateDataMgpProfileDataUserJson, MobalyticsProfileGraphqlQueryStateDataMgpProfileJson (+11 more)

### Community 42 - "UserControl"
Cohesion: 0.11
Nodes (18): Affixes, DataContext.AddTradeItemCommand, DataContext.EditTradeItemCommand, DataContext.RemoveTradeItemCommand, DataContext.TradeConfigCommand, ItemPower, ItemType, TradeItemsFiltered (+10 more)

### Community 43 - "UserControl"
Cohesion: 0.11
Nodes (16): ColorsConfigDoneCommand, DefaultColorAspects, DefaultColorAspectsOffSlot, DefaultColorGreater, DefaultColorImplicit, DefaultColorNormal, DefaultColorRunes, DefaultColorTempered (+8 more)

### Community 44 - "TradeItem"
Cohesion: 0.16
Nodes (12): Rectangle, ItemAffixAreaDescriptor, List, TradeItem, TradeItemType, List, Tuple, ITradeItemManager (+4 more)

### Community 45 - "LoggingViewModel"
Cohesion: 0.15
Nodes (13): ErrorOccurredMessage, ErrorOccurredMessageParams, ExceptionOccurredMessage, ExceptionOccurredMessageParams, InfoOccurredMessage, InfoOccurredMessageParams, WarningOccurredMessage, WarningOccurredMessageParams (+5 more)

### Community 46 - "UserControl"
Cohesion: 0.12
Nodes (15): AddBuildCommand, DataContext.RemoveBuildCommand, DataContext.SetColorBuildCommand, MultiBuildConfigDoneCommand, MultiBuildList, ButtonDone, UserControl, AffixPresets (+7 more)

### Community 47 - "Image"
Cohesion: 0.37
Nodes (6): Image, List, Gray, Location, Point, Similarity

### Community 48 - "SigilInfo"
Cohesion: 0.19
Nodes (4): SigilInfo, ICommand, List, SigilInfoWanted

### Community 49 - "AffixManagerMessages.cs"
Cohesion: 0.13
Nodes (6): DungeonTiersEnabledChangedMessage, SelectedAffixesChangedMessage, SelectedAspectsChangedMessage, SelectedRunesChangedMessage, SelectedSigilsChangedMessage, SelectedUniquesChangedMessage

### Community 50 - "DebugViewModel"
Cohesion: 0.12
Nodes (12): Axis, ScreenProcessItemTypePowerOcrReadyMessage, BitmapSource, Dictionary, ICommand, ILogger, int, object (+4 more)

### Community 51 - "MetroWindow"
Cohesion: 0.12
Nodes (16): ApplicationLoadedCommand, CompactPaneLength, Icon, IsTopMost, Label, LaunchGitHubCommand, LaunchGitHubWikiCommand, LaunchKofiCommand (+8 more)

### Community 52 - "OcrResultAffix"
Cohesion: 0.14
Nodes (4): ItemAffixDescriptor, OcrResultAffix, Image, IOcrHandler

### Community 53 - "WeaponGroupComparerTests"
Cohesion: 0.30
Nodes (3): WeaponGroupComparer, Test, WeaponGroupComparerTests

### Community 55 - "UserControl"
Cohesion: 0.13
Nodes (13): AvailableImages, ControllerConfigDoneCommand, DataContext.AddControllerCommand, DataContext.RemoveControllerCommand, DataContext.SelectedSystemPreset, Folder, SelectedImages, ButtonDone (+5 more)

### Community 56 - "UserControl"
Cohesion: 0.16
Nodes (12): SelectedSuggestion, ShowOverwriteWarning, Suggestions, ButtonCancel, ButtonDone, UserControl, Name, SetCancelCommand (+4 more)

### Community 57 - "InfinityBuildsBuild"
Cohesion: 0.15
Nodes (7): List, InfinityBuildsAffix, InfinityBuildsBuild, InfinityBuildsBuildVariant, ParagonBoard, List, IBuildsManagerInfinityBuilds

### Community 58 - "OverlayHandlerMessages.cs"
Cohesion: 0.24
Nodes (6): MenuLockedMessage, MenuLockedMessageParams, MenuUnlockedMessage, MenuUnlockedMessageParams, ToggleOverlayMessage, ToggleOverlayMessageParams

### Community 59 - "MobalyticsDownloadViewModel"
Cohesion: 0.17
Nodes (10): MobalyticsBuildsLoadedMessage, MobalyticsCompletedMessage, MobalyticsProfilesLoadedMessage, MobalyticsStatusUpdateMessage, MobalyticsStatusUpdateMessageParams, bool, ICommand, ObservableCollection (+2 more)

### Community 60 - ".FindBestAspectMatch"
Cohesion: 0.26
Nodes (5): AspectMatchKind, IEnumerable, Test, TestCase, AffixManagerFindBestAspectMatchTests

### Community 61 - "UserControl"
Cohesion: 0.14
Nodes (12): AffixConfigDoneCommand, IsMinimalAffixValueFilterEnabled, ResetMinimalAffixValuesCommand, ButtonDone, UserControl, IsMultiBuildModeEnabled, IsTemperedAffixDetectionEnabled, SetColorsCommand (+4 more)

### Community 62 - "UserControl"
Cohesion: 0.14
Nodes (12): IsCollapsedParagonboardEnabled, ParagonBorderSize, ParagonConfigDoneCommand, ParagonLeftOffsetCollapsed, ParagonNodeSize, ParagonNodeSizeCollapsed, ParagonTopOffsetCollapsed, ButtonDone (+4 more)

### Community 63 - "MobalyticsBuild"
Cohesion: 0.11
Nodes (9): List, MobalyticsAffix, MobalyticsBuild, MobalyticsBuildVariant, List, MobalyticsProfile, MobalyticsProfileBuildVariant, List (+1 more)

### Community 64 - "TradeViewModel"
Cohesion: 0.17
Nodes (7): ICommand, IDialogCoordinator, ILogger, int, ListCollectionView, ObservableCollection, TradeViewModel

### Community 65 - "D2CoreDownloadViewModel"
Cohesion: 0.17
Nodes (10): D2CoreBuildsLoadedMessage, D2CoreCompletedMessage, D2CoreStatusUpdateMessage, D2CoreStatusUpdateMessageParams, bool, ICommand, ObservableCollection, string (+2 more)

### Community 66 - "MainWindowViewModel"
Cohesion: 0.15
Nodes (9): IDownloadManager, Dictionary, EventArgs, ILogger, int, string, MainWindowViewModel, DispatcherTimer (+1 more)

### Community 67 - "UserControl"
Cohesion: 0.15
Nodes (11): AspectConfigDoneCommand, AspectIconOffset, ButtonDone, UserControl, IsAspectDetectionEnabled, IsMultiBuildModeEnabled, SetColorsCommand, SetMultiBuildCommand (+3 more)

### Community 68 - "UserControl"
Cohesion: 0.15
Nodes (11): SigilConfigDoneCommand, ButtonDone, UserControl, IsDungeonTiersEnabled, IsMultiBuildModeEnabled, SelectedSigilDisplayMode, SetMultiBuildCommand, SigilDisplayModes (+3 more)

### Community 69 - "MultiBuildConfigViewModel"
Cohesion: 0.15
Nodes (6): Color, MultiBuild, ICommand, IDialogCoordinator, ObservableCollection, MultiBuildConfigViewModel

### Community 70 - "AffixViewModelMessages.cs"
Cohesion: 0.16
Nodes (8): AffixLanguageChangedMessage, AffixPresetChangedMessage, AffixPresetChangedMessageParams, AvailableImagesChangedMessage, ToggleOverlayFromGUIMessage, ToggleOverlayFromGUIMessageParams, HandleAffixPresetChangedMessage(), HandleToggleOverlayFromGUIMessage()

### Community 71 - "D4BuildsDownloadViewModel"
Cohesion: 0.19
Nodes (9): D4BuildsBuildsLoadedMessage, D4BuildsCompletedMessage, D4BuildsStatusUpdateMessage, D4BuildsStatusUpdateMessageParams, bool, ICommand, ObservableCollection, string (+1 more)

### Community 72 - "InfinityBuildsDownloadViewModel"
Cohesion: 0.17
Nodes (10): InfinityBuildsBuildsLoadedMessage, InfinityBuildsCompletedMessage, InfinityBuildsStatusUpdateMessage, InfinityBuildsStatusUpdateMessageParams, bool, ICommand, ObservableCollection, string (+2 more)

### Community 73 - "LiveOcrTooltipTests"
Cohesion: 0.22
Nodes (6): Image, OneTimeSetUp, Test, TestCase, LiveOcrTooltipTests, OneTimeTearDown

### Community 74 - "UserControl"
Cohesion: 0.17
Nodes (10): HotkeyConfigDoneCommand, Keys, Modifiers, SelectedKey, SelectedModifier, ButtonDone, UserControl, RoutedEventArgs (+2 more)

### Community 75 - "UserControl"
Cohesion: 0.17
Nodes (10): IsRuneDetectionEnabled, RuneConfigDoneCommand, ButtonDone, UserControl, IsMultiBuildModeEnabled, SetColorsCommand, SetMultiBuildCommand, RoutedEventArgs (+2 more)

### Community 76 - "UserControl"
Cohesion: 0.17
Nodes (10): IsUniqueDetectionEnabled, UniqueConfigDoneCommand, ButtonDone, UserControl, IsMultiBuildModeEnabled, SetColorsCommand, SetMultiBuildCommand, RoutedEventArgs (+2 more)

### Community 77 - "MultiBooleanToVisibilityConverter"
Cohesion: 0.21
Nodes (7): CultureInfo, Type, MultiBooleanToVisibilityConverter, CultureInfo, Type, MultiOrBooleanToVisibilityConverter, IMultiValueConverter

### Community 78 - "D2CoreBuild"
Cohesion: 0.18
Nodes (3): D2CoreBuild, List, IBuildsManagerD2Core

### Community 79 - "MaxrollBuild"
Cohesion: 0.18
Nodes (3): MaxrollBuild, List, IBuildsManagerMaxroll

### Community 80 - "ScreenCaptureMessages.cs"
Cohesion: 0.18
Nodes (10): Bitmap, HWND, MouseUpdatedMessage, MouseUpdatedMessageParams, ScreenCaptureReadyMessage, ScreenCaptureReadyMessageParams, TakeScreenshotRequestedMessage, WindowHandleUpdatedMessage (+2 more)

### Community 81 - "UserControl"
Cohesion: 0.21
Nodes (9): ButtonCancel, ButtonDone, UserControl, Name, SetCancelCommand, SetDoneCommand, RoutedEventArgs, RenamePresetNameView (+1 more)

### Community 82 - "UserControl"
Cohesion: 0.18
Nodes (9): ShowCurrentItem, TradeConfigDoneCommand, ButtonDone, UserControl, IsTradeOverlayEnabled, OverlayFontSize, RoutedEventArgs, TradeConfigView (+1 more)

### Community 83 - "App"
Cohesion: 0.21
Nodes (7): Application, Exception, IServiceProvider, App, Logger, Mutex, StartupEventArgs

### Community 84 - "SetPresetNameViewModel"
Cohesion: 0.20
Nodes (7): Color, ColorWrapper, StringWrapper, ICommand, List, string, SetPresetNameViewModel

### Community 85 - "ISettingsManager"
Cohesion: 0.18
Nodes (6): Color, List, SettingsD4, ISettingsManager, FakeSettingsManager, SettingsManagerStub

### Community 86 - "KeyBindingMessages.cs"
Cohesion: 0.17
Nodes (6): SwitchOverlayKeyBindingMessage, SwitchPresetKeyBindingMessage, ToggleControllerKeyBindingMessage, ToggleDebugLockScreencaptureKeyBindingMessage, ToggleOverlayKeyBindingMessage, HandleToggleDebugLockScreencaptureKeyBindingMessage()

### Community 87 - "SettingsMessages.cs"
Cohesion: 0.17
Nodes (5): BrightnessThresholdChangedMessage, SystemPresetChangedMessage, ToggleCurrentItemMessage, TopMostStateChangedMessage, UpdateHotkeysRequestMessage

### Community 88 - "D4Companion.Services.csproj"
Cohesion: 0.17
Nodes (11): net10.0-windows, Microsoft.AspNet.WebApi.Client (6.0.0), Microsoft.Extensions.Logging.Abstractions (10.0.9), Selenium.WebDriver (4.45.0), Microsoft.NET.Sdk, Emgu.CV (4.13.0.5924), Emgu.CV.Bitmap (4.13.0.5924), Emgu.CV.runtime.windows (4.13.0.5924) (+3 more)

### Community 89 - ".ConvertToItemType"
Cohesion: 0.27
Nodes (4): OneTimeSetUp, Test, TestCase, OcrHandlerTooltipClassificationTests

### Community 90 - "AffixManagerAddAspectTests"
Cohesion: 0.33
Nodes (4): SetUp, string, Test, AffixManagerAddAspectTests

### Community 91 - "UserControl"
Cohesion: 0.18
Nodes (9): ButtonDone, UserControl, BuildName, SetDoneCommand, Status, Variants, RoutedEventArgs, D2CoreDownloadView (+1 more)

### Community 92 - "UserControl"
Cohesion: 0.18
Nodes (9): ButtonDone, UserControl, BuildName, SetDoneCommand, Status, Variants, RoutedEventArgs, D4BuildsDownloadView (+1 more)

### Community 93 - "UserControl"
Cohesion: 0.18
Nodes (9): ButtonDone, UserControl, BuildName, SetDoneCommand, Status, Variants, RoutedEventArgs, InfinityBuildsDownloadView (+1 more)

### Community 94 - "UserControl"
Cohesion: 0.18
Nodes (9): ButtonDone, UserControl, BuildName, SetDoneCommand, Status, Variants, RoutedEventArgs, MobalyticsDownloadView (+1 more)

### Community 95 - "AffixView"
Cohesion: 0.29
Nodes (7): AffixPresetName, TextBoxFilterAffix, TextBoxPresetName, AffixTextFilter, RoutedEventArgs, AffixView, TextBox

### Community 97 - "D4Companion.Updater.Interfaces"
Cohesion: 0.25
Nodes (6): D4Companion.Updater.Services, D4Companion.Updater, D4Companion.Updater.Interfaces, D4Companion.Updater.ViewModels, D4Companion.Updater.Views, MainWindow

### Community 98 - "D4Companion.csproj"
Cohesion: 0.18
Nodes (10): net10.0-windows, Microsoft.Extensions.Logging.Abstractions (10.0.9), NLog.Extensions.Logging (6.1.3), Microsoft.NET.Sdk, Hardcodet.NotifyIcon.Wpf (2.0.1), LiveChartsCore.SkiaSharpView.WPF (2.0.0-rc2), MahApps.Metro (2.4.11), MahApps.Metro.IconPacks.Material (6.2.1) (+2 more)

### Community 99 - "DownloadManager"
Cohesion: 0.24
Nodes (4): Task, IHttpClientHandler, ILogger, DownloadManager

### Community 100 - "TradeItemWanted"
Cohesion: 0.16
Nodes (9): Color, ItemAffixTradeVM, ObservableCollection, string, TradeItemAdd, TradeItemBase, TradeItemCurrent, TradeItemCustomSort (+1 more)

### Community 101 - "IComparer"
Cohesion: 0.22
Nodes (5): AffixInfoCustomSort, AspectInfoCustomSort, SigilInfoCustomSort, UniqueInfoCustomSort, IComparer

### Community 102 - ".ButtonDone_Click"
Cohesion: 0.47
Nodes (9): ImportAffixPresetDoneCommand, ButtonDone, ButtonDoneD2Core, ButtonDoneD4Builds, ButtonDoneInfinityBuilds, ButtonDoneMaxroll, ButtonDoneMerge, ButtonDoneMobalytics (+1 more)

### Community 104 - "SelectAffixColorViewModel"
Cohesion: 0.31
Nodes (6): Color, ICommand, IEnumerable, KeyValuePair, ObservableCollection, SelectAffixColorViewModel

### Community 105 - "Diablo IV Companion"
Cohesion: 0.14
Nodes (13): Community, Configurations, Diablo IV Companion, Features, Installation, Licensing, Mentions, Readme translations (+5 more)

### Community 106 - "D4Companion.Messages"
Cohesion: 0.20
Nodes (10): D4Companion.Messages, net10.0-windows, Microsoft.NET.Sdk, D4Companion.Updater, net10.0-windows, Microsoft.AspNet.WebApi.Client (6.0.0), Microsoft.Extensions.Logging.Abstractions (10.0.9), NLog.Extensions.Logging (6.1.3) (+2 more)

### Community 107 - "RegexTests"
Cohesion: 0.24
Nodes (5): Dictionary, OneTimeSetUp, SetUp, Test, RegexTests

### Community 108 - "Diablo IV Companion"
Cohesion: 0.14
Nodes (13): Características, Comunidad, Configuraciones, Diablo IV Companion, Instalación, Licencia, Menciones, Paquetes de terceros (+5 more)

### Community 109 - "AffixInfoWanted"
Cohesion: 0.38
Nodes (4): List, AffixInfoBase, AffixInfoConfig, AffixInfoWanted

### Community 111 - "SetAffixTypeColorViewModel"
Cohesion: 0.31
Nodes (6): Color, ICommand, IEnumerable, KeyValuePair, ObservableCollection, SetAffixTypeColorViewModel

### Community 112 - "D4Companion.Helpers.csproj"
Cohesion: 0.25
Nodes (6): net10.0-windows, Microsoft.NET.Sdk, net10.0-windows, Selenium.WebDriver (4.45.0), Microsoft.NET.Sdk, Microsoft.Windows.CsWin32 (0.3.298)

### Community 113 - "AffixConfigViewModel"
Cohesion: 0.25
Nodes (3): ICommand, IDialogCoordinator, AffixConfigViewModel

### Community 114 - "ValueChangedMessage"
Cohesion: 0.36
Nodes (7): DownloadCompletedMessage, DownloadProgressUpdatedMessage, DownloadSystemPresetCompletedMessage, HttpProgress, UploadProgressUpdatedMessage, ScreenProcessItemAffixesOcrReadyMessage, ValueChangedMessage

### Community 115 - "D4Companion.Tests"
Cohesion: 0.25
Nodes (8): D4Companion.Tests, net10.0-windows, Microsoft.NET.Sdk, coverlet.collector (10.0.1), Microsoft.NET.Test.Sdk (18.7.0), NUnit (4.6.1), NUnit3TestAdapter (6.2.0), NUnit.Analyzers (4.14.0)

### Community 116 - "HttpClientHandler"
Cohesion: 0.32
Nodes (4): HttpClient, ILogger, Task, HttpClientHandler

### Community 117 - "AffixIdToDescriptionConverterForOcrResults"
Cohesion: 0.60
Nodes (3): CultureInfo, Type, AffixIdToDescriptionConverterForOcrResults

### Community 118 - "D4Companion.sln"
Cohesion: 0.29
Nodes (4): net10.0-windows, Microsoft.NET.Sdk, net10.0-windows, Microsoft.NET.Sdk

### Community 119 - "AffixDuplicateToSolidBrushConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, AffixDuplicateToSolidBrushConverter

### Community 120 - "RenamePresetNameViewModel"
Cohesion: 0.33
Nodes (3): ICommand, string, RenamePresetNameViewModel

### Community 121 - "UserControl"
Cohesion: 0.50
Nodes (3): ClearLogMessagesCommand, LogMessages, UserControl

### Community 122 - "HotkeyConfigViewModel"
Cohesion: 0.20
Nodes (7): KeyBindingConfig, ICommand, ObservableCollection, string, HotkeyConfigViewModel, Key, ModifierKeys

### Community 123 - "OverlayConfigViewModel"
Cohesion: 0.29
Nodes (4): ICommand, IDialogCoordinator, ObservableCollection, OverlayConfigViewModel

### Community 124 - "SigilConfigViewModel"
Cohesion: 0.29
Nodes (4): ICommand, IDialogCoordinator, ObservableCollection, SigilConfigViewModel

### Community 125 - "App"
Cohesion: 0.40
Nodes (4): Application, Application, IServiceProvider, App

### Community 126 - "IValueConverter"
Cohesion: 0.38
Nodes (4): CultureInfo, Type, AffixIdToDescriptionConverter, IValueConverter

### Community 127 - "AffixIdToRuneDescriptionConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, AffixIdToRuneDescriptionConverter

### Community 128 - "AffixIdToRuneNameConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, AffixIdToRuneNameConverter

### Community 129 - "D4Companion.Converters"
Cohesion: 0.38
Nodes (4): D4Companion.Converters, CultureInfo, Type, AffixTypeToBgSolidBrushConverter

### Community 130 - "AffixTypeToFgSolidBrushConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, AffixTypeToFgSolidBrushConverter

### Community 131 - "AspectIdToDescriptionConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, AspectIdToDescriptionConverter

### Community 132 - "AspectIdToNameConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, AspectIdToNameConverter

### Community 133 - "BooleanToVisibilityConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, BooleanToVisibilityConverter

### Community 134 - "ColorToSolidBrushConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, ColorToSolidBrushConverter

### Community 135 - "FileNameToFileNameNoExtConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, FileNameToFileNameNoExtConverter

### Community 136 - "FlagToImagePathConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, FlagToImagePathConverter

### Community 137 - "InverseBooleanConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, InverseBooleanConverter

### Community 138 - "InverseBooleanToVisibilityConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, InverseBooleanToVisibilityConverter

### Community 139 - "LanguageReadyBoolToOpacityConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, LanguageReadyBoolToOpacityConverter

### Community 140 - "Resources"
Cohesion: 0.50
Nodes (3): CultureInfo, ResourceManager, Resources

### Community 141 - "RankToVisibilityConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, RankToVisibilityConverter

### Community 142 - "RuneIdToDescriptionConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, RuneIdToDescriptionConverter

### Community 143 - "RuneIdToNameConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, RuneIdToNameConverter

### Community 144 - "SigilIdToDescriptionConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, SigilIdToDescriptionConverter

### Community 145 - "SigilIdToNameConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, SigilIdToNameConverter

### Community 146 - "SystemPresetStatusToHealthConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, SystemPresetStatusToHealthConverter

### Community 147 - "UniqueIdToDescriptionConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, UniqueIdToDescriptionConverter

### Community 148 - "UniqueIdToNameConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, UniqueIdToNameConverter

### Community 149 - "WeaponTypeToGroupLabelConverter"
Cohesion: 0.47
Nodes (3): CultureInfo, Type, WeaponTypeToGroupLabelConverter

### Community 150 - "AffixGlobal.cs"
Cohesion: 0.67
Nodes (5): List, AffixGlobal, ArSortedAffixes, ArSortedAffixGroups, PtContent

### Community 151 - "D4Companion.Entities.csproj"
Cohesion: 0.33
Nodes (4): net10.0-windows, Microsoft.NET.Sdk, net10.0-windows, Microsoft.NET.Sdk

### Community 152 - "AspectConfigViewModel"
Cohesion: 0.33
Nodes (3): ICommand, IDialogCoordinator, AspectConfigViewModel

### Community 153 - "DrawGraphicsAffixesMulti"
Cohesion: 0.18
Nodes (11): Color, IEnumerable, KeyValuePair, List, Tuple, DrawGraphicsAffixesMulti(), DrawStatPriority(), GetColors() (+3 more)

### Community 155 - "UniqueConfigViewModel"
Cohesion: 0.33
Nodes (3): ICommand, IDialogCoordinator, UniqueConfigViewModel

### Community 156 - "TextBlock"
Cohesion: 0.33
Nodes (6): TextBoxBuildIdD2CoreWatermark, TextBoxBuildIdD4BuildsWatermark, TextBoxBuildIdInfinityBuildsWatermark, TextBoxBuildIdMaxrollWatermark, TextBoxBuildIdMobalyticsWatermark, TextBlock

### Community 157 - "Window"
Cohesion: 0.40
Nodes (4): DownloadProgress, StatusText, Window, WindowTitle

### Community 158 - ".AddRange"
Cohesion: 0.40
Nodes (3): Collection, IEnumerable, CollectionExtensions

### Community 159 - "TranslationSource"
Cohesion: 0.50
Nodes (4): CultureInfo, ResourceManager, TranslationSource, INotifyPropertyChanged

### Community 161 - "ObjectPool"
Cohesion: 0.27
Nodes (4): ConcurrentBag, Func, ObjectPool, Image

### Community 162 - "Frequently-asked-questions.md"
Cohesion: 0.18
Nodes (10): Brightness, Configuration issue, How to enable controller support, Item Power, Item Type, Micro stutters with High-End PC and g-sync, Missing dll exception from Emgu.CV.CvInvoke, Missing green/red dots even when overlay app icon is visible (+2 more)

### Community 163 - "settings.json"
Cohesion: 0.33
Nodes (5): enabledPlugins, extraKnownMarketplaces, hooks, PreToolUse, $schema

### Community 164 - "Follow-up C report: locale-independent weapon subtype detection"
Cohesion: 0.18
Nodes (10): 14-locale alignment verification (Part 1, step 1), AffixManager.IsTypeMatch / backward compatibility, Follow-up C report: locale-independent weapon subtype detection, Line-number mismatches found vs. the brief, Old cache file behavior (verified with a test, not asserted from memory), Part 1: locale-independent weapon subtype detection, Part 2a: D4Builds weapon-subtype provenance, Part 2b: D2Core weapon-class mapping (+2 more)

### Community 165 - "OcrResult"
Cohesion: 0.21
Nodes (6): OcrResult, OcrResultDescriptor, OcrResultItemType, List, ScreenProcessItemAffixesOcrReadyMessageParams, ScreenProcessItemTypePowerOcrReadyMessageParams

### Community 166 - "RuneInfoVM.cs"
Cohesion: 0.24
Nodes (4): RuneInfoBase, RuneInfoConfig, RuneInfoCustomSort, RuneInfoWanted

### Community 167 - "D4Companion.Constants"
Cohesion: 0.07
Nodes (19): D4Companion.Entities.Canonical, D4Companion.Constants, D4Companion.Services.BuildAdapters, string, AffixTypeConstants, string, ItemRarityConstants, string (+11 more)

### Community 168 - "HamburgerMenuControl"
Cohesion: 0.33
Nodes (4): HamburgerMenuControl, MainWindow, HamburgerMenuItemInvokedEventArgs, HamburgerMenu

### Community 169 - "NotifyIcon"
Cohesion: 0.67
Nodes (3): NotifyIconDoubleClickCommand, NotifyIcon, TaskbarIcon

### Community 175 - "HotkeysConfigViewModel"
Cohesion: 0.38
Nodes (3): ICommand, IDialogCoordinator, HotkeysConfigViewModel

### Community 176 - "TextBoxFilterAffixWatermark"
Cohesion: 0.67
Nodes (3): TextBoxFilterAffixWatermark, TextBoxPresetNameWatermark, TextBlock

### Community 187 - "How-to-create-a-new-System-Preset.md"
Cohesion: 0.20
Nodes (9): Capture images, Folder structure, Languages, Naming convention, Sharing, Socket images, systempresets.json, Testing (+1 more)

### Community 188 - "Frequently-asked-questions-(esES).md"
Cohesion: 0.20
Nodes (9): Brillo, Cómo activar la compatibilidad con Mando, Falta el icono de la aplicación(Overlay) en la esquina superior izquierda, Faltan puntos verdes/rojos incluso cuando el icono Overlay de la aplicación está visible., Micro tirones con PC de gama alta y g-sync, Missing dll exception from Emgu.CV.CvInvoke, Poder de Objeto, Problemas de configuración (+1 more)

### Community 189 - "How-to-create-a-new-System-Preset-(esES).md"
Cohesion: 0.20
Nodes (9): Capturar imágenes, Compartir, Convención de nombres, Estructura de carpetas, Idiomas, Imágenes de huecos, Pruebas, Servicio de herramientas (+1 more)

### Community 192 - "D4Companion - Claude Code guidance"
Cohesion: 0.25
Nodes (7): Architecture, C# tooling / navigation, D4Companion - Claude Code guidance, Gotchas (hard constraints, not style), Personal data - `loadout/` (local-only, gitignored), Platform - Windows only, Test gotchas

### Community 193 - "RuneConfigViewModel"
Cohesion: 0.33
Nodes (3): ICommand, IDialogCoordinator, RuneConfigViewModel

### Community 194 - "TooltipDataReadyMessage"
Cohesion: 0.40
Nodes (3): TooltipDataReadyMessage, TooltipDataReadyMessageParams, HandleTooltipDataReadyMessage()

### Community 196 - "Task 7 report: Mobalytics adapter (Wave B)"
Cohesion: 0.25
Nodes (7): Build / test results, Commit, Discrepancy from instructions (report, not silently worked around), Files touched, Status: Complete, Task 7 report: Mobalytics adapter (Wave B), Verification of the brief's assumed names (the brief itself flagged `AffixText` as uncertain)

### Community 197 - "HandleWindowHandleUpdatedMessage"
Cohesion: 0.50
Nodes (5): HWND, HandleWindowHandleUpdatedMessage(), HasNewWindowBounds(), InitOverlayWindow(), IsValidWindowSize()

### Community 198 - "Import"
Cohesion: 0.29
Nodes (6): Export, Import, Instructions D2Core, Instructions D4Builds, Instructions Maxroll, Instructions Mobalytics

### Community 199 - "Follow-up B: fix manual-add aspect fan-out"
Cohesion: 0.29
Nodes (6): Could not verify, Follow-up B: fix manual-add aspect fan-out, Investigation findings (points 1-4), Line-number check, Test note, What changed

### Community 200 - "AspectInfoVM.cs"
Cohesion: 0.60
Nodes (4): List, AspectInfoBase, AspectInfoConfig, AspectInfoWanted

### Community 202 - "ObservableObject"
Cohesion: 0.14
Nodes (10): Color, ICommand, IDialogCoordinator, ColorsConfigViewModel, ICommand, IDialogCoordinator, ParagonConfigViewModel, ICommand (+2 more)

### Community 203 - "Import"
Cohesion: 0.33
Nodes (5): Export, Import, Instructions D4Builds, Instructions Maxroll, Instructions Mobalytics

### Community 205 - "EventArgs"
Cohesion: 0.50
Nodes (4): EventArgs, CurrentAffixPresetTimer_Tick(), NotificationTimer_Tick(), ParagonStepTimer_Tick()

### Community 206 - "Table of Contents"
Cohesion: 0.40
Nodes (4): Diablo IV Companion, Frequently asked questions, Guides, Table of Contents

### Community 207 - "Índice"
Cohesion: 0.40
Nodes (4): Diablo IV Companion, Guías, Preguntas más frecuentes, Índice

### Community 209 - "ItemTooltipDescriptor"
Cohesion: 0.17
Nodes (9): Rectangle, ItemAffixLocationDescriptor, Rectangle, ItemSplitterLocationDescriptor, Dictionary, List, Rectangle, Tuple (+1 more)

## Knowledge Gaps
- **759 isolated node(s):** `C# tooling / navigation`, `Architecture`, `Gotchas (hard constraints, not style)`, `Test gotchas`, `Personal data - `loadout/` (local-only, gitignored)` (+754 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **28 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ISettingsManager` connect `ISettingsManager` to `BuildsManagerMobalytics`, `AffixViewModel`, `.Project`, `BuildsManagerD4Builds`, `SystemPresetManager`, `BuildsManagerD2Core`, `AffixManager`, `BuildsManagerInfinityBuilds`, `ImportAffixPresetViewModel`, `AspectConfigViewModel`, `ScreenCaptureHandler`, `UniqueConfigViewModel`, `ScreenProcessHandler`, `SetAffixViewModel`, `OcrHandler`, `SettingsManager`, `SettingsViewModel`, `ISystemPresetManager`, `MainWindowViewModel`, `TradeItem`, `HotkeysConfigViewModel`, `SigilInfo`, `DebugViewModel`, `TradeViewModel`, `RuneConfigViewModel`, `MultiBuildConfigViewModel`, `ObservableObject`, `AffixConfigViewModel`, `OverlayConfigViewModel`, `SigilConfigViewModel`?**
  _High betweenness centrality (0.202) - this node is a cross-community bridge._
- **Why does `D4Companion.Entities` connect `D4Companion.Entities` to `D4Companion.Converters`, `AffixTypeToFgSolidBrushConverter`, `BuildsManagerMobalytics`, `WeaponTypeResolver`, `OverlayHandler.cs`, `BuildsManagerD4Builds`, `ItemAffix`, `SystemPresetManager`, `BuildsManagerD2Core`, `AffixManager`, `FileNameToImagePathConverter.cs`, `BuildsManagerInfinityBuilds`, `IAffixManager`, `AffixGlobal.cs`, `D4Companion.Interfaces`, `ScreenProcessMessages.cs`, `SetAffixViewModel`, `MaxrollBuildJson.cs`, `SettingsViewModel`, `OcrResult`, `ISystemPresetManager`, `D4Companion.Constants`, `RuneInfoVM.cs`, `MobalyticsProfileJson.cs`, `HOcrClasses.cs`, `Inventory.cs`, `TradeItem`, `ItemTypeDescriptor.cs`, `SigilInfo`, `AffixLanguage.cs`, `BuildImportWebsite.cs`, `OcrResultAffix`, `SigilInfoVM.cs`, `InfinityBuildsBuild`, `MobalyticsDownloadViewModel`, `.FindItemAspect`, `AffixPreset`, `MobalyticsBuild`, `D2CoreDownloadViewModel`, `MultiBuildConfigViewModel`, `D4BuildsDownloadViewModel`, `InfinityBuildsDownloadViewModel`, `.Resolve`, `AspectInfoVM.cs`, `D2CoreBuild`, `MaxrollBuild`, `ItemTooltipDescriptor`, `ItemAspectLocationDescriptor`, `ItemSocketLocationDescriptor`, `SetPresetNameViewModel`, `ISettingsManager`, `UniqueInfo`, `TradeItemWanted`, `AspectInfo`, `AffixInfoWanted`, `AffixDuplicateToSolidBrushConverter`, `HotkeyConfigViewModel`?**
  _High betweenness centrality (0.155) - this node is a cross-community bridge._
- **Why does `AffixViewModel` connect `AffixViewModel` to `UserControl`, `BuildsManagerD4Builds`, `ItemAffix`, `IAffixManager`, `D4Companion.Interfaces`, `SetAffixViewModel`, `ISystemPresetManager`, `RuneInfoVM.cs`, `SigilInfo`, `AffixLanguage.cs`, `BuildImportWebsite.cs`, `AffixManagerMessages.cs`, `SigilInfoVM.cs`, `InfinityBuildsBuild`, `OverlayHandlerMessages.cs`, `MobalyticsBuild`, `AffixPreset`, `AspectInfoVM.cs`, `ObservableObject`, `D2CoreBuild`, `MaxrollBuild`, `ISettingsManager`, `KeyBindingMessages.cs`, `UniqueInfo`, `AspectInfo`, `AffixInfoWanted`, `.UpdateAffixPresets`?**
  _High betweenness centrality (0.102) - this node is a cross-community bridge._
- **What connects `C# tooling / navigation`, `Architecture`, `Gotchas (hard constraints, not style)` to the rest of the system?**
  _759 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `UserControl` be split into smaller, more focused modules?**
  _Cohesion score 0.015151515151515152 - nodes in this community are weakly interconnected._
- **Should `UserControl` be split into smaller, more focused modules?**
  _Cohesion score 0.023529411764705882 - nodes in this community are weakly interconnected._
- **Should `BuildsManagerMobalytics` be split into smaller, more focused modules?**
  _Cohesion score 0.05442428730099963 - nodes in this community are weakly interconnected._