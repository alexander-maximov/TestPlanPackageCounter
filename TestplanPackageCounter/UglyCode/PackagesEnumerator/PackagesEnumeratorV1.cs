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
using TestplanPackageCounter.Packages.Content.V1;
using TestplanPackageCounter.Packages.Content.V1.Events;
using TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents;
using TestplanPackageCounter.Packages.Converters.V1;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal class PackagesEnumeratorV1 : CommonEnumerator
    {
        Dictionary<string, Dictionary<string, List<ProxyPackageInfoV1>>> _deserializedPackages =
                new Dictionary<string, Dictionary<string, List<ProxyPackageInfoV1>>>();

        private List<ProxyPackageInfoV1> _previousTestPackages = new List<ProxyPackageInfoV1>();

        internal PackagesEnumeratorV1(CounterSettings counterSettings, Dictionary<string, bool> testBeforeCleanDictionary)
        {
            this._counterSettings = counterSettings;
            this.MaxUeDictionary = new Dictionary<string, int>();
            this._platformList = this.GetPlatformList(counterSettings.PathToResults);
            this._testBeforeCleanDictionary = testBeforeCleanDictionary;
            this.PackagesStatusDictionary = new Dictionary<string, Dictionary<string, TestPackagesData>>();
        }

        internal override void Enumerate()
        {
            this.DeserializeAllPackagesV1();
            this.PackagesStatusDictionary = this.EnumeratePackagesV1();
            this.PackagesStatusDictionary = this.ConvertPackageDictionary(PackagesStatusDictionary);
        }

        /// <summary>
        /// Deserialize all result packages and dictionary with packages data.
        /// </summary>
        private void DeserializeAllPackagesV1()
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

            packageSerializationSettings.Converters.Add(new RequestJsonConverterV1());
            packageSerializationSettings.Converters.Add(new CommonConverterV1());
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

                        ProxyPackageInfoV1 deserializedJsonV1 = JsonConvert.DeserializeObject<ProxyPackageInfoV1>(
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
        /// Calculates packages in given result folder.
        /// </summary>
        /// <returns>Platform separated dictionary of dictionary of test and packages.</returns>
        private Dictionary<string, Dictionary<string, TestPackagesData>> EnumeratePackagesV1()
        {
            Dictionary<string, Dictionary<string, TestPackagesData>> packagesDataDictionary =
                new Dictionary<string, Dictionary<string, TestPackagesData>>();

            foreach (var deserializedPlatformPackages in this._deserializedPackages)
            {
                string platformName = deserializedPlatformPackages.Key;

                Dictionary<string, List<ProxyPackageInfoV1>> platformPackages =
                    deserializedPlatformPackages.Value;
                Dictionary<string, TestPackagesData> platformPackagesData =
                    new Dictionary<string, TestPackagesData>();

                foreach (var deserializedTestPackages in platformPackages)
                {
                    string testName = deserializedTestPackages.Key;

                    TestPackagesData testPackagesData = this.CalculatePackages(
                        new List<ProxyPackageInfoV1>(deserializedTestPackages.Value),
                        testName
                    );

                    platformPackagesData.Add(testName, testPackagesData);
                }

                packagesDataDictionary.Add(platformName, platformPackagesData);
            }

            return packagesDataDictionary;
        }

        private TestPackagesData CalculatePackages(
            List<ProxyPackageInfoV1> testPackagesOriginal,
            string testName
        )
        {
            List<ProxyPackageInfoV1> testPackages =
                this.GetTestPackagesWithoutDoubles(new List<ProxyPackageInfoV1>(testPackagesOriginal));

            this._previousTestPackages = new List<ProxyPackageInfoV1>(testPackagesOriginal);

            List<string> doublesSignatures = this.PackagesDoubleCheck(testPackages, testPackagesOriginal);
            List<ProxyPackageInfoV1> alContainingPackages = this.FindAllPackagesOfEvent<AlEvent>(testPackages).ToList();
            List<ProxyPackageInfoV1> ueContainingPackages = this.FindAllPackagesOfEvent<UeEvent>(testPackages).ToList();

            ProxyPackageInfoV1 packageWithLastAlEvent = this.GetPackageWithLastEventOfType<AlEvent>(testPackages);
            ProxyPackageInfoV1 packageWithLastUeEvent = this.GetPackageWithLastEventOfType<UeEvent>(testPackages);

            bool previousTestContainsClean = this._testBeforeCleanDictionary != null
                && this._testBeforeCleanDictionary.ContainsKey(testName.ToUpper())
                && this._testBeforeCleanDictionary[testName.ToUpper()];

            TestPackagesData testPackagesData = new TestPackagesData(
                originalPackagesCount: testPackagesOriginal.Count,
                packagesCount: testPackages.Count,
                alPackagesCount: alContainingPackages.Count,
                uePackagesCount: ueContainingPackages.Count,
                lastAlRemoved: (previousTestContainsClean && this._counterSettings.IgnoreLastAl) && packageWithLastAlEvent != null,
                lastUeRemoved: (previousTestContainsClean && this._counterSettings.IgnoreLastUe) && packageWithLastUeEvent != null,
                doublesSignatures: doublesSignatures
            );

            return testPackagesData;
        }

        private ProxyPackageInfoV1 GetPackageWithLastEventOfType<T>(IEnumerable<ProxyPackageInfoV1> testPackages)
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
                    lastEventByTimestamp is AbstractSdkEvent abstractSdkEvent
                )
                {
                    ProxyPackageInfoV1 packageWithDesiredEvent = abstractSdkEvent.FindPackageForSubEvent(testPackages);
                    return packageWithDesiredEvent.GetAllLuEvents().GetAllLevelSubevents().Count() == 1 ? packageWithDesiredEvent : null;
                }

            }

            return null;
        }

        private IEnumerable<ProxyPackageInfoV1> FindAllPackagesOfEvent<T>(IEnumerable<ProxyPackageInfoV1> testPackages)
        {
            List<ProxyPackageInfoV1> packagesWithDesiredEventType = new List<ProxyPackageInfoV1>();

            IEnumerable<T> desiredTypeEvents = testPackages.GetAllLuEvents().AllSubeventsOfType<T>();

            foreach (T desiredTypeEvent in desiredTypeEvents)
            {
                if (desiredTypeEvent is AbstractSdkEvent abstractSdkEvent)
                {
                    packagesWithDesiredEventType.Add(abstractSdkEvent.FindPackageForSubEvent(testPackages));
                }
            }

            return packagesWithDesiredEventType.Distinct();
        }

        private List<ProxyPackageInfoV1> GetTestPackagesWithoutDoubles(
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
