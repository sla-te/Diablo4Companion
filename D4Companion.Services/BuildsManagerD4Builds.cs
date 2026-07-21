using CommunityToolkit.Mvvm.Messaging;
using D4Companion.Entities;
using D4Companion.Helpers;
using D4Companion.Interfaces;
using D4Companion.Messages;
using D4Companion.Services.BuildAdapters;
using FuzzierSharp;
using FuzzierSharp.SimilarityRatio;
using FuzzierSharp.SimilarityRatio.Scorer.Composite;
using FuzzierSharp.SimilarityRatio.Scorer.StrategySensitive;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace D4Companion.Services
{
    public class BuildsManagerD4Builds : IBuildsManagerD4Builds
    {
        private readonly ILogger _logger;
        private readonly IAffixManager _affixManager;
        private readonly IBuildPresetProjector _projector;
        private readonly ISettingsManager _settingsManager;
        private readonly D4BuildsBuildAdapter _d4BuildsBuildAdapter = new();

        private static readonly int _delayVariant = 100;
        private static readonly int _delayTab = 100;

        private List<AffixInfo> _affixes = new List<AffixInfo>();
        private List<string> _affixDescriptions = new List<string>();
        private Dictionary<string, string> _affixMapDescriptionToId = new Dictionary<string, string>();
        private List<AspectInfo> _aspects = new List<AspectInfo>();
        private List<string> _aspectNames = new List<string>();
        private Dictionary<string, string> _aspectMapNameToId = new Dictionary<string, string>();
        private List<D4BuildsBuild> _d4BuildsBuilds = new();
        private List<RuneInfo> _runes = new List<RuneInfo>();
        private List<string> _runeNames = new List<string>();
        private Dictionary<string, string> _runeMapNameToId = new Dictionary<string, string>();
        private List<UniqueInfo> _uniques = new List<UniqueInfo>();
        private List<string> _uniqueNames = new List<string>();
        private Dictionary<string, string> _uniqueMapNameToId = new Dictionary<string, string>();
        private WebDriver? _webDriver = null;
        private WebDriverWait? _webDriverWait = null;
        private int _webDriverProcessId = 0;

        // Start of Constructors region

        #region Constructors

        public BuildsManagerD4Builds(ILogger<BuildsManagerD4Builds> logger, IAffixManager affixManager, IBuildPresetProjector projector, ISettingsManager settingsManager)
        {
            // Init services
            _affixManager = affixManager;
            _logger = logger;
            _projector = projector;
            _settingsManager = settingsManager;

            // Init data
            InitAffixData();
            InitAspectData();
            InitRuneData();
            InitUniqueData();

            // Load available D4Builds builds.
            Task.Factory.StartNew(() =>
            {
                LoadAvailableD4BuildsBuilds();
            });
        }

        #endregion

        // Start of Events region

        #region Events

        #endregion

        // Start of Properties region

        #region Properties

        public List<D4BuildsBuild> D4BuildsBuilds { get => _d4BuildsBuilds; set => _d4BuildsBuilds = value; }

        #endregion

        // Start of Event handlers region

        #region Event handlers

        #endregion

        // Start of Methods region

        #region Methods

        private void InitAffixData()
        {
            _affixes.Clear();
            string resourcePath = @".\Data\Affixes.enUS.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    _affixes = JsonSerializer.Deserialize<List<AffixInfo>>(stream, options) ?? new List<AffixInfo>();
                }
            }

            // Create affix description list for FuzzierSharp
            _affixDescriptions.Clear();
            _affixDescriptions = _affixes.Select(affix =>
            {
                // Remove class restrictions from description. D4builds does not show this information.
                return affix.DescriptionClean.Contains(")") ? affix.DescriptionClean.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0] : affix.DescriptionClean;
            }).ToList();

            // Create dictionary to map affix description with affix id
            _affixMapDescriptionToId.Clear();
            _affixMapDescriptionToId = _affixes.ToDictionary(affix =>
            {
                // Remove class restrictions from description. D4builds does not show this information.
                return affix.DescriptionClean.Contains(")") ? affix.DescriptionClean.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[0] : affix.DescriptionClean;
            }, affix => affix.IdName);
        }

        private void InitAspectData()
        {
            _aspects.Clear();
            string resourcePath = @".\Data\Aspects.enUS.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    _aspects = JsonSerializer.Deserialize<List<AspectInfo>>(stream, options) ?? new List<AspectInfo>();
                }
            }

            // Create aspect name list for FuzzierSharp
            _aspectNames.Clear();
            _aspectNames = _aspects.Select(aspect => aspect.Name).ToList();

            // Create dictionary to map aspect name with aspect id
            _aspectMapNameToId.Clear();
            _aspectMapNameToId = _aspects.ToDictionary(aspect => aspect.Name, aspect => aspect.IdName);
        }

        private void InitRuneData()
        {
            _runes.Clear();
            string resourcePath = @".\Data\Runes.enUS.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    _runes = JsonSerializer.Deserialize<List<RuneInfo>>(stream, options) ?? new List<RuneInfo>();
                }
            }

            // Create rune name list for FuzzierSharp
            _runeNames.Clear();
            _runeNames = _runes.Select(rune => rune.Name).ToList();

            // Create dictionary to map rune name with run id
            _runeMapNameToId.Clear();
            _runeMapNameToId = _runes.ToDictionary(rune => rune.Name, rune => rune.IdName);
        }

        private void InitUniqueData()
        {
            _uniques.Clear();
            string resourcePath = @".\Data\Uniques.enUS.json";
            using (FileStream? stream = File.OpenRead(resourcePath))
            {
                if (stream != null)
                {
                    // create the options
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    // register the converter
                    options.Converters.Add(new BoolConverter());
                    options.Converters.Add(new IntConverter());

                    _uniques = JsonSerializer.Deserialize<List<UniqueInfo>>(stream, options) ?? new List<UniqueInfo>();
                }
            }

            // Create unique name list for FuzzierSharp
            _uniqueNames.Clear();
            _uniqueNames = _uniques.Select(unique => unique.Name).ToList();

            // Create dictionary to map unique name with unique id
            _uniqueMapNameToId.Clear();
            //_uniqueMapNameToId = _uniques.ToDictionary(unique => unique.Name, unique => unique.IdName);
            foreach (var unique in _uniques)
            {
                if (!_uniqueMapNameToId.ContainsKey(unique.Name))
                {
                    _uniqueMapNameToId.Add(unique.Name, unique.IdName);
                }
            }
        }

        private void InitSelenium()
        {
            // Options: Headless, size, security, ...
            var options = new ChromeOptions();

            // Note: ChromeDriver 129 is bugged and causes blank window when using headless mode. Test again with the release of 130.
            //options.AddArgument("--headless=old"); //v129 and older
            options.AddArgument("--headless"); // v130+

            // Note: ChromeDriver DevToolsActivePort file doesn't exist exceptions. Below fix might be needed in combination with "--headless=old"
            // https://issues.chromium.org/issues/42323434#comment36
            //options.AddArgument("--remote-debugging-pipe");

            options.AddArgument("--disable-gpu"); // Applicable to windows os only

            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--dns-prefetch-disable");
            options.AddArgument("--disable-dev-shm-usage"); // Overcome limited resource problems
            options.AddArgument("--no-sandbox"); // Bypass OS security model
            options.AddArgument("--window-size=1600,900");
            options.AddArgument("--window-position=-32000,-32000");

            // Service
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;

            // Create driver
            _webDriver = new ChromeDriver(service: service, options: options);
            _webDriverWait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(10));
            _webDriverProcessId = service.ProcessId;
        }

        public void CreatePresetFromD4BuildsBuild(D4BuildsBuildVariant d4BuildsBuild, string buildNameOriginal, string buildName)
        {
            buildName = string.IsNullOrWhiteSpace(buildName) ? buildNameOriginal : buildName;

            // Note: Only allow one D4Builds build. Update if already exists.
            _affixManager.AffixPresets.RemoveAll(p => p.Name.Equals(buildName));

            var affixPreset = d4BuildsBuild.AffixPreset.Clone();
            affixPreset.Name = buildName;

            _affixManager.AddAffixPreset(affixPreset);
        }

        public void DownloadD4BuildsBuild(string buildIdD4Builds)
        {
            bool isLegacyPage = false;
            try
            {
                if (_webDriver == null) InitSelenium();
                if (_webDriver == null) throw new Exception("WebDriver initialization failed.");
                if (_webDriverWait == null) throw new Exception("WebDriverWait initialization failed.");

                D4BuildsBuild d4BuildsBuild = new D4BuildsBuild
                {
                    Id = buildIdD4Builds
                };

                //var watch = System.Diagnostics.Stopwatch.StartNew();
                WeakReferenceMessenger.Default.Send(new D4BuildsStatusUpdateMessage(new D4BuildsStatusUpdateMessageParams
                {
                    Build = d4BuildsBuild,
                    Status = $"Downloading {d4BuildsBuild.Id}."
                }));

                _webDriver.Navigate().GoToUrl($"https://d4builds.gg/builds/{buildIdD4Builds}/?var=0");

                try
                {
                    _webDriverWait.Until(e => !string.IsNullOrEmpty(e.FindElement(By.Id("renameBuild")).GetAttribute("value")));
                    isLegacyPage = true;
                }
                catch
                {
                    // ignored
                }

                // TODO: Need a better _webDriverWait.Until or other check for page load status.
                // Extra sleep to make sure page is loaded.
                Thread.Sleep(5000);

                // Build name
                if(isLegacyPage)
                {
                    d4BuildsBuild.Name = _webDriver.FindElement(By.Id("renameBuild")).GetAttribute("value") ?? string.Empty;
                }
                else
                {
                    d4BuildsBuild.Name = _webDriver.FindElement(By.ClassName("builder__header__description")).GetAttribute("innerHTML") ?? string.Empty;
                }

                WeakReferenceMessenger.Default.Send(new D4BuildsStatusUpdateMessage(new D4BuildsStatusUpdateMessageParams
                    {
                        Build = d4BuildsBuild,
                        Status = $"Downloaded {d4BuildsBuild.Name}."
                    }));

                // Last update
                d4BuildsBuild.Date = GetLastUpdateInfo();

                // Variants
                //watch = System.Diagnostics.Stopwatch.StartNew();
                ExportBuildVariants(d4BuildsBuild);
                //watch.Stop();
                //System.Diagnostics.Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name} (Export): Elapsed time: {watch.ElapsedMilliseconds}");
                //watch = System.Diagnostics.Stopwatch.StartNew();
                ConvertBuildVariants(d4BuildsBuild);
                //watch.Stop();
                //System.Diagnostics.Debug.WriteLine($"{MethodBase.GetCurrentMethod()?.Name} (Convert): Elapsed time: {watch.ElapsedMilliseconds}");

                // Save
                Directory.CreateDirectory(@".\Builds\D4Builds");
                using (FileStream stream = File.Create(@$".\Builds\D4Builds\{d4BuildsBuild.Id}.json"))
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    JsonSerializer.Serialize(stream, d4BuildsBuild, options);
                }
                LoadAvailableD4BuildsBuilds();

                WeakReferenceMessenger.Default.Send(new D4BuildsStatusUpdateMessage(new D4BuildsStatusUpdateMessageParams
                {
                    Build = d4BuildsBuild,
                    Status = $"Done."
                }));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"{MethodBase.GetCurrentMethod()?.Name} ({buildIdD4Builds})");

                WeakReferenceMessenger.Default.Send(new ErrorOccurredMessage(new ErrorOccurredMessageParams
                {
                    Message = $"Failed to download from D4Builds ({buildIdD4Builds})"
                }));

                WeakReferenceMessenger.Default.Send(new D4BuildsStatusUpdateMessage(new D4BuildsStatusUpdateMessageParams
                {
                    Build = new D4BuildsBuild { Id = buildIdD4Builds },
                    Status = $"Failed."
                }));
            }
            finally
            {
                // Kill process because of issue with lingering Chrome processes.
                var process = System.Diagnostics.Process.GetProcesses().FirstOrDefault(p => p.Id == _webDriverProcessId);
                process?.Kill(true);
                process?.WaitForExit(1000);

                // The following fix to close Chrome processes the correct way does not always work.
                // Note: You need to call driver.close() before driver.quit() otherwise you get lingering chrome processes with high resource usage.
                // This is an issue with recent chrome versions (124+).
                //_webDriver?.Close(); // Can't use Close() in combination with process?.Kill(true).
                _webDriver?.Quit();
                _webDriver?.Dispose();
                _webDriver = null;
                _webDriverWait = null;

                WeakReferenceMessenger.Default.Send(new D4BuildsCompletedMessage());
            }
        }

        private void ConvertBuildVariants(D4BuildsBuild d4BuildsBuild)
        {
            foreach (var variant in d4BuildsBuild.Variants)
            {
                WeakReferenceMessenger.Default.Send(new D4BuildsStatusUpdateMessage(new D4BuildsStatusUpdateMessageParams
                {
                    Build = d4BuildsBuild,
                    Status = $"Converting {variant.Name}."
                }));

                var canonicalBuild = _d4BuildsBuildAdapter.ToCanonical(variant, d4BuildsBuild.Name);
                var canonicalVariant = canonicalBuild.Variants[0];

                foreach (var item in canonicalVariant.Items)
                {
                    // Resolve affixes. The adapter can only stash the raw scraped affix text
                    // on the CanonicalAffix because it has no fuzzy-matching dependency;
                    // replace it with the matched AffixInfo.IdName here.
                    foreach (var affix in item.Affixes)
                    {
                        var affixResult = Process.ExtractOne(affix.Id, _affixDescriptions, scorer: ScorerCache.Get<DefaultRatioScorer>());
                        affix.Id = _affixMapDescriptionToId[affixResult.Value];
                    }

                    // Resolve aspects.
                    var resolvedAspectIds = new List<string>();
                    foreach (var aspect in item.AspectIds)
                    {
                        var aspectResult = Process.ExtractOne(aspect.Replace("Aspect", string.Empty, StringComparison.OrdinalIgnoreCase), _aspectNames, scorer: ScorerCache.Get<WeightedRatioScorer>());
                        resolvedAspectIds.Add(_aspectMapNameToId[aspectResult.Value]);
                    }
                    item.AspectIds = resolvedAspectIds;

                    // Resolve runes.
                    var resolvedRuneIds = new List<string>();
                    foreach (var rune in item.RuneIds)
                    {
                        var runeResult = Process.ExtractOne(rune, _runeNames, scorer: ScorerCache.Get<WeightedRatioScorer>());
                        resolvedRuneIds.Add(_runeMapNameToId[runeResult.Value]);
                    }
                    item.RuneIds = resolvedRuneIds;

                    // Resolve uniques. Workaround to ignore empty item slots: a low score
                    // means the scraped text was not actually a unique name.
                    var resolvedUniqueIds = new List<string>();
                    foreach (var unique in item.UniqueIds)
                    {
                        var uniqueResult = Process.ExtractOne(unique, _uniqueNames, scorer: ScorerCache.Get<WeightedRatioScorer>());
                        if (uniqueResult.Score < 90) continue;
                        resolvedUniqueIds.Add(_uniqueMapNameToId[uniqueResult.Value]);
                    }
                    item.UniqueIds = resolvedUniqueIds;
                }

                var affixPreset = _projector.Project(canonicalVariant, variant.Name);
                variant.AffixPreset = affixPreset;

                WeakReferenceMessenger.Default.Send(new D4BuildsStatusUpdateMessage(new D4BuildsStatusUpdateMessageParams
                {
                    Build = d4BuildsBuild,
                    Status = $"Converted {variant.Name}."
                }));
            }
        }

        private void ExportBuildVariants(D4BuildsBuild d4BuildsBuild)
        {
            var buttonElement = _webDriver?.FindElement(By.ClassName("item__arrow__icon--variant"));
            //buttonElement?.Click(); // Note: This requires the element to be visually clickable and could cause issues when there is a cookie banner. Use ExecuteScript instead.
            _ = _webDriver?.ExecuteScript("arguments[0].click();", buttonElement);


            Thread.Sleep(_delayVariant);

            var dropdownElements = _webDriver?.FindElements(By.ClassName("dropdown__option"));
            var count = dropdownElements?.Count ?? 0;

            for (int i = 0; i < count; i++)
            {
                string variantName = dropdownElements![i].GetAttribute("innerHTML") ?? string.Empty;
                _ = _webDriver?.ExecuteScript("arguments[0].click();", dropdownElements[i]);
                Thread.Sleep(_delayVariant);

                ExportBuildVariant(variantName, d4BuildsBuild);

                // Open dropdown menu again for next variant
                _ = _webDriver?.ExecuteScript("arguments[0].click();", buttonElement);
                Thread.Sleep(_delayVariant);
                dropdownElements = _webDriver?.FindElements(By.ClassName("dropdown__option"));
            }
        }

        private void ExportBuildVariant(string variantName, D4BuildsBuild d4BuildsBuild)
        {
            if (_webDriver == null) throw new Exception("WebDriver initialization failed.");

            // Set timeout to improve performance
            // https://stackoverflow.com/questions/16075997/iselementpresent-is-very-slow-in-case-if-element-does-not-exist
            _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(0);

            WeakReferenceMessenger.Default.Send(new D4BuildsStatusUpdateMessage(new D4BuildsStatusUpdateMessageParams
            {
                Build = d4BuildsBuild,
                Status = $"Exporting {variantName}."
            }));

            var d4BuildsBuildVariant = new D4BuildsBuildVariant
            {
                Name = variantName
            };

            var tabElements = _webDriver.FindElements(By.ClassName("builder__navigation__link"));
            var count = tabElements?.Count ?? 0;
            if (count > 0)
            {
                // Process "Gear & Skills" tab
                _ = _webDriver.ExecuteScript("arguments[0].click();", tabElements![0]);
                Thread.Sleep(_delayTab);

                // Aspects
                d4BuildsBuildVariant.Aspect = GetAllAspects();
                d4BuildsBuildVariant.Uniques = GetAllUniques();

                // Armor
                d4BuildsBuildVariant.Helm = GetAllAffixes("Helm");
                d4BuildsBuildVariant.Chest = GetAllAffixes("ChestArmor");
                d4BuildsBuildVariant.Gloves = GetAllAffixes("Gloves");
                d4BuildsBuildVariant.Pants = GetAllAffixes("Pants");
                d4BuildsBuildVariant.Boots = GetAllAffixes("Boots");

                // Accessories
                d4BuildsBuildVariant.Amulet = GetAllAffixes("Amulet");
                d4BuildsBuildVariant.Ring.AddRange(GetAllAffixes("Ring1"));
                d4BuildsBuildVariant.Ring.AddRange(GetAllAffixes("Ring2"));
                d4BuildsBuildVariant.Ring = d4BuildsBuildVariant.Ring.Distinct().ToList();

                // Weapons. D4Builds' own structural selectors already distinguish the
                // Barbarian Arsenal slots (BludgeoningWeapon / SlashingWeapon /
                // Dual-WieldWeapon1+2 = mainhand+offhand); preserve that instead of
                // merging everything into one Weapon bucket.
                d4BuildsBuildVariant.Weapon.AddRange(GetAllAffixes("Weapon"));
                d4BuildsBuildVariant.Weapon = d4BuildsBuildVariant.Weapon.Distinct().ToList();

                d4BuildsBuildVariant.WeaponBludgeoning.AddRange(GetAllAffixes("BludgeoningWeapon"));
                d4BuildsBuildVariant.WeaponBludgeoning = d4BuildsBuildVariant.WeaponBludgeoning.Distinct().ToList();

                d4BuildsBuildVariant.WeaponSlicing.AddRange(GetAllAffixes("SlashingWeapon"));
                d4BuildsBuildVariant.WeaponSlicing = d4BuildsBuildVariant.WeaponSlicing.Distinct().ToList();

                d4BuildsBuildVariant.WeaponOneHand.AddRange(GetAllAffixes("Dual-WieldWeapon1"));
                d4BuildsBuildVariant.WeaponOneHand.AddRange(GetAllAffixes("Dual-WieldWeapon2"));
                d4BuildsBuildVariant.WeaponOneHand = d4BuildsBuildVariant.WeaponOneHand.Distinct().ToList();

                d4BuildsBuildVariant.Ranged = GetAllAffixes("RangedWeapon");
                d4BuildsBuildVariant.Offhand = GetAllAffixes("Offhand");

                // Runes
                d4BuildsBuildVariant.Runes = GetAllRunes();

                // Process "Paragon" tab
                if (_settingsManager.Settings.IsImportParagonD4BuildsEnabled)
                {
                    _ = _webDriver.ExecuteScript("arguments[0].click();", tabElements[2]);
                    Thread.Sleep(_delayTab);

                    // Paragon
                    d4BuildsBuildVariant.ParagonBoards = GetAllParagonBoards(d4BuildsBuild);
                }

                d4BuildsBuild.Variants.Add(d4BuildsBuildVariant);
                WeakReferenceMessenger.Default.Send(new D4BuildsStatusUpdateMessage(new D4BuildsStatusUpdateMessageParams
                {
                    Build = d4BuildsBuild,
                    Status = $"Exported {variantName}."
                }));

                // Reset Timeout
                _webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(10 * 1000);
            }
        }

        private List<string> GetAllAspects()
        {
            try
            {
                // Note: Text property for weapon slots is empty. Use innerHTML instead.
                //_webDriver.FindElements(By.ClassName("builder__gear__name")).Select(e => e.Text).ToList();

                return _webDriver!.FindElements(By.ClassName("builder__gear__name"))
                    .Select(e => e.GetAttribute("innerHTML")).OfType<string>()
                    .Where(e => e.Contains("Aspect")).ToList();
            }
            catch (Exception)
            {
                return new();
            }
        }

        private List<D4buildsAffix> GetAllAffixes(string itemType)
        {
            try
            {
                List<D4buildsAffix> affixes = new List<D4buildsAffix>();

                // Find the element with affixes
                var elementAffixes = _webDriver!.FindElement(By.CssSelector($".builder__stats__group.{itemType}")).FindElements(By.ClassName("builder__stat"));
                foreach (var elementAffix in elementAffixes) 
                {
                    try
                    {
                        D4buildsAffix d4buildsAffix = new D4buildsAffix();
                        var asHtml = elementAffix.GetAttribute("outerHTML") ?? string.Empty;

                        d4buildsAffix.IsGreater = asHtml.Contains("greater__affix__button--filled");
                        d4buildsAffix.IsImplicit = asHtml.Contains("implicit");
                        d4buildsAffix.IsTempered = asHtml.Contains("tempering");
                        d4buildsAffix.AffixText = elementAffix.FindElement(By.ClassName("filled")).GetAttribute("innerText") ?? string.Empty;

                        // Clean string
                        if (d4buildsAffix.IsImplicit)
                        {
                            d4buildsAffix.AffixText = d4buildsAffix.AffixText.Contains(":") ? d4buildsAffix.AffixText.Substring(d4buildsAffix.AffixText.IndexOf(":") + 1) : d4buildsAffix.AffixText;
                        }
                        else if (d4buildsAffix.IsTempered)
                        {
                            d4buildsAffix.AffixText = Regex.Replace(d4buildsAffix.AffixText, @"\[(.+?)\]", string.Empty);
                            d4buildsAffix.AffixText = Regex.Replace(d4buildsAffix.AffixText, @"\((.+?)\)", string.Empty);
                        }
                        d4buildsAffix.AffixText = String.Concat(d4buildsAffix.AffixText.Where(c =>
                            (c < '0' || c > '9') &&
                            (c != '[') &&
                            (c != ']') &&
                            (c != '(') &&
                            (c != ')') &&
                            (c != '+') &&
                            (c != '-') &&
                            (c != '.') &&
                            (c != ',') &&
                            (c != '%'))).Trim();

                        affixes.Add(d4buildsAffix);
                    }
                    catch
                    {
                        continue;
                    }
                }
                return affixes;
            }
            catch (Exception)
            {
                return new();
            }
        }

        private List<string> GetAllRunes()
        {
            try
            {
                return _webDriver!.FindElements(By.ClassName("builder__gem__slot"))
                    .Select(e => e.GetAttribute("innerHTML"))
                    .OfType<string>()
                    .Where(e => e.Length == 3 || e.Length == 4).ToList();
            }
            catch (Exception)
            {
                return new();
            }
        }

        private List<string> GetAllUniques()
        {
            try
            {
                // Note: Text property for weapon slots is empty. Use innerHTML instead.
                //_webDriver!.FindElements(By.ClassName("builder__gear__name")).Select(e => e.Text).Where(e => !e.Contains("Aspect")).ToList();

                return _webDriver!.FindElements(By.ClassName("builder__gear__name"))
                    .Select(e => e.GetAttribute("innerHTML")).OfType<string>()
                    .Where(e => !e.Contains("Aspect")).ToList();
            }
            catch (Exception)
            {
                return new();
            }
        }

        private List<ParagonBoard> GetAllParagonBoards(D4BuildsBuild d4BuildsBuild)
        {
            List<ParagonBoard> paragonBoards = new List<ParagonBoard>();

            // Get all boards
            var boardElements = _webDriver?.FindElements(By.ClassName("paragon__board"));
            var countBoards = boardElements?.Count ?? 0;
            for (int i = 0; i < countBoards; i++)
            {
                string name = boardElements![i].FindElement(By.ClassName("paragon__board__name")).GetAttribute("innerText") ?? string.Empty;
                var lines = name.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAll(e => e.Length <= 1);
                name = lines.FirstOrDefault() ?? "unknown";
                string glyph = string.Empty;
                var possibleGlyph = boardElements[i].FindElements(By.ClassName("paragon__board__name__glyph"));
                if (possibleGlyph.Any())
                {
                    glyph = possibleGlyph[0].GetAttribute("innerText") ?? string.Empty;
                }
                string rotateString = boardElements[i].GetAttribute("style") ?? string.Empty;
                glyph = glyph.Replace("(", string.Empty).Replace(")", string.Empty);

                WeakReferenceMessenger.Default.Send(new D4BuildsStatusUpdateMessage(new D4BuildsStatusUpdateMessageParams
                {
                    Build = d4BuildsBuild,
                    Status = $"Paragon: {name} ({glyph})."
                }));

                // Convert rotate string
                int rotateInt = 0;
                string subStringBegin = "rotate(";
                string subStringEnd = "deg)";
                if (rotateString.Contains(subStringBegin))
                {
                    rotateString = rotateString.Substring(rotateString.IndexOf(subStringBegin) + subStringBegin.Length,
                        rotateString.IndexOf(subStringEnd) - (rotateString.IndexOf(subStringBegin) + subStringBegin.Length));
                    rotateInt = int.Parse(rotateString) % 360;
                }

                var paragonBoard = new ParagonBoard();
                paragonBoard.Name = name;
                paragonBoard.Glyph = glyph;
                string rotationInfo = rotateInt == 0 ? "0°" :
                                rotateInt == 90 ? "90°" :
                                rotateInt == 180 ? "180°" :
                                rotateInt == 270 ? "270°" : "?°";
                paragonBoard.Rotation = rotationInfo;
                paragonBoards.Add(paragonBoard);

                // Get all nodes
                var tileElements = boardElements[i].FindElements(By.ClassName("paragon__board__tile"));
                var countTiles = tileElements?.Count ?? 0;
                for (int j = 0; j < countTiles; j++)
                {
                    // Example "paragon__board__tile r2 c10 active enabled"
                    string htmlclass = tileElements![j].GetAttribute("class") ?? string.Empty;
                    if (!htmlclass.Contains("active")) continue;

                    var nodeInfo = htmlclass.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    int locationX = int.Parse(string.Concat(nodeInfo[2].Where(Char.IsDigit)));
                    int locationY = int.Parse(string.Concat(nodeInfo[1].Where(Char.IsDigit)));
                    int locationXT = locationX;
                    int locationYT = locationY;

                    if (rotateInt == 0)
                    {
                        locationXT = locationXT - 1;
                        locationYT = locationYT - 1;
                    }
                    else if (rotateInt == 90)
                    {
                        locationXT = 21 - locationY;
                        locationYT = locationX;
                        locationYT = locationYT - 1;
                    }
                    else if (rotateInt == 180)
                    {
                        locationXT = 21 - locationX;
                        locationYT = 21 - locationY;
                    }
                    else if (rotateInt == 270)
                    {
                        locationXT = locationY;
                        locationYT = 21 - locationX;
                        locationXT = locationXT - 1;
                    }
                    paragonBoard.Nodes[locationYT * 21 + locationXT] = true;
                }
            }

            return paragonBoards;
        }

        private string GetLastUpdateInfo()
        {
            try
            {
                return _webDriver!.FindElement(By.ClassName("builder__last__updated")).Text;
            }
            catch (Exception)
            {
                return DateTime.Now.ToString();
            }
        }

        public void RemoveD4BuildsBuild(string buildId)
        {
            string directory = @".\Builds\D4Builds";
            File.Delete(@$"{directory}\{buildId}.json");
            LoadAvailableD4BuildsBuilds();
        }

        private void LoadAvailableD4BuildsBuilds()
        {
            try
            {
                D4BuildsBuilds.Clear();

                string directory = @".\Builds\D4Builds";
                if (Directory.Exists(directory))
                {
                    var fileEntries = Directory.EnumerateFiles(directory).Where(tooltip => tooltip.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
                    foreach (string fileName in fileEntries)
                    {
                        string json = File.ReadAllText(fileName);
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            D4BuildsBuild? d4BuildsBuild = JsonSerializer.Deserialize<D4BuildsBuild>(json);
                            if (d4BuildsBuild != null)
                            {
                                D4BuildsBuilds.Add(d4BuildsBuild);
                            }
                        }
                    }

                    WeakReferenceMessenger.Default.Send(new D4BuildsBuildsLoadedMessage());
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
