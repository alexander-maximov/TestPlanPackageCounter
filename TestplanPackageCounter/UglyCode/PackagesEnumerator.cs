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
using TestplanPackageCounter.Packages.Converters.V1;

namespace TestplanPackageCounter.UglyCode
{
    internal class PackagesEnumerator
    {
        private readonly CounterSettings _counterSettings;

        private Dictionary<string, Dictionary<string, List<ProxyPackageInfoV1>>> _allPackagesDeserialized =
            new Dictionary<string, Dictionary<string, List<ProxyPackageInfoV1>>>();

        internal PackagesEnumerator(CounterSettings counterSettings)
        {
            this._counterSettings = counterSettings;
        }

        internal Dictionary<string, int> MaxUeDictionary { get; set; }

        internal Dictionary<string, Dictionary<string, int>> PackagesDictionary { get; set; }

        internal void Enumerate()
        {
            this.MaxUeDictionary = new Dictionary<string, int>();

            List<string> platformList = this.GetPlatformList(this._counterSettings.PathToResults);

            //Gets packages content
            this.DeserializeAllPackagesInresults();

            this.EnumeratePackages();

            #region obsolete
            //Read in dict
            Dictionary<string, Dictionary<string, int>> packagesDictionary = 
                this.FillPackageDictionary();            

            //Convert dict to another
            this.PackagesDictionary =
                ConvertPackageDictionary(packagesDictionary, platformList);
            #endregion

            if (this._counterSettings.WriteToCsv)
            {
                WriteToCsv();
            }
        }

        private void EnumeratePackages()
        {
            Dictionary<string, Dictionary<string, Dictionary<EventType, int>>> packagesCountDictionary =
                new Dictionary<string, Dictionary<string, Dictionary<EventType, int>>>();

            foreach (var deserializedPlatformPackages in this._allPackagesDeserialized)
            {
                Dictionary<string, Dictionary<EventType, int>> platformPackagesCount =
                    new Dictionary<string, Dictionary<EventType, int>>();

                string platformName = deserializedPlatformPackages.Key;

                Dictionary<string, List<ProxyPackageInfoV1>> platformPackages =
                    deserializedPlatformPackages.Value;

                foreach (var deserializedTestPackages in platformPackages)
                {
                    string testName = deserializedTestPackages.Key;

                    List<ProxyPackageInfoV1> testPackages = deserializedTestPackages.Value;

                    IEnumerable<Dictionary<int, LuEvent>> luEventList = testPackages.GetAllLuEvents();

                    IEnumerable<Dictionary<EventType, AbstractSdkEvent[]>> subEventsPackedList =
                        luEventList.GetAllSubeventsByPackages();

                    IEnumerable<AbstractSdkEvent> allSubEvents = subEventsPackedList.GetAllSubevents();

                    IEnumerable<IHasTimestamp> timeStampEvents = allSubEvents.OfType<IHasTimestamp>().OrderBy(item => item.Timestamp);
                    IEnumerable<UeEvent> ueEvents = allSubEvents.OfType<UeEvent>();

                    if (timeStampEvents != null && ueEvents != null)
                    {
                        UeEvent lastUeEvent = ueEvents.Last();

                        if (timeStampEvents.Last() is UeEvent lastTimestampEvent)
                        {
                            if (lastUeEvent == lastTimestampEvent)
                            {
                                //IGNORE!
                            }
                        }
                    }
                }
            }
        }

        private void DeserializeAllPackagesInresults()
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

            packageSerializationSettings.Converters.Add(new RequestJsonConverter());
            packageSerializationSettings.Converters.Add(new CommonConverter());
            packageSerializationSettings.Converters.Add(new EventsArrayConverter());
            packageSerializationSettings.Converters.Add(new LuDataConverter());

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

                        ProxyPackageInfoV1 desetializedJson = JsonConvert.DeserializeObject<ProxyPackageInfoV1>(
                            packageContent, 
                            packageSerializationSettings
                        );

                        packagesList.Add(desetializedJson);
                    }

                    string testName = Path.GetFileName(subDirectory);

                    platformPackagesDictionary.Add(testName, packagesList);
                }

                this._allPackagesDeserialized.Add(platformName, platformPackagesDictionary);
            }
        }

        private void WriteToCsv()
        {
            StringBuilder csvContent = new StringBuilder();

            GenerateTitleLine(this._counterSettings.PathToResults, csvContent);
            GenerateRestContent(csvContent, this.PackagesDictionary);

            string filePath = Path.Combine(this._counterSettings.PathToResults, "packagesCountNew.csv");

            File.WriteAllText(filePath, csvContent.ToString());
        }

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

        private void GenerateTitleLine(string resultsPath, StringBuilder csvContent)
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

            foreach (string directory in Directory.GetDirectories(resultsPath))
            {
                titleLine += $",{Path.GetFileName(directory)}";
            }

            csvContent.AppendLine(titleLine);
        }

        private static Dictionary<string, Dictionary<string, int>> ConvertPackageDictionary(
            Dictionary<string, Dictionary<string, int>> packagesDictionary, 
            List<string> platformList
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
                foreach (var platformName in platformList)
                {
                    if (!testName.Value.Keys.Contains(platformName))
                    {
                        testName.Value.Add(platformName, -1);
                    }
                }
            }

            return convertedDictionary;
        }

        private Dictionary<string, Dictionary<string, int>> FillPackageDictionary()
        {
            Dictionary<string, Dictionary<string, int>> packagesDictionary =
                new Dictionary<string, Dictionary<string, int>>();

            foreach (string directory in Directory.GetDirectories(this._counterSettings.PathToResults))
            {
                string platformName = Path.GetFileName(directory);                

                Dictionary<string, int> testPackagesDictionary = new Dictionary<string, int>();

                foreach (string subDirectory in Directory.GetDirectories(directory))
                {
                    string[] jsonFiles = Directory.GetFiles(subDirectory, "*.json");

                    string testName = Path.GetFileName(subDirectory);

                    int maxUeCount = 0;
                    int packagesCount = jsonFiles.Length;

                    if (this._counterSettings.IgnoreUePackages)
                    {
                        int ueCount = 0;

                        foreach (var jsonFile in jsonFiles)
                        {
                            IEnumerable<string> ueLines = File.ReadLines(jsonFile).Skip(4).Take(9);

                            if (ueLines.First().Contains("ue") && ueLines.Last().Contains("}"))
                            {
                                packagesCount--;
                                ueCount++;
                            }

                            continue;
                        }

                        maxUeCount = ueCount > maxUeCount ? ueCount : maxUeCount;

                        if (this._counterSettings.CalculatePackagesWithMaxUe)
                        {
                            packagesCount += maxUeCount;
                        }
                    }

                    testPackagesDictionary.Add(testName, packagesCount);

                    if (this.MaxUeDictionary.ContainsKey(testName.ToUpper()))
                    {
                        this.MaxUeDictionary[testName.ToUpper()] = maxUeCount;
                    }
                    else
                    {
                        this.MaxUeDictionary.Add(testName.ToUpper(), maxUeCount);
                    }
                }

                packagesDictionary.Add(platformName, testPackagesDictionary);                
            }

            return packagesDictionary;
        }

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
