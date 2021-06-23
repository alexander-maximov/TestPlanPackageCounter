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
using TestplanPackageCounter.Packages.Content.V2;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events;
using TestplanPackageCounter.Packages.Content.V2.UserIdentification;
using TestplanPackageCounter.Packages.Converters.V2;
using TestplanPackageCounter.Testplan.Content;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal class PackagesEnumeratorV2 : CommonEnumerator
    {
        private Dictionary<string, Dictionary<string, List<ProxyPackageInfoV2>>> _deserializedPackages =
                new Dictionary<string, Dictionary<string, List<ProxyPackageInfoV2>>>();

        private List<ProxyPackageInfoV2> _previousTestPackages = new List<ProxyPackageInfoV2>();

        internal PackagesEnumeratorV2(CounterSettings counterSettings, Dictionary<string, bool> testBeforeCleanDictionary, List<TestSuite> testSuites)
        {
            this._counterSettings = counterSettings;
            this.MaxUeDictionary = new Dictionary<string, int>();
            this._platformList = this.GetPlatformList(counterSettings.PathToResults);
            this._testBeforeCleanDictionary = testBeforeCleanDictionary;
            this.PackagesStatusDictionary = new Dictionary<string, Dictionary<string, TestPackagesData>>();
            this.testSuites = testSuites;
        }

        internal override void Enumerate()
        {            
            this.TestsList = this.GetTestList();
            this.DeserializeAllPackagesV2();
            this.PackagesStatusDictionary = this.EnumeratePackagesV2();
            this.PackagesStatusDictionary = this.ConvertPackageDictionary(PackagesStatusDictionary);            
        }

        /// <summary>
        /// Deserialize all result packages and dictionary with packages data.
        /// </summary>
        private void DeserializeAllPackagesV2()
        {
            Console.WriteLine("Packages deserialization started..");

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

            foreach (string directory in Directory.GetDirectories(this._counterSettings.PathToResults))
            {
                string platformName = Path.GetFileName(directory);

                Console.WriteLine($"Deserialize {platformName} packages..");

                Dictionary<string, List<ProxyPackageInfoV2>> platformPackagesDictionary =
                    new Dictionary<string, List<ProxyPackageInfoV2>>();

                string[] subDirectories = Directory.GetDirectories(directory);

                int subDirectoriesCount = subDirectories.Length + 1;
                int counter = 1;

                foreach (string subDirectory in subDirectories)
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

                    if (this.TestsList.ContainsKey(testName.ToUpper()))
                    {
                        this.TestsList[testName.ToUpper()].Add(platformName);
                    }

                    platformPackagesDictionary.Add(testName, packagesList);

                    Console.Write($"\r({++counter}/{subDirectoriesCount}) deserialized.");                    
                }

                if (counter == subDirectoriesCount)
                {
                    Console.Write($"\rAll {subDirectoriesCount} packages deserialized.");
                }

                Console.WriteLine("\n");

                this._deserializedPackages.Add(platformName, platformPackagesDictionary);
            }
        }

        /// <summary>
        /// Calculates packages in given result folder.
        /// </summary>
        /// <returns>Platform separated dictionary of dictionary of test and packages.</returns>
        private Dictionary<string, Dictionary<string, TestPackagesData>> EnumeratePackagesV2()
        {
            Console.WriteLine("Packages enumeration started..");

            Dictionary<string, Dictionary<string, TestPackagesData>> packagesDataDictionary = 
                new Dictionary<string, Dictionary<string, TestPackagesData>>();

            foreach (var deserializedPlatformPackages in this._deserializedPackages)
            {
                string platformName = deserializedPlatformPackages.Key;

                Console.WriteLine($"Enumerate {platformName} packages..");

                Dictionary<string, List<ProxyPackageInfoV2>> platformPackages =
                    deserializedPlatformPackages.Value;
                Dictionary<string, TestPackagesData> platformPackagesData = new Dictionary<string, TestPackagesData>();

                int platformPackagesCount = platformPackages.Count() + 1;
                int counter = 1;
                int skipCounter = 0;

                foreach (var testName in this.TestsList.Keys)
                {
                    if (!platformPackages.ContainsKey(testName.ToUpper())
                        && !this.TestsList[testName.ToUpper()].Contains(platformName))
                    {
                        platformPackagesData.Add(testName, new TestPackagesData());
                        skipCounter++;
                        continue;
                    }

                    List<ProxyPackageInfoV2> packagesList = 
                        platformPackages.FirstOrDefault(e => e.Key.Equals(testName, StringComparison.OrdinalIgnoreCase)).Value;

                    TestPackagesData testPackagesData = this.CalculatePackages(packagesList, testName);

                    platformPackagesData.Add(testName, testPackagesData);

                    Console.Write($"\r({++counter}/{platformPackagesCount}) enumerated. ({skipCounter}/{this.TestsList.Count}) tests wasn't in results folder.");                    
                }

                if (counter == platformPackagesCount)
                {
                    Console.Write($"\rAll {platformPackagesCount} packages enumerated. ({skipCounter}/{this.TestsList.Count}) tests wasn't in results folder.");
                }

                packagesDataDictionary.Add(platformName, platformPackagesData);

                Console.WriteLine("\n");
            }

            return packagesDataDictionary;
        }

        private IEnumerable<ProxyPackageInfoV2> FindAllPackagesOfEvent<T>(IEnumerable<ProxyPackageInfoV2> testPackages)
        {
            List<ProxyPackageInfoV2> packagesWithDesiredEventType = new List<ProxyPackageInfoV2>();

            IEnumerable<T> desiredTypeEvents = testPackages.AllEventsOfType<T>();

            foreach (T desiredTypeEvent in desiredTypeEvents)
            {
                if (desiredTypeEvent is AbstractSdkEventV2 abstractSdkEvent)
                {
                    packagesWithDesiredEventType.Add(abstractSdkEvent.FindPackage(testPackages));
                }
            }

            return packagesWithDesiredEventType.Distinct();
        }

        private TestPackagesData CalculatePackages(
            List<ProxyPackageInfoV2> testPackagesOriginal,
            string testName
        )
        {
            List<ProxyPackageInfoV2> testPackages =
                this.GetTestPackagesWithoutDoubles(new List<ProxyPackageInfoV2>(testPackagesOriginal));

            this._previousTestPackages = new List<ProxyPackageInfoV2>(testPackagesOriginal);

            List<string> doublesSignatures = this.PackagesDoubleCheck(testPackages, testPackagesOriginal);
            List<ProxyPackageInfoV2> alContainingPackages = this.FindAllPackagesOfEvent<AlV2>(testPackages).ToList();
            List<ProxyPackageInfoV2> ueContainingPackages = this.FindAllPackagesOfEvent<UeV2>(testPackages).ToList();

            ProxyPackageInfoV2 packageWithLastAlEvent = this.GetPackageWithLastEventOfType<AlV2>(testPackages);
            ProxyPackageInfoV2 packageWithLastUeEvent = this.GetPackageWithLastEventOfType<UeV2>(testPackages);

            if (packageWithLastUeEvent != null)
            {
                List<ProxyPackageInfoV2> testPackagesWithoutLastUe = new List<ProxyPackageInfoV2>(testPackages);
                testPackagesWithoutLastUe.Remove(packageWithLastUeEvent);
                packageWithLastAlEvent = this.GetPackageWithLastEventOfType<AlV2>(testPackagesWithoutLastUe);
            }

            List<ProxyPackageInfo> badCodePackages = new List<ProxyPackageInfo>();
            bool containsZeroResponseCode = false;

            if (this._counterSettings.IgnoreBadCodePackages)
            {
                foreach (ProxyPackageInfoV2 testPackage in testPackages.Where(e => e.ResponseCode == 0 || e.ResponseCode == 404).ToList())
                {
                    containsZeroResponseCode = testPackage.ResponseCode == 0;

                    badCodePackages.Add(testPackage);
                }
            }

            List<string> eventCodes = testPackages.AllEvents().Select(e => e.Code).ToList();

            bool previousTestContainsClean = this._testBeforeCleanDictionary != null
                && this._testBeforeCleanDictionary.ContainsKey(testName.ToUpper())
                && this._testBeforeCleanDictionary[testName.ToUpper()];

            IEnumerable<ProxyPackageInfoV2> attemptPackages = testPackages.Where(e => e.RequestJson is UserIdentificationRequest);

            TestPackagesData testPackagesData = new TestPackagesData(
                originalPackagesCount: testPackagesOriginal.Count,
                packagesCount: testPackages.Count,
                alPackagesCount: alContainingPackages.Count,
                uePackagesCount: ueContainingPackages.Count,
                attemptPackagesCount: attemptPackages.Count(),
                isLastAlRemoved: (previousTestContainsClean && this._counterSettings.IgnoreLastAl) && packageWithLastAlEvent != null,
                isLastUeRemoved: (previousTestContainsClean && this._counterSettings.IgnoreLastUe) && packageWithLastUeEvent != null,
                isAllEventsOrdered: CheckEventsTimestampOrder(testPackages),
                events: eventCodes,
                doublesSignatures: doublesSignatures,
                badCodesPackages: badCodePackages,
                containsZeroCodePackage: containsZeroResponseCode
            );

            return testPackagesData;
        }

        private static bool CheckEventsTimestampOrder(List<ProxyPackageInfoV2> testPackages)
        {
            foreach (ProxyPackageInfoV2 proxyPackage in testPackages)
            {
                IEnumerable<IHasTimestamp> packageEvents = proxyPackage.AllEvents().OfType<IHasTimestamp>();

                if (!packageEvents.SequenceEqual(packageEvents.OrderBy(e => e.Timestamp)))
                {
                    return false;
                }
            }

            return true;
        }

        private ProxyPackageInfoV2 GetPackageWithLastEventOfType<T>(IEnumerable<ProxyPackageInfoV2> testPackages)
        {
            IEnumerable<T> desiredTypeEvents = testPackages.AllEventsOfType<T>();
            IEnumerable<IHasTimestamp> timestampEvents = testPackages.AllEventsOfType<IHasTimestamp>().OrderBy(e => e.Timestamp);

            if (desiredTypeEvents.Count() != 0 && timestampEvents.Count() != 0)
            {
                IHasTimestamp lastEventByTimestamp = timestampEvents.Last();

                foreach (T desiredTypeEvent in desiredTypeEvents)
                {
                    if (desiredTypeEvent is IHasTimestamp timestampEvent
                        && timestampEvent == lastEventByTimestamp
                    )
                    {
                        lastEventByTimestamp = timestampEvent;
                    }
                }

                if (lastEventByTimestamp is T &&
                    lastEventByTimestamp is AbstractSdkEventV2 abstractSdkEvent
                )
                {
                    ProxyPackageInfoV2 packageWithDesiredEvent = abstractSdkEvent.FindPackage(testPackages);
                    return packageWithDesiredEvent.AllEvents().Count() == 1 ? packageWithDesiredEvent : null;
                }

            }

            return null;
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
