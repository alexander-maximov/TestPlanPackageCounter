using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using TestplanPackageCounter.Counter;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V2;
using TestplanPackageCounter.Packages.Content.V2.Analytics;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events;
using TestplanPackageCounter.Packages.Converters.V2;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal class PackagesEnumeratorV2 : CommonEnumerator
    {
        private readonly string _pathToResults;

        Dictionary<string, Dictionary<string, List<ProxyPackageInfoV2>>> _deserializedPackages =
                new Dictionary<string, Dictionary<string, List<ProxyPackageInfoV2>>>();

        private List<ProxyPackageInfoV2> _previousTestPackages = new List<ProxyPackageInfoV2>();
        private List<string> _platformList = new List<string>();

        internal Dictionary<string, Dictionary<string, int>> PackagesDictionary { get; set; }

        internal Dictionary<string, int> MaxUeDictionary { get; set; }

        internal PackagesEnumeratorV2(string pathToResults)
        {
            this._pathToResults = pathToResults;
            this.MaxUeDictionary = new Dictionary<string, int>();
            this._platformList = this.GetPlatformList(pathToResults);
        }

        internal void Enumerate(Dictionary<string, bool> testBeforeCleanDictionary)
        {
            Dictionary<string, Dictionary<string, int>> packagesDictionary =
                new Dictionary<string, Dictionary<string, int>>();

            this.DeserializeAllPackagesV2();
            packagesDictionary = this.EnumeratePackagesV2(testBeforeCleanDictionary);

            this.PackagesDictionary = this.ConvertPackageDictionary(packagesDictionary);
        }

        /// <summary>
        /// Get list of platforms from results.
        /// </summary>
        /// <param name="resultsPath">Path to results to generate list of platforms based on folder names.</param>
        /// <returns>List of platforms.</returns>
        private List<string> GetPlatformList(string resultsPath)
        {
            List<string> platformList = new List<string>();

            foreach (string directory in Directory.GetDirectories(resultsPath))
            {
                string platformName = Path.GetFileName(directory);

                platformList.Add(platformName);
            }

            return platformList;
        }

        /// <summary>
        /// Deserialize all result packages and dictionary with packages data.
        /// </summary>
        private void DeserializeAllPackagesV2()
        {
            JsonSerializerSettings packageSerializationSettings = new JsonSerializerSettings
            {
                CheckAdditionalContent = true,
                MissingMemberHandling = MissingMemberHandling.Error,
                ReferenceLoopHandling = ReferenceLoopHandling.Error,
                DefaultValueHandling = DefaultValueHandling.Populate,
                ContractResolver = new EscapedStringResolver(),
                FloatFormatHandling = FloatFormatHandling.String,
                FloatParseHandling = FloatParseHandling.Double,
                Culture = CultureInfo.InvariantCulture
            };

            packageSerializationSettings.Converters.Add(new CommonConverterV2());
            packageSerializationSettings.Converters.Add(new EventConverter());
            packageSerializationSettings.Converters.Add(new RequestJsonConverterV2());
            packageSerializationSettings.Converters.Add(new ResponseJsonConverter());

            foreach (string directory in Directory.GetDirectories(this._pathToResults))
            {
                string platformName = Path.GetFileName(directory);

                Dictionary<string, List<ProxyPackageInfoV2>> platformPackagesDictionary =
                    new Dictionary<string, List<ProxyPackageInfoV2>>();

                foreach (string subDirectory in Directory.GetDirectories(directory))
                {
                    List<ProxyPackageInfoV2> packagesList = new List<ProxyPackageInfoV2>();

                    IEnumerable<string> jsonFiles = Directory.GetFiles(subDirectory, "*.json");

                    foreach (string jsonFile in jsonFiles)
                    {
                        string packageContent = File.ReadAllText(jsonFile, Encoding.UTF8);

                        ProxyPackageInfoV2 deserializedJsonV1 = JsonConvert.DeserializeObject<ProxyPackageInfoV2>(
                                packageContent,
                                packageSerializationSettings
                            );

                        packagesList.Add(deserializedJsonV1);
                    }

                    string testName = Path.GetFileName(subDirectory);

                    platformPackagesDictionary.Add(testName, packagesList);
                }

                this._deserializedPackages.Add(platformName, platformPackagesDictionary);
            }
        }

        /// <summary>
        /// Convert packages dictionary from platform separated dictionary of dictionary of test and packages count 
        /// to dictionary of tests and packages count, separated by platform.
        /// </summary>
        /// <param name="packagesDictionary">Source dictionary.</param>
        /// <param name="platformList">List of platforms to separate packages count.</param>
        /// <returns>Dictionary of tests and packages count, separated by platform.</returns>
        private Dictionary<string, Dictionary<string, int>> ConvertPackageDictionary(
            Dictionary<string, Dictionary<string, int>> packagesDictionary
        )
        {
            Dictionary<string, Dictionary<string, int>> convertedDictionary =
                new Dictionary<string, Dictionary<string, int>>();

            foreach (var platform in packagesDictionary)
            {
                foreach (var testName in platform.Value)
                {
                    string upperKey = testName.Key.ToUpper();

                    if (convertedDictionary.ContainsKey(upperKey))
                    {
                        convertedDictionary[upperKey][platform.Key] = testName.Value;
                        continue;
                    }

                    Dictionary<string, int> innerDictionary = new Dictionary<string, int>
                    {
                        { platform.Key, testName.Value }
                    };

                    convertedDictionary.Add(upperKey, innerDictionary);
                }
            }

            foreach (var testName in convertedDictionary)
            {
                foreach (var platformName in this._platformList)
                {
                    if (!testName.Value.Keys.Contains(platformName))
                    {
                        testName.Value.Add(platformName, -1);
                    }
                }
            }

            return convertedDictionary;
        }

        /// <summary>
        /// Calculates packages in given result folder.
        /// </summary>
        /// <returns>Platform separated dictionary of dictionary of test and packages.</returns>
        private Dictionary<string, Dictionary<string, int>> EnumeratePackagesV2(
            Dictionary<string, bool> testBeforeCleanDictionary = null
        )
        {
            Dictionary<string, Dictionary<string, int>> packagesCountDictionary =
                new Dictionary<string, Dictionary<string, int>>();

            foreach (var deserializedPlatformPackages in this._deserializedPackages)
            {
                string platformName = deserializedPlatformPackages.Key;

                Dictionary<string, List<ProxyPackageInfoV2>> platformPackages =
                    deserializedPlatformPackages.Value;
                Dictionary<string, int> platformPackagesCount =
                    new Dictionary<string, int>();

                foreach (var deserializedTestPackages in platformPackages)
                {
                    string testName = deserializedTestPackages.Key;

                    #region for debug
                    //TODO: remove me
                    string compareString = "StartAfterSwitchBeforeInit".ToLower();

                    if (testName.ToLower().Contains(compareString))
                    {
                        Console.Write("");
                    }
                    #endregion                    

                    List<ProxyPackageInfoV2> testPackagesOriginal =
                        new List<ProxyPackageInfoV2>(deserializedTestPackages.Value);

                    this._previousTestPackages = new List<ProxyPackageInfoV2>(deserializedTestPackages.Value);

                    List<ProxyPackageInfoV2> testPackages =
                        GetTestPackagesWithoutDoubles(deserializedTestPackages.Value);

                    bool showTestName = false;

                    if (testPackagesOriginal.Count != testPackages.Count)
                    {
                        var excludedPackages = testPackagesOriginal.Except(testPackages);

                        Console.WriteLine($"{platformName} {testName}");
                        showTestName = true;

                        foreach (var excludedPackage in excludedPackages)
                        {
                            NameValueCollection paramsUrl =
                                HttpUtility.ParseQueryString(new UriBuilder(excludedPackage.RequestUrl).Query);

                            Console.WriteLine($"Double package signature: s={paramsUrl["s"]}");
                        }
                    }

                    List<AlV2> alEvents = testPackages.AllEventsOfType<AlV2>().OrderBy(e => e.Timestamp).ToList();

                    #region find last al
                    IEnumerable<IHasTimestamp> timestampEvents = testPackages.AllEventsOfType<IHasTimestamp>().OrderBy(e => e.Timestamp).ToList();

                    AlV2 lastAlEvent = null;

                    if (timestampEvents.Count() != 0 && alEvents.Count() != 0)
                    {
                        if (timestampEvents.Last() is AlV2 alEvent
                            && alEvent == alEvents.Last())
                        {
                            lastAlEvent = alEvent;
                        }
                    }

                    ProxyPackageInfoV2 packageWithLastAlEvent = lastAlEvent.FindPackage(testPackages);
                    #endregion

                    #region find alPackages
                    List<ProxyPackageInfoV2> packagesWithAlEvents = new List<ProxyPackageInfoV2>();

                    foreach (AlV2 alEvent in alEvents)
                    {
                        packagesWithAlEvents.Add(alEvent.FindPackage(testPackages));
                    }

                    packagesWithAlEvents = packagesWithAlEvents.Distinct().ToList();
                    #endregion

                    #region find Ue packages
                    List<ProxyPackageInfoV2> packagesWithUeEvents = new List<ProxyPackageInfoV2>();
                    IEnumerable<UeV2> ueEvents = testPackages.AllEventsOfType<UeV2>();

                    foreach (UeV2 ueEvent in ueEvents)
                    {
                        packagesWithUeEvents.Add(ueEvent.FindPackage(testPackages));
                    }

                    packagesWithUeEvents = packagesWithUeEvents.Distinct().ToList();
                    #endregion

                    platformPackagesCount.Add(
                        testName,
                        testPackages.Count() - packagesWithUeEvents.Count() - (packageWithLastAlEvent != null ? 1 : 0)
                    );

                    bool lastUePackageRemoved = false;

                    if (testBeforeCleanDictionary != null
                        && testBeforeCleanDictionary.ContainsKey(testName.ToUpper())
                        && testBeforeCleanDictionary[testName.ToUpper()]
                    )
                    {
                        packagesWithAlEvents.Remove(packageWithLastAlEvent);

                        if (!showTestName)
                        {
                            Console.WriteLine($"{platformName} {testName}");
                            showTestName = true;
                        }

                        Console.WriteLine("Last Ue dropped.");
                        #region for doubles Ue report
                        lastUePackageRemoved = true;
                        #endregion
                    }

                    int maxAlPackagesCount = packagesWithUeEvents.Count;
                    /*
                    #region for doubles Ue report
                    if (this.PlatformUeDictionary.ContainsKey(platformName))
                    {
                        this.PlatformUeDictionary[platformName].Add(testName, (maxUePackagesCount, lastUePackageRemoved));
                    }
                    else
                    {
                        this.PlatformUeDictionary.Add(platformName, new Dictionary<string, (int, bool)>());
                        this.PlatformUeDictionary[platformName].Add(testName, (maxUePackagesCount, lastUePackageRemoved));
                    }
                    #endregion  
                    */

                    string ueDictionaryTestName = testName.ToUpper();

                    if (this.MaxUeDictionary.ContainsKey(ueDictionaryTestName))
                    {
                        this.MaxUeDictionary[ueDictionaryTestName] = maxAlPackagesCount > this.MaxUeDictionary[ueDictionaryTestName]
                            ? maxAlPackagesCount
                            : this.MaxUeDictionary[ueDictionaryTestName];
                    }
                    else
                    {
                        this.MaxUeDictionary.Add(ueDictionaryTestName, maxAlPackagesCount);
                    }

                    if (showTestName)
                    {
                        Console.WriteLine("---------------------------------------------------");
                    }
                }

                packagesCountDictionary.Add(platformName, platformPackagesCount);
            }

            return packagesCountDictionary;
        }

        private List<ProxyPackageInfoV2> GetTestPackagesWithoutDoubles(
            List<ProxyPackageInfoV2> deserializedTestPackages
        )
        {
            List<ProxyPackageInfoV2> testPackages = deserializedTestPackages;

            PackageComparerV2 packageComparer = new PackageComparerV2();

            if (this._previousTestPackages.Count != 0)
            {
                List<ProxyPackageInfoV2> testPackagesToRemove = new List<ProxyPackageInfoV2>();

                foreach (ProxyPackageInfoV2 testPackage in testPackages)
                {
                    if (this._previousTestPackages.Contains(testPackage, packageComparer))
                    {
                        testPackagesToRemove.Add(testPackage);
                    }
                }

                foreach (var testpackageToRemove in testPackagesToRemove)
                {
                    testPackages.Remove(testpackageToRemove);
                }
            }

            testPackages = testPackages.Distinct(packageComparer).ToList();
            this._previousTestPackages = new List<ProxyPackageInfoV2>();

            return testPackages;
        }
    }
}
