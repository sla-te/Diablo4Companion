using CommunityToolkit.Mvvm.Messaging;
using D4Companion.Entities;
using D4Companion.Entities.Canonical;
using D4Companion.Interfaces;
using D4Companion.Messages;
using D4Companion.Services.BuildAdapters;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace D4Companion.Services
{
    public class BuildsManagerMaxroll : IBuildsManagerMaxroll
    {
        private readonly ILogger _logger;
        private readonly IAffixManager _affixManager;
        private readonly IBuildPresetProjector _projector;
        private readonly IHttpClientHandler _httpClientHandler;
        private readonly ISettingsManager _settingsManager;
        private readonly MaxrollBuildAdapter _maxrollBuildAdapter = new();

        private List<MaxrollBuild> _maxrollBuilds = new();
        private Dictionary<int, int> _maxrollMappingsAspects = new();

        // Start of Constructors region

        #region Constructors

        public BuildsManagerMaxroll(ILogger<BuildsManagerMaxroll> logger, IAffixManager affixManager, IBuildPresetProjector projector, IHttpClientHandler httpClientHandler, ISettingsManager settingsManager)
        {
            // Init services
            _affixManager = affixManager;
            _projector = projector;
            _httpClientHandler = httpClientHandler;
            _logger = logger;
            _settingsManager = settingsManager;

            // Load available Maxroll builds.
            Task.Factory.StartNew(() =>
            {
                LoadAvailableMaxrollBuilds();
            });
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public List<MaxrollBuild> MaxrollBuilds { get => _maxrollBuilds; set => _maxrollBuilds = value; }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        #endregion

        // Start of Methods region

        #region Methods

        public void CreatePresetFromMaxrollBuild(MaxrollBuild maxrollBuild, string profile, string name)
        {
            name = string.IsNullOrWhiteSpace(name) ? maxrollBuild.Name : name;

            // Note: Only allow one Maxroll build. Update if already exists.
            _affixManager.AffixPresets.RemoveAll(p => p.Name.Equals(name));

            var maxrollBuildDataProfileJson = maxrollBuild.Data.Profiles.FirstOrDefault(p => p.Name.Equals(profile));
            if (maxrollBuildDataProfileJson != null)
            {
                var canonicalBuild = _maxrollBuildAdapter.ToCanonical(maxrollBuild);
                var canonicalVariant = canonicalBuild.Variants.FirstOrDefault(v => v.Name.Equals(profile));
                if (canonicalVariant == null) return;

                int canonicalItemIndex = 0;
                string itemType = string.Empty;

                // Loop through all items
                foreach (var item in maxrollBuildDataProfileJson.Items)
                {
                    switch (item.Key)
                    {
                        case 4: // Helm
                            itemType = Constants.ItemTypeConstants.Helm;
                            break;
                        case 5: // Chest
                            itemType = Constants.ItemTypeConstants.Chest;
                            break;
                        case 6: // 1HTotem
                            itemType = Constants.ItemTypeConstants.Offhand;
                            break;
                        case 7: // 1HAxe, 1HFlail
                        case 8: // 2HMace
                        case 9: // 2HAxe
                        case 11: // 1HMace, 1HSword - mainhand
                        case 12: // 1HMace, 1HSword - offhand
                            // Coarser than what the preset ends up carrying, and that is
                            // fine: itemType survives only as far as the Charm/Seal skip
                            // below. The type actually written comes from CanonicalItem.Slot
                            // via MaxrollBuildAdapter, which does separate the two hands.
                            itemType = Constants.ItemTypeConstants.Weapon;
                            break;
                        case 10: // 2HCrossbow
                            itemType = Constants.ItemTypeConstants.Ranged;
                            break;
                        case 13: // Gloves
                            itemType = Constants.ItemTypeConstants.Gloves;
                            break;
                        case 14: // Pants
                            itemType = Constants.ItemTypeConstants.Pants;
                            break;
                        case 15: // Boots
                            itemType = Constants.ItemTypeConstants.Boots;
                            break;
                        case 16: // Ring
                        case 17: // Ring
                            itemType = Constants.ItemTypeConstants.Ring;
                            break;
                        case 18: // Amulet
                            itemType = Constants.ItemTypeConstants.Amulet;
                            break;
                        case 20: // Talisman Seal
                            itemType = Constants.ItemTypeConstants.HoradricSeal;
                            break;
                        case 21: // Talisman Charm
                        case 22: // Talisman Charm
                        case 23: // Talisman Charm
                        case 24: // Talisman Charm
                        case 25: // Talisman Charm
                        case 26: // Talisman Charm
                            itemType = Constants.ItemTypeConstants.Charm;
                            break;
                        default:
                            _logger.LogWarning($"{MethodBase.GetCurrentMethod()?.Name}: Unknown itemtype id: {item.Key}");
                            WeakReferenceMessenger.Default.Send(new WarningOccurredMessage(new WarningOccurredMessageParams
                            {
                                Message = $"Imported Maxroll build contains unknown itemtype id: {item.Key}."
                            }));
                            continue;
                    }

                    // Skip Charm and HoradricSeal. Not implemented yet, and MaxrollBuildAdapter.ResolveSlot
                    // returns null for these slots, so the adapter never emits a CanonicalItem for them
                    // either. Skip here before touching canonicalItemIndex so the two stay aligned.
                    if (itemType.Equals(Constants.ItemTypeConstants.Charm) || itemType.Equals(Constants.ItemTypeConstants.HoradricSeal))
                    {
                        continue;
                    }

                    // Match the CanonicalItem the adapter produced for this same slot entry.
                    // Invariant: the adapter and this manager must skip identical item types, so the
                    // surviving entries line up index-for-index with canonicalVariant.Items. If that
                    // invariant is ever violated, fail loudly instead of binding affixes to the wrong item.
                    if (canonicalItemIndex >= canonicalVariant.Items.Count)
                    {
                        _logger.LogError($"{MethodBase.GetCurrentMethod()?.Name}: canonicalItemIndex {canonicalItemIndex} out of range for {canonicalVariant.Items.Count} canonical items. Adapter and manager item-type skip lists have diverged.");
                        break;
                    }
                    var canonicalItem = canonicalVariant.Items[canonicalItemIndex++];

                    // Process runes
                    foreach (var socket in maxrollBuild.Data.Items[item.Value].Sockets)
                    {
                        string runeId = socket ?? string.Empty;
                        if (!runeId.StartsWith("Rune_", StringComparison.OrdinalIgnoreCase)) continue;

                        canonicalItem.RuneIds.Add($"Item_{runeId}");
                    }

                    // Process unique items
                    string uniqueId = maxrollBuild.Data.Items[item.Value].Id;
                    var uniqueInfo = _affixManager.Uniques.FirstOrDefault(u => u.IdNameItem.Contains(uniqueId)) ??
                        _affixManager.Uniques.FirstOrDefault(u => u.IdNameItemActor.Equals(uniqueId));
                    if (uniqueInfo != null)
                    {
                        // Add unique items
                        canonicalItem.UniqueIds.Add(uniqueInfo.IdName);
                    }

                    // Process implicit affixes
                    if (uniqueInfo != null)
                    {
                        // Process unique implicit affixes.
                        // - Uniques
                        // - Mythics

                        // Note: If Mythics only have implicit affixes on specific item types the check below could be used to skip them.
                        //if (itemType.Equals(Constants.ItemTypeConstants.Amulet) ||
                        //    itemType.Equals(Constants.ItemTypeConstants.Pants) ||
                        //    itemType.Equals(Constants.ItemTypeConstants.Ring))
                        //{
                        //    // This item type no longer has implicit affixes.
                        //}

                        foreach (var implicitAffix in maxrollBuild.Data.Items[item.Value].Implicits)
                        {
                            int affixSno = implicitAffix.Nid;

                            AffixInfo? affixInfo = _affixManager.GetAffixInfoMaxrollByIdSno(affixSno.ToString());
                            if (affixInfo == null)
                            {
                                _logger.LogWarning($"{MethodBase.GetCurrentMethod()?.Name}: Unknown implicit affix sno: {affixSno}");
                                WeakReferenceMessenger.Default.Send(new WarningOccurredMessage(new WarningOccurredMessageParams
                                {
                                    Message = $"Imported Maxroll build contains unknown implicit affix sno: {affixSno}."
                                }));
                            }
                            else
                            {
                                canonicalItem.Affixes.Add(new CanonicalAffix { Id = affixInfo.IdName, IsImplicit = true });
                            }
                        }
                    }
                    /*else
                    {
                        // Process legendary implicit affixes.

                        // Note: Maxroll implicits are overruled by the website for each legendary type.
                        // The implicits set in the json data are ignored.

                        // Note: Only legendary items with implicit affixes are boots and shields.
                        //       Implicit affixes for boots are no longer seperated from normal affixes. (Season 13)

                        string itemId = maxrollBuild.Data.Items[item.Value].Id;
                        string itemTypeFromJson = GetItemTypeFromItemId(itemId);

                        List<string> affixNames = new List<string>();
                        if (itemTypeFromJson.Equals("1HShield"))
                        {
                            affixNames.Add("INHERENT_Block");
                            affixNames.Add("INHERENT_Shield_Damage_Bonus");
                            affixNames.Add("INHERENT_Thorns");
                        }

                        foreach (var affix in affixNames)
                        {
                            AffixInfo? affixInfo = _affixManager.GetAffixInfoByIdName(affix);
                            if (affixInfo == null)
                            {
                                _logger.LogWarning($"{MethodBase.GetCurrentMethod()?.Name}: Imported Maxroll build contains unknown implicit affix name: {affix}.");
                                WeakReferenceMessenger.Default.Send(new WarningOccurredMessage(new WarningOccurredMessageParams
                                {
                                    Message = $"Imported Maxroll build contains unknown implicit affix name: {affix}."
                                }));
                                continue;
                            }

                            if (!affixPreset.ItemAffixes.Any(a => a.Id.Equals(affixInfo.IdName) && a.Type.Equals(itemType)))
                            {
                                affixPreset.ItemAffixes.Add(new ItemAffix
                                {
                                    Id = affixInfo.IdName,
                                    Type = itemType,
                                    Color = _settingsManager.Settings.DefaultColorImplicit,
                                    IsImplicit = true
                                });
                            }
                        }

                        if (itemTypeFromJson.Equals("Boots"))
                        {
                            // Note: Do not use a hardcoded affix for boots. Boots have different implicit affixes.
                            //affixNames.Add("INHERENT_Evade_Attack_Reset");
                            //affixNames.Add("INHERENT_Evade_Charges");
                            //affixNames.Add("INHERENT_Evade_MovementSpeed");
                            foreach (var implicitAffix in maxrollBuild.Data.Items[item.Value].Implicits)
                            {
                                int affixSno = implicitAffix.Nid;

                                AffixInfo? affixInfo = _affixManager.GetAffixInfoMaxrollByIdSno(affixSno.ToString());
                                if (affixInfo != null)
                                {
                                    if (!affixPreset.ItemAffixes.Any(a => a.Id.Equals(affixInfo.IdName) && a.Type.Equals(itemType)))
                                    {
                                        affixPreset.ItemAffixes.Add(new ItemAffix
                                        {
                                            Id = affixInfo.IdName,
                                            Type = itemType,
                                            Color = _settingsManager.Settings.DefaultColorImplicit,
                                            IsImplicit = true
                                        });
                                    }
                                }
                            }
                        }
                    }*/

                    // Add all explicit affixes for current item.Value
                    // Import every listed stat, not just the four an item can physically
                    // roll. Maxroll's explicit list is a ranked stat-priority list - the
                    // amulet in the Whirlwind guide names seven - so truncating it to four
                    // left the lower-priority stats unmatched and showing as unwanted.
                    // Entries that are not affixes at all (a unique's own aspect) still fall
                    // out below, where an unresolved sno is skipped.
                    for (int i = 0; i < maxrollBuild.Data.Items[item.Value].Explicits.Count; i++)
                    {
                        var explicitAffix = maxrollBuild.Data.Items[item.Value].Explicits[i];
                        int affixSno = explicitAffix.Nid;
                        AffixInfo? affixInfo = _affixManager.GetAffixInfoMaxrollByIdSno(affixSno.ToString());

                        if (affixInfo == null)
                        {
                            // Only log warning when affix is not found and it's not a unique aspect.
                            // Note: Check needed because list of affixes returned by Maxroll also contains the unique aspect.
                            //       In season 11 sanctified unique aspects are also added as affixes and will log a warning here.
                            if (_affixManager.GetUniqueInfoMaxrollByIdSno(affixSno.ToString()) == null)
                            {
                                _logger.LogWarning($"{MethodBase.GetCurrentMethod()?.Name}: Unknown affix sno: {affixSno}");
                                WeakReferenceMessenger.Default.Send(new WarningOccurredMessage(new WarningOccurredMessageParams
                                {
                                    Message = $"Imported Maxroll build contains unknown affix sno: {affixSno}."
                                }));
                            }
                        }
                        else
                        {
                            canonicalItem.Affixes.Add(new CanonicalAffix
                            {
                                Id = affixInfo.IdName,
                                IsGreater = explicitAffix.IsGreaterAffix,
                                // The loop index, not a running counter over the affixes we
                                // managed to resolve. An entry that resolves to nothing is
                                // still numbered in the guide, so renumbering around it would
                                // shift every later stat one rank up.
                                Rank = i + 1
                            });
                        }
                    }

                    // Add all tempered affixes for current item.Value
                    foreach (var temperedAffix in maxrollBuild.Data.Items[item.Value].Tempered)
                    {
                        int affixSno = temperedAffix.Nid;
                        AffixInfo? affixInfo = _affixManager.GetAffixInfoMaxrollByIdSno(affixSno.ToString());

                        if (affixInfo == null)
                        {
                            _logger.LogWarning($"{MethodBase.GetCurrentMethod()?.Name}: Unknown tempered affix sno: {affixSno}");
                            WeakReferenceMessenger.Default.Send(new WarningOccurredMessage(new WarningOccurredMessageParams
                            {
                                Message = $"Imported Maxroll build contains unknown tempered affix sno: {affixSno}."
                            }));
                        }
                        else
                        {
                            canonicalItem.Affixes.Add(new CanonicalAffix
                            {
                                Id = affixInfo.IdName,
                                IsTempered = true
                            });
                        }
                    }

                    // Resolve aspects / legendary powers. The adapter can only stash the raw
                    // Maxroll sno on the CanonicalItem because it has no IAffixManager access;
                    // replace those raw values with the real IdName here.
                    var resolvedAspectIds = new List<string>();
                    foreach (var aspectSno in canonicalItem.AspectIds)
                    {
                        AspectInfo? aspectInfo = _affixManager.GetAspectInfoMaxrollByIdSno(aspectSno);
                        if (aspectInfo == null)
                        {
                            _logger.LogWarning($"{MethodBase.GetCurrentMethod()?.Name}: Unknown aspect sno: {aspectSno}");
                            WeakReferenceMessenger.Default.Send(new WarningOccurredMessage(new WarningOccurredMessageParams
                            {
                                Message = $"Imported Maxroll build contains unknown aspect sno: {aspectSno}."
                            }));
                        }
                        else
                        {
                            resolvedAspectIds.Add(aspectInfo.IdName);
                        }
                    }
                    canonicalItem.AspectIds = resolvedAspectIds;
                }

                var affixPreset = _projector.Project(canonicalVariant, name);

                // Add all paragon boards
                if (_settingsManager.Settings.IsImportParagonMaxrollEnabled)
                {
                    foreach (var paragonBoardStep in maxrollBuildDataProfileJson.Paragon.Steps)
                    {
                        var paragonBoards = new List<ParagonBoard>();

                        string paragonBoardStepName = paragonBoardStep.Name;
                        foreach (var paragonBoardData in paragonBoardStep.Data)
                        {
                            var paragonBoard = new ParagonBoard();
                            paragonBoard.Name = _affixManager.GetParagonBoardLocalisation(paragonBoardData.Id);
                            paragonBoard.Glyph = _affixManager.GetParagonGlyphLocalisation(paragonBoardData.Glyph);
                            string rotationInfo = paragonBoardData.Rotation == 0 ? "0°" :
                                paragonBoardData.Rotation == 1 ? "90°" :
                                paragonBoardData.Rotation == 2 ? "180°" :
                                paragonBoardData.Rotation == 3 ? "270°" : "?°";
                            paragonBoard.Rotation = rotationInfo;
                            paragonBoards.Add(paragonBoard);

                            // Process nodes
                            int rotation = paragonBoardData.Rotation;
                            foreach (var location in paragonBoardData.Nodes.Keys)
                            {
                                int locationT = location;
                                int locationX = location % 21;
                                int locationY = location / 21;
                                int locationXT = locationX;
                                int locationYT = locationY;
                                switch (rotation)
                                {
                                    case 0:
                                        locationT = location;
                                        break;
                                    case 1:
                                        locationXT = 21 - locationY;
                                        locationYT = locationX;
                                        locationXT = locationXT - 1;
                                        locationT = locationYT * 21 + locationXT;
                                        break;
                                    case 2:
                                        locationXT = 21 - locationX;
                                        locationYT = 21 - locationY;
                                        locationXT = locationXT - 1;
                                        locationYT = locationYT - 1;
                                        locationT = locationYT * 21 + locationXT;
                                        break;
                                    case 3:
                                        locationXT = locationY;
                                        locationYT = 21 - locationX;
                                        locationYT = locationYT - 1;
                                        locationT = locationYT * 21 + locationXT;
                                        break;
                                    default:
                                        locationT = location;
                                        break;
                                }
                                paragonBoard.Nodes[locationT] = true;
                            }
                        }
                        affixPreset.ParagonBoardsList.Add(paragonBoards);
                    }
                }
                _affixManager.AddAffixPreset(affixPreset);
            }
        }

        public async void DownloadMaxrollBuild(string build)
        {
            try
            {
                string uri = $"https://planners.maxroll.gg/profiles/d4/{build}";

                string json = await _httpClientHandler.GetRequest(uri);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    MaxrollBuildJson? maxrollBuildJson = JsonSerializer.Deserialize<MaxrollBuildJson>(json);
                    if (maxrollBuildJson != null)
                    {
                        MaxrollBuildDataJson? maxrollBuildDataJson = null;
                        maxrollBuildDataJson = JsonSerializer.Deserialize<MaxrollBuildDataJson>(maxrollBuildJson.Data);
                        if (maxrollBuildJson != null)
                        {
                            // Valid json - Save and refresh available builds.
                            Directory.CreateDirectory(@".\Builds\Maxroll");
                            File.WriteAllText(@$".\Builds\Maxroll\{build}.json", json);
                            LoadAvailableMaxrollBuilds();
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid response. uri: {uri}");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, MethodBase.GetCurrentMethod()?.Name);
            }
        }

        private string GetItemTypeFromItemId(string itemId)
        {
            List<string> itemIdParts = itemId.Split('_').ToList();
            List<string> itemTypes = new List<string>
            {
                "1HAxe",
                "1HDagger",
                "1HFocus",
                "1HMace",
                "1HScythe",
                "1HShield",
                "1HSword",
                "1HTotem",
                "1HWand",
                "2HAxe",
                "2HBow",
                "2HCrossbow",
                "2HGlaive",
                "2HMace",
                "2HPolearm",
                "2HQuarterstaff",
                "2HScythe",
                "2HStaff",
                "2HSword",
                "Amulet",
                "Boots",
                "Chest",
                "Gloves",
                "Helm",
                "Pants",
                "Ring"
            };

            for (int i = 0; i < itemIdParts.Count; i++)
            {
                if (itemTypes.Contains(itemIdParts[i]))
                {
                    return itemIdParts[i];
                }
            }

            return itemIdParts[0];
        }

        public void RemoveMaxrollBuild(string buildId)
        {
            string directory = @".\Builds\Maxroll";
            File.Delete(@$"{directory}\{buildId}.json");
            LoadAvailableMaxrollBuilds();
        }

        private void LoadAvailableMaxrollBuilds()
        {
            try
            {
                MaxrollBuilds.Clear();

                string directory = @".\Builds\Maxroll";
                if (Directory.Exists(directory))
                {
                    var fileEntries = Directory.EnumerateFiles(directory).Where(tooltip => tooltip.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
                    foreach (string fileName in fileEntries)
                    {
                        string json = File.ReadAllText(fileName);
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            MaxrollBuildJson? maxrollBuildJson = JsonSerializer.Deserialize<MaxrollBuildJson>(json);
                            if (maxrollBuildJson != null)
                            {
                                MaxrollBuildDataJson? maxrollBuildDataJson = null;
                                maxrollBuildDataJson = JsonSerializer.Deserialize<MaxrollBuildDataJson>(maxrollBuildJson.Data);
                                if (maxrollBuildDataJson != null)
                                {
                                    MaxrollBuild maxrollBuild = new MaxrollBuild
                                    {
                                        Data = maxrollBuildDataJson,
                                        Date = maxrollBuildJson.Date,
                                        Id = maxrollBuildJson.Id,
                                        Name = maxrollBuildJson.Name
                                    };

                                    MaxrollBuilds.Add(maxrollBuild);
                                }
                            }
                        }
                    }

                    WeakReferenceMessenger.Default.Send(new MaxrollBuildsLoadedMessage());
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, MethodBase.GetCurrentMethod()?.Name);
            }
        }

        #endregion
    }
}
