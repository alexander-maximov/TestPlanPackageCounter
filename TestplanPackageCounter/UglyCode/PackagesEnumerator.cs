using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TestplanPackageCounter.Counter;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V1;
using TestplanPackageCounter.Packages.Content.V1.Events;
using TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents;
using TestplanPackageCounter.Packages.Content.V2;
using TestplanPackageCounter.Packages.Converters.V1;
using TestplanPackageCounter.Packages.Converters.V2;

namespace TestplanPackageCounter.UglyCode
{
    internal class PackagesEnumerator
    {
        private readonly CounterSettings _counterSettings;

        private List<ProxyPackageInfoV1> _previousTestUePackages = new List<ProxyPackageInfoV1>();
        private List<string> _platformList = new List<string>();

        private Dictionary<string, Dictionary<string, List<ProxyPackageInfoV1>>> _allPackagesDeserialized =
            new Dictionary<string, Dictionary<string, List<ProxyPackageInfoV1>>>();

        internal PackagesEnumerator(CounterSettings counterSettings)
        {
            this._counterSettings = counterSettings;
        }

        internal Dictionary<string, int> MaxUeDictionary { get; set; }
        internal Dictionary<string, Dictionary<string, (int, bool)>> PlatformUeDictionary { get; set; }
        internal Dictionary<string, Dictionary<string, int>> PackagesDictionary { get; set; }        

        //TODO: this function looks like all in one function. Separate functionality.
        internal void Enumerate(Dictionary<string, bool> testBeforeCleanDictionary)
        {
            this.MaxUeDictionary = new Dictionary<string, int>();
            this.PlatformUeDictionary = new Dictionary<string, Dictionary<string, (int, bool)>>();
            this._platformList = this.GetPlatformList(this._counterSettings.PathToResults);
            //Gets packages content
            this.DeserializeAllPackagesInResults();
            //Read in dict
            this.PackagesDictionary =
                ConvertPackageDictionary(this.EnumeratePackages(testBeforeCleanDictionary));
            this.CheckMaxUe();

            if (this._counterSettings.WriteToCsv)
            {
                WriteToCsv();
            }
        }

        #region MaxUe report section
        private Dictionary<string, Dictionary<string, (int, bool)>> TurnDictionary(
            Dictionary<string, Dictionary<string, (int, bool)>> packagesDictionary
        )
        {
            Dictionary<string, Dictionary<string, (int, bool)>> turnedDictionary =
                new Dictionary<string, Dictionary<string, (int, bool)>>();

            foreach (var platformContent in packagesDictionary)
            {
                foreach (var testContent in platformContent.Value)
                {
                    string upperKey = testContent.Key.ToUpper();

                    if (turnedDictionary.ContainsKey(upperKey))
                    {
                        turnedDictionary[upperKey][platformContent.Key] = testContent.Value;
                        continue;
                    }

                    Dictionary<string, (int, bool)> innerDictionary = new Dictionary<string, (int, bool)>
                    {
                        { platformContent.Key, testContent.Value }
                    };

                    turnedDictionary.Add(upperKey, innerDictionary);
                }
            }

            foreach (var testName in turnedDictionary)
            {
                foreach (var platformName in this._platformList)
                {
                    if (!testName.Value.Keys.Contains(platformName))
                    {
                        testName.Value.Add(platformName, (-1, false));
                    }
                }
            }

            return turnedDictionary;
        }


        private void CheckMaxUe()
        {
            Dictionary<string, Dictionary<string, (int, bool)>> turnedPlatformPackages =
                this.TurnDictionary(this.PlatformUeDictionary);

            StringBuilder csvContent = new StringBuilder();

            string titleLine = "Ue Count";

            foreach (string platformName in this.PlatformUeDictionary.Keys)
            {
                titleLine += $",{platformName},Ue ignored";
            }

            titleLine += $",MaxUeCount";

            csvContent.AppendLine(titleLine);

            foreach (var testContent in turnedPlatformPackages)
            {
                string testName = testContent.Key;
                string packagesInfo = "";

                foreach (var platformContent in testContent.Value.OrderBy(e => e.Key))
                {
                    packagesInfo += $";{platformContent.Value.Item1};{platformContent.Value.Item2}";
                }

                packagesInfo += $";{this.MaxUeDictionary[testName]}";

                string newLine = string.Format("{0}{1}", testName, packagesInfo);

                csvContent.AppendLine(newLine);                
            }

            string filePath = Path.Combine(this._counterSettings.PathToResults, "ueCount.csv");

            File.WriteAllText(filePath, csvContent.ToString());
        }
        #endregion

        /// <summary>
        /// Calculates packages in given result folder.
        /// </summary>
        /// <returns>Platform separated dictionary of dictionary of test and packages.</returns>
        private Dictionary<string, Dictionary<string, int>> EnumeratePackages(
            Dictionary<string, bool> testBeforeCleanDictionary
        )
        {
            Dictionary<string, Dictionary<string, int>> packagesCountDictionary =
                new Dictionary<string, Dictionary<string, int>>();

            foreach (var deserializedPlatformPackages in this._allPackagesDeserialized)
            {
                string platformName = deserializedPlatformPackages.Key;

                Dictionary<string, List<ProxyPackageInfoV1>> platformPackages =
                    deserializedPlatformPackages.Value;
                Dictionary<string, int> platformPackagesCount =
                    new Dictionary<string, int>();

                foreach (var deserializedTestPackages in platformPackages)
                {
                    string testName = deserializedTestPackages.Key;

                    #region for debug
                    //TODO: remove me
                    string compareString = "PROGRESSIONEVENTSUITE_TWOLOCATIONWITHOUTANOTHEREVENTSBEFOREINIT".ToLower();

                    if (testName.ToLower().Contains(compareString))
                    {
                        Console.WriteLine();
                    }
                    #endregion                    

                    List<ProxyPackageInfoV1> testPackagesOriginal = deserializedTestPackages.Value;
                    List<ProxyPackageInfoV1> testPackages =
                        GetTestPackagesWithoutUeDoubles(deserializedTestPackages.Value);                    

                    IEnumerable<Dictionary<int, LuEvent>> luEventList = testPackages.GetAllLuEvents();

                    Dictionary<int, LuEvent> luEventWithLastUe = FindLuEventsWithLastUe(luEventList);

                    List<Dictionary<int, LuEvent>> luEventsWithUe =
                        this.FindPackagesForSubevents(
                            luEventList.AllSubeventsOfType<UeEvent>(),
                            luEventList
                        ).ToList();
                    
                    SaveUePackagesToNextTest(testPackagesOriginal);

                    platformPackagesCount.Add(
                        testName,
                        this.CalculatePackages(testPackages, luEventList, luEventsWithUe)
                    );

                    bool lastUePackageRemoved = false;

                    if (testBeforeCleanDictionary.ContainsKey(testName.ToUpper())
                        && testBeforeCleanDictionary[testName.ToUpper()])
                    {
                        luEventsWithUe.Remove(luEventWithLastUe);
                        #region for doubles Ue report
                        lastUePackageRemoved = true;
                        #endregion
                    }

                    int maxUePackagesCount = luEventsWithUe.Count;

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

                    string ueDictionaryTestName = testName.ToUpper();

                    if (this.MaxUeDictionary.ContainsKey(ueDictionaryTestName))
                    {
                        this.MaxUeDictionary[ueDictionaryTestName] = maxUePackagesCount > this.MaxUeDictionary[ueDictionaryTestName]
                            ? maxUePackagesCount
                            : this.MaxUeDictionary[ueDictionaryTestName];
                    }
                    else
                    {
                        this.MaxUeDictionary.Add(ueDictionaryTestName, maxUePackagesCount);
                    }
                }

                packagesCountDictionary.Add(platformName, platformPackagesCount);
            }

            return packagesCountDictionary;
        }

        private int CalculatePackages(
            List<ProxyPackageInfoV1> testPackages, 
            IEnumerable<Dictionary<int, LuEvent>> luEventList, 
            List<Dictionary<int, LuEvent>> luEventsWithUe
        )
        {
            int luEventPackagesCount = luEventList.Count();
            luEventPackagesCount -= luEventsWithUe.Count;
            int notLuEventPackagesCount = this.CountNonLuEventPackages(testPackages);
            return luEventPackagesCount + notLuEventPackagesCount;
        }

        private List<ProxyPackageInfoV1> GetTestPackagesWithoutUeDoubles(
            List<ProxyPackageInfoV1> deserializedTestPackages
        )
        {
            List<ProxyPackageInfoV1> testPackages = deserializedTestPackages;

            UePackageComparer uePackageComparer = new UePackageComparer();

            if (this._previousTestUePackages.Count != 0)
            {
                List<ProxyPackageInfoV1> testPackagesToRemove = new List<ProxyPackageInfoV1>();                

                foreach (ProxyPackageInfoV1 testPackage in testPackages)
                {
                    if (this._previousTestUePackages.Contains(testPackage, uePackageComparer))
                    {
                        testPackagesToRemove.Add(testPackage);
                    }
                }

                foreach (var testpackageToRemove in testPackagesToRemove)
                {
                    testPackages.Remove(testpackageToRemove);
                }
            }            

            testPackages = testPackages.Distinct(uePackageComparer).ToList();
            this._previousTestUePackages = new List<ProxyPackageInfoV1>();

            return testPackages;
        }

        /// <summary>
        /// Gets Lu event containing ue event with highest timestamp value among other subevents.
        /// </summary>
        /// <param name="luEventList">List of LuEvents in which to search for an desired value.</param>
        /// <returns>Lu event with Ue event.</returns>
        private Dictionary<int, LuEvent> FindLuEventsWithLastUe(IEnumerable<Dictionary<int, LuEvent>> luEventList)
        {
            IEnumerable<Dictionary<EventType, AbstractSdkEvent[]>> levelSubeventsList =
                        luEventList.GetAllLevelSubevents();
            IEnumerable<AbstractSdkEvent> allSubvents = levelSubeventsList.GetAllSubevents();

            UeEvent lastUeEvent = this.GetLastUeEvent(allSubvents);

            Dictionary<EventType, AbstractSdkEvent[]> levelSubeventWithLastUe =
                levelSubeventsList.FindSubeventPackForEvent(lastUeEvent, false);
            Dictionary<int, LuEvent> luEventWithLastUe =
                luEventList.FindLuEventForEventsPack(levelSubeventWithLastUe, false);
            return luEventWithLastUe;
        }

        /// <summary>
        /// Fills list with ue containing packages to weed out of doubles.
        /// </summary>
        /// <param name="testPackages">List of packages in which to search for an ue event packages.</param>
        /// <param name="luEventsWithUe">Lu events containing ue events.</param>
        private void SaveUePackagesToNextTest(
            List<ProxyPackageInfoV1> testPackages            
        )
        {
            IEnumerable<Dictionary<int, LuEvent>> luEventList = testPackages.GetAllLuEvents();

            List<Dictionary<int, LuEvent>> luEventsWithUe =
                        this.FindPackagesForSubevents(
                            luEventList.AllSubeventsOfType<UeEvent>(),
                            luEventList
                        ).ToList();

            foreach (Dictionary<int, LuEvent> luEventWithUe in luEventsWithUe)
            {
                ProxyPackageInfoV1 packageWithUe = testPackages.FindPackageForLuEvent(luEventWithUe);

                if (packageWithUe != null)
                {
                    this._previousTestUePackages.Add(packageWithUe);
                }
            }
        }

        /// <summary>
        /// Finds packages list for list of subevents taken.
        /// </summary>
        /// <param name="subevents">List of subevents.</param>
        /// <param name="luEvents">List of packages where to find.</param>
        /// <returns>List of packages, contains subevents.</returns>
        private IEnumerable<Dictionary<int, LuEvent>> FindPackagesForSubevents(
            IEnumerable<AbstractSdkEvent> subevents,
            IEnumerable<Dictionary<int, LuEvent>> luEvents
        )
        {
            var levelSubevents = luEvents.GetAllLevelSubevents();

            var foundedSubevents = new List<Dictionary<EventType, AbstractSdkEvent[]>>();

            foreach (var subevent in subevents)
            {
                Dictionary<EventType, AbstractSdkEvent[]> foundedSubevent = 
                    levelSubevents.FindSubeventPackForEvent(subevent, false);

                foundedSubevents.Add(foundedSubevent);
            }

            var foundedLuEvents = new List<Dictionary<int, LuEvent>>();

            foreach (var foundedSubevent in foundedSubevents)
            {
                Dictionary<int, LuEvent> foundedLuEvent = 
                    luEvents.FindLuEventForEventsPack(foundedSubevent);

                foundedLuEvents.Add(foundedLuEvent);
            }

            return foundedLuEvents;
        }

        /// <summary>
        /// Returns count of packages, not contains LuEvent. I.e. Sdk version info etc.
        /// </summary>
        /// <param name="packagesList">List of packages.</param>
        /// <returns>Count of non-LuEvent packages.</returns>
        private int CountNonLuEventPackages(IEnumerable<ProxyPackageInfoV1> packagesList)
        {
            int LuEventCount = 0;

            foreach (var package in packagesList)
            {
                if (package.RequestJson is LuData)
                {
                    LuEventCount++;
                }                
            }

            return packagesList.Count() - LuEventCount;
        }

        /// <summary>
        /// Searches for last UeEvent in all events which contains timestamp.
        /// </summary>
        /// <param name="allSubEvents">Subevents to search.</param>
        /// <returns>UeEvent or null.</returns>
        private UeEvent GetLastUeEvent(IEnumerable<AbstractSdkEvent> allSubEvents)
        {
            IEnumerable<IHasTimestamp> timeStampEvents =
                        allSubEvents.OfType<IHasTimestamp>().OrderBy(item => item.Timestamp);
            IEnumerable<UeEvent> ueEvents = allSubEvents.OfType<UeEvent>().OrderBy(e => e.Timestamp);

            if (timeStampEvents.Count() != 0 && ueEvents.Count() != 0)
            {
                UeEvent lastUeEvent = ueEvents.Last();

                if (timeStampEvents.Last() is UeEvent lastTimestampEvent 
                    && lastUeEvent == lastTimestampEvent
                )
                {
                    return lastUeEvent;
                }
            }

            return null;
        }

        /// <summary>
        /// Deserialize all result packages and dictionary with packages data.
        /// </summary>
        private void DeserializeAllPackagesInResults()
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

            if (this._counterSettings.SdkVersion == SdkVersions.V1)
            {
                packageSerializationSettings.Converters.Add(new RequestJsonConverterV1());
                packageSerializationSettings.Converters.Add(new CommonConverterV1());
                packageSerializationSettings.Converters.Add(new EventsArrayConverter());
                packageSerializationSettings.Converters.Add(new LuDataConverter());
            }
            else
            {
                packageSerializationSettings.Converters.Add(new CommonConverterV2());
                packageSerializationSettings.Converters.Add(new EventConverter());
                packageSerializationSettings.Converters.Add(new RequestJsonConverterV2());
                packageSerializationSettings.Converters.Add(new ResponseJsonConverter());
            }

            foreach (string directory in Directory.GetDirectories(this._counterSettings.PathToResults))
            {
                string platformName = Path.GetFileName(directory);

                Dictionary<string, List<ProxyPackageInfoV1>> platformPackagesDictionary =
                    new Dictionary<string, List<ProxyPackageInfoV1>>();                

                foreach (string subDirectory in Directory.GetDirectories(directory))
                {
                    List<ProxyPackageInfoV1> packagesList = new List<ProxyPackageInfoV1>();

                    IEnumerable<string> jsonFiles = Directory.GetFiles(subDirectory, "*.json");

                    foreach (string jsonFile in jsonFiles)
                    {
                        string packageContent = File.ReadAllText(jsonFile, Encoding.UTF8);

                        if (this._counterSettings.SdkVersion == SdkVersions.V1)
                        {
                            ProxyPackageInfoV1 deserializedJsonV1 = JsonConvert.DeserializeObject<ProxyPackageInfoV1>(
                                packageContent,
                                packageSerializationSettings
                            );

                            packagesList.Add(deserializedJsonV1);
                        }
                        else
                        {
                            ProxyPackageInfoV2 deserializedJsonV2 = JsonConvert.DeserializeObject<ProxyPackageInfoV2>(
                                packageContent,
                                packageSerializationSettings
                            );
                        }
                    }

                    string testName = Path.GetFileName(subDirectory);

                    platformPackagesDictionary.Add(testName, packagesList);
                }

                this._allPackagesDeserialized.Add(platformName, platformPackagesDictionary);
            }
        }

        /// <summary>
        /// Generate csv report.
        /// </summary>
        private void WriteToCsv()
        {
            StringBuilder csvContent = new StringBuilder();

            GenerateTitleLine(csvContent);
            GenerateRestContent(csvContent, this.PackagesDictionary);

            string filePath = Path.Combine(this._counterSettings.PathToResults, "packagesCountNew.csv");

            File.WriteAllText(filePath, csvContent.ToString());
        }

        /// <summary>
        /// Fill csv with values.
        /// </summary>
        /// <param name="csvContent">Csv contetn builder to fill.</param>
        /// <param name="convertedDictionary">Set of platforms and tests with packages counting.</param>
        private static void GenerateRestContent(
            StringBuilder csvContent, 
            Dictionary<string, Dictionary<string, int>> convertedDictionary
        )
        {
            foreach (var testName in convertedDictionary)
            {
                string nameofTest = testName.Key;
                string packagesCount = "";

                foreach (var platformName in testName.Value.OrderBy(test => test.Key))
                {
                    packagesCount += $";{platformName.Value}";
                }

                string newLine = string.Format("{0}{1}", nameofTest, packagesCount);

                csvContent.AppendLine(newLine);
            }            
        }

        /// <summary>
        /// Adds title line to csv.
        /// </summary>        
        /// <param name="csvContent">Csv contetn builder to fill.</param>
        private void GenerateTitleLine(StringBuilder csvContent)
        {
            string titleLine;

            if (this._counterSettings.IgnoreUePackages)
            {
                titleLine = this._counterSettings.CalculatePackagesWithMaxUe
                    ? "With max Ue count" 
                    : "Ue packages ignored";
            }
            else
            {
                titleLine = "All packages counted";
            }

            foreach (string directory in this._platformList)
            {
                titleLine += $",{Path.GetFileName(directory)}";
            }

            csvContent.AppendLine(titleLine);
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
    }
}
