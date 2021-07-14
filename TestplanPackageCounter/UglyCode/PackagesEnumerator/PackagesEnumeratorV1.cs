using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TestplanPackageCounter.Counter;
using TestplanPackageCounter.General;
using TestplanPackageCounter.General.EventExtensions;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V1.Events;
using TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents;
using TestplanPackageCounter.Packages.Converters.V1;
using TestplanPackageCounter.Testplan.Content;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal class PackagesEnumeratorV1 : CommonEnumerator
    {
        private Dictionary<string, Dictionary<string, List<ProxyPackageInfoV1>>> _deserializedPackages =
                new Dictionary<string, Dictionary<string, List<ProxyPackageInfoV1>>>();

        private List<ProxyPackageInfoV1> _previousTestPackages = new List<ProxyPackageInfoV1>();

        internal PackagesEnumeratorV1(CounterSettings counterSettings, List<TestSuite> testSuites)
        {
            this.counterSettings = counterSettings;
            this.MaxUeDictionary = new Dictionary<string, int>();
            this.platformList = this.GetPlatformList(counterSettings.PathToResults);
            this.testBeforeCleanDictionary = this.GetToKnowCleaningTest();
            this.PackagesStatusDictionary = new Dictionary<string, Dictionary<string, TestPackagesData>>();
            this.testSuites = testSuites;
        }

        internal override void Enumerate()
        {
            this.TestsList = this.GetTestList();
            this._DeserializeAllPackagesV1();
            this.PackagesStatusDictionary = this._EnumeratePackagesV1();
            this.PackagesStatusDictionary = this.ConvertPackageDictionary(PackagesStatusDictionary);            
        }

        /// <summary>
        /// Deserialize all result packages and dictionary with packages data.
        /// </summary>
        private void _DeserializeAllPackagesV1()
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

            packageSerializationSettings.Converters.Add(new RequestJsonConverterV1());
            packageSerializationSettings.Converters.Add(new CommonConverterV1());
            packageSerializationSettings.Converters.Add(new EventsArrayConverter());
            packageSerializationSettings.Converters.Add(new LuDataConverter());

            foreach (string directory in Directory.GetDirectories(this.counterSettings.PathToResults))
            {
                string platformName = Path.GetFileName(directory);

                Console.Write($"Deserialize {platformName} packages..");

                Dictionary<string, List<ProxyPackageInfoV1>> platformPackagesDictionary =
                    new Dictionary<string, List<ProxyPackageInfoV1>>();

                foreach (string subDirectory in Directory.GetDirectories(directory))
                {
                    List<ProxyPackageInfoV1> packagesList = new List<ProxyPackageInfoV1>();

                    IEnumerable<string> jsonFiles = Directory.GetFiles(subDirectory, "*.json");

                    foreach (string jsonFile in jsonFiles)
                    {
                        string packageContent = File.ReadAllText(jsonFile, Encoding.UTF8);

                        ProxyPackageInfoV1 deserializedJsonV1 = JsonConvert.DeserializeObject<ProxyPackageInfoV1>(
                                packageContent,
                                packageSerializationSettings
                            );

                        packagesList.Add(deserializedJsonV1);
                    }

                    string testName = Path.GetFileName(subDirectory);

                    platformPackagesDictionary.Add(testName, packagesList);

                    Console.Write(".");
                }

                Console.WriteLine();

                this._deserializedPackages.Add(platformName, platformPackagesDictionary);
            }
        }

        /// <summary>
        /// Calculates packages in given result folder.
        /// </summary>
        /// <returns>Platform separated dictionary of dictionary of test and packages.</returns>
        private Dictionary<string, Dictionary<string, TestPackagesData>> _EnumeratePackagesV1()
        {
            Console.WriteLine("Packages enumeration started..");

            Dictionary<string, Dictionary<string, TestPackagesData>> packagesDataDictionary =
                new Dictionary<string, Dictionary<string, TestPackagesData>>();

            foreach (var deserializedPlatformPackages in this._deserializedPackages)
            {
                string platformName = deserializedPlatformPackages.Key;

                Console.Write($"Enumerate {platformName} packages..");

                Dictionary<string, List<ProxyPackageInfoV1>> platformPackages =
                    deserializedPlatformPackages.Value;
                Dictionary<string, TestPackagesData> platformPackagesData =
                    new Dictionary<string, TestPackagesData>();

                foreach (var deserializedTestPackages in platformPackages)
                {
                    string testName = deserializedTestPackages.Key;

                    TestPackagesData testPackagesData = this._CalculatePackages(
                        new List<ProxyPackageInfoV1>(deserializedTestPackages.Value),
                        testName
                    );

                    platformPackagesData.Add(testName, testPackagesData);
                }

                packagesDataDictionary.Add(platformName, platformPackagesData);

                Console.Write(".");
            }

            Console.WriteLine();

            return packagesDataDictionary;
        }

        private TestPackagesData _CalculatePackages(
            List<ProxyPackageInfoV1> testPackagesOriginal,
            string testName
        )
        {
            List<ProxyPackageInfoV1> testPackages =
                this._GetTestPackagesWithoutDoubles(new List<ProxyPackageInfoV1>(testPackagesOriginal));

            this._previousTestPackages = new List<ProxyPackageInfoV1>(testPackagesOriginal);

            List<string> doublesSignatures = this.PackagesDoubleCheck(testPackages, testPackagesOriginal);
            List<ProxyPackageInfoV1> alContainingPackages = this._FindAllPackagesOfEvent<AlEvent>(testPackages).ToList();
            List<ProxyPackageInfoV1> ueContainingPackages = this._FindAllPackagesOfEvent<UeEvent>(testPackages).ToList();

            ProxyPackageInfoV1 packageWithLastAlEvent = this._GetPackageWithLastEventOfType<AlEvent>(testPackages);
            ProxyPackageInfoV1 packageWithLastUeEvent = this._GetPackageWithLastEventOfType<UeEvent>(testPackages);

            IEnumerable<ProxyPackageInfoV1> sdkVersionPackages = testPackages.Where(e => e.RequestJson is SdkVersionData);

            bool previousTestContainsClean = this.testBeforeCleanDictionary != null
                && this.testBeforeCleanDictionary.ContainsKey(testName.ToUpper())
                && this.testBeforeCleanDictionary[testName.ToUpper()];

            TestPackagesData testPackagesData = new TestPackagesData(
                originalPackagesCount: testPackagesOriginal.Count,
                packagesCount: testPackages.Count,
                alPackagesCount: alContainingPackages.Count,
                uePackagesCount: ueContainingPackages.Count,
                caPackagesCount: 0,
                attemptPackagesCount: 0,
                sdkVersionCount: sdkVersionPackages.Count(),
                isLastAlRemoved: (previousTestContainsClean && this.counterSettings.IgnoreLastAl) && packageWithLastAlEvent != null,
                isLastUeRemoved: (previousTestContainsClean && this.counterSettings.IgnoreBadUe) && packageWithLastUeEvent != null,
                isAllEventsOrdered: true,
                events: null,
                doublesSignatures: doublesSignatures,
                badCodesPackages: new List<ProxyPackageInfo>(),
                previousTestContainsCleaning: previousTestContainsClean,
                containsDeserializationErrors: false
            );

            return testPackagesData;
        }

        private ProxyPackageInfoV1 _GetPackageWithLastEventOfType<T>(IEnumerable<ProxyPackageInfoV1> testPackages)
        {
            IEnumerable<T> desiredTypeEvents = testPackages.GetAllLuEvents().AllSubeventsOfType<T>();
            IEnumerable<IHasTimestamp> timestampEvents = 
                testPackages.GetAllLuEvents().AllSubeventsOfType<IHasTimestamp>().OrderBy(e => e.Timestamp);

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
                    lastEventByTimestamp is AbstractSdkEventV1 abstractSdkEvent
                )
                {
                    ProxyPackageInfoV1 packageWithDesiredEvent = abstractSdkEvent.FindPackageForSubEvent(testPackages);
                    return packageWithDesiredEvent.GetAllLuEvents().GetAllLevelSubevents().Count() == 1 ? packageWithDesiredEvent : null;
                }

            }

            return null;
        }

        private IEnumerable<ProxyPackageInfoV1> _FindAllPackagesOfEvent<T>(IEnumerable<ProxyPackageInfoV1> testPackages)
        {
            List<ProxyPackageInfoV1> packagesWithDesiredEventType = new List<ProxyPackageInfoV1>();

            IEnumerable<T> desiredTypeEvents = testPackages.GetAllLuEvents().AllSubeventsOfType<T>();

            foreach (T desiredTypeEvent in desiredTypeEvents)
            {
                if (desiredTypeEvent is AbstractSdkEventV1 abstractSdkEvent)
                {
                    packagesWithDesiredEventType.Add(abstractSdkEvent.FindPackageForSubEvent(testPackages));
                }
            }

            return packagesWithDesiredEventType.Distinct();
        }

        private List<ProxyPackageInfoV1> _GetTestPackagesWithoutDoubles(
            List<ProxyPackageInfoV1> deserializedTestPackages
        )
        {
            List<ProxyPackageInfoV1> testPackages = deserializedTestPackages;

            PackageComparerV1 uePackageComparer = new PackageComparerV1();

            if (this._previousTestPackages.Count != 0)
            {
                List<ProxyPackageInfoV1> testPackagesToRemove = new List<ProxyPackageInfoV1>();

                foreach (ProxyPackageInfoV1 testPackage in testPackages)
                {
                    if (this._previousTestPackages.Contains(testPackage, uePackageComparer))
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
            this._previousTestPackages = new List<ProxyPackageInfoV1>();

            return testPackages;
        }
    }
}
