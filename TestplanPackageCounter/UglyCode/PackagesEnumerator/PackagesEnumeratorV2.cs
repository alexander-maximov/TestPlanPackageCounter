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
using TestplanPackageCounter.Packages.Content.V2;
using TestplanPackageCounter.Packages.Content.V2.Analytics;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events;
using TestplanPackageCounter.Packages.Content.V2.SdkVersion;
using TestplanPackageCounter.Packages.Content.V2.UserIdentification;
using TestplanPackageCounter.Packages.Converters.V2;
using TestplanPackageCounter.Testplan.Content;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal class PackagesEnumeratorV2 : CommonEnumerator
    {
        private Dictionary<string,LinkedList<ProxyPackageInfoV2>> _linkedPackages = new();

        private Dictionary<string, Dictionary<string, List<ProxyPackageInfoV2>>> _deserializedPackages = new();

        private List<ProxyPackageInfoV2> _previousTestPackages = new();

        internal PackagesEnumeratorV2(List<TestSuite> testSuites)
        {
            this.MaxUeDictionary = new Dictionary<string, int>();
            this.platformList = this.GetPlatformList(CounterSettings.PathToResults);            
            this.PackagesStatusDictionary = new Dictionary<string, Dictionary<string, TestPackagesData>>();
            this.testSuites = testSuites;
        }

        internal override void Enumerate()
        {            
            this.TestsList = this.GetTestList();
            this.DeserializeAllPackagesV2();
            this.testBeforeCleanDictionary = this.GetToKnowCleaningTest();
            this.EnumeratePackagesV2();
            this.PackagesStatusDictionary = this.ConvertPackageDictionary(PackagesStatusDictionary);            
        }

        /// <summary>
        /// Deserialize all result packages and dictionary with packages data.
        /// </summary>
        private void DeserializeAllPackagesV2()
        {
            Console.WriteLine("Packages deserialization started..");

            JsonSerializerSettings packageSerializationSettings = new()
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

            foreach (string directory in Directory.GetDirectories(CounterSettings.PathToResults))
            {
                string platformName = Path.GetFileName(directory);

                string pathToTestplanList = Path.Combine(directory, "testplan.list");

                if (!File.Exists(pathToTestplanList))
                {
                    continue;
                }

                LinkedList<string> testsSequence = new(
                    File.ReadAllLines(Path.Combine(directory, "testplan.list")).Select(
                        e => e.Replace(".", "_").Replace("ANDRETURNERROR:", "")
                    )
                );

                this.platformCompletedTestSequenceList.Add(
                    platformName,
                    testsSequence.Select(e => (e, this.IsPreviousTestContainsCleaning(e, testsSequence))).ToList()
                );

                this._deserializedPackages.Add(platformName, new Dictionary<string, List<ProxyPackageInfoV2>>());

                Console.WriteLine($"Deserialize {platformName} packages..");

                List<string> completedTestSequenceList = 
                    File.ReadLines(Path.Combine(directory, "testplan.list")).Select(e => e.Replace(".", "_")).ToList();

                string[] subDirectories = Directory.GetDirectories(directory);

                int subDirectoriesCount = subDirectories.Length + 1;
                int counter = 1;

                foreach (string subDirectory in subDirectories)
                {
                    string testName = Path.GetFileName(subDirectory);

                    this._deserializedPackages[platformName].Add(testName, new List<ProxyPackageInfoV2>());
                    IEnumerable<string> jsonFiles = new List<string>();

                    try
                    {
                        jsonFiles =
                            Directory.GetFiles(subDirectory, "*.json", SearchOption.TopDirectoryOnly).OrderBy(e => Path.GetFileName(e));
                    }
                    catch
                    {
                        Console.WriteLine($"\nUnable to get files from {subDirectory}" +
                            $"\nToo long path, perhaps");
                        break;
                    }

                    foreach (string jsonFile in jsonFiles)
                    {
                        string packageContent;

                        try
                        {
                            packageContent = File.ReadAllText(jsonFile, Encoding.UTF8);
                        }
                        catch
                        {
                            Console.WriteLine($"\nUnable to read {jsonFile}" +
                                $"\nToo long path, perhaps");
                            break;
                        }

                        try
                        {
                            //TODO: handle it
                            ProxyPackageInfoV2 deserializedJsonV2 = JsonConvert.DeserializeObject<ProxyPackageInfoV2>(
                                packageContent,
                                packageSerializationSettings
                            );
                            /*
                            if (deserializedJsonV2.RequestJson == null)
                            {
                                continue;
                            }
                            */
                            this._deserializedPackages[platformName][testName].Add(deserializedJsonV2);
                        }
                        catch(Exception e)
                        {
                            if (!this.catchDeserializationErrorTests.ContainsKey(platformName))
                            {
                                this.catchDeserializationErrorTests.Add(platformName, new List<string>());
                            }

                            this.catchDeserializationErrorTests[platformName].Add(testName);

                            Console.WriteLine($"\n{Path.GetFileName(jsonFile)} {e.Message}");
                        }
                    }

                    if (this.TestsList.ContainsKey(testName.ToUpper()))
                    {
                        this.TestsList[testName.ToUpper()].Add(platformName);
                    }

                    Console.Write($"\r({++counter}/{subDirectoriesCount}) deserialized.");                    
                }

                if (counter == subDirectoriesCount)
                {
                    //TODO: not accurate
                    Console.Write($"\rAll {subDirectoriesCount} packages deserialized.");
                }

                Console.WriteLine("\n");
            }

            foreach (var completedTests in this.platformCompletedTestSequenceList)
            {
                string platform = completedTests.Key;

                this._linkedPackages.Add(platform, new LinkedList<ProxyPackageInfoV2>());

                Dictionary<string, List<ProxyPackageInfoV2>> completedTestsPackages = this._deserializedPackages[platform];

                foreach (var completedTest in completedTests.Value)
                {
                    if (!completedTestsPackages.Any(e => e.Key.Equals(completedTest.Item1, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    string potentialKey = completedTestsPackages.First(
                            e => e.Key.Equals(completedTest.Item1, StringComparison.OrdinalIgnoreCase)
                        ).Key;

                    List<ProxyPackageInfoV2> completedPackages = completedTestsPackages[potentialKey].OrderBy(e => e.Timestamp).ToList();

                    foreach (ProxyPackageInfoV2 completedPackage in completedPackages)
                    {/*
                        if (completedPackage.Timestamp == null && completedPackage.ResponseCode != 0 && completedPackage.ResponseCode != 404)
                        {
                            continue;
                        }

                        if (completedPackage.ResponseJson == null)
                        {
                            continue;
                        }
                        */
                        this._linkedPackages[platform].AddLast(completedPackage);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates packages in given result folder.
        /// </summary>
        /// <returns>Platform separated dictionary of dictionary of test and packages.</returns>
        private void EnumeratePackagesV2()
        {
            Console.WriteLine("Packages enumeration started..");

            foreach (var deserializedPlatformPackages in this._deserializedPackages)
            {
                string platformName = deserializedPlatformPackages.Key;

                this.PackagesStatusDictionary.Add(platformName, new Dictionary<string, TestPackagesData>());

                Console.WriteLine($"Enumerate {platformName} packages..");

                Dictionary<string, List<ProxyPackageInfoV2>> platformPackages =
                    deserializedPlatformPackages.Value;

                int platformPackagesCount = platformPackages.Count() + 1;
                int counter = 1;
                int skipCounter = 0;

                foreach (var testName in this.TestsList.Keys)
                {
                    this.PackagesStatusDictionary[platformName].Add(testName, new TestPackagesData());

                    if (!platformPackages.ContainsKey(testName.ToUpper())
                        && !this.TestsList[testName.ToUpper()].Contains(platformName))
                    {
                        skipCounter++;
                        continue;
                    }

                    List<ProxyPackageInfoV2> packagesList = 
                        platformPackages.FirstOrDefault(e => e.Key.Equals(testName, StringComparison.OrdinalIgnoreCase)).Value;

                    this.PackagesStatusDictionary[platformName][testName] = this.CalculatePackages(packagesList, testName, platformName);

                    Console.Write($"\r({++counter}/{platformPackagesCount}) enumerated. ({skipCounter}/{this.TestsList.Count}) tests wasn't in results folder.");                    
                }

                if (counter == platformPackagesCount)
                {
                    Console.Write($"\rAll {platformPackagesCount} packages enumerated. ({skipCounter}/{this.TestsList.Count}) tests wasn't in results folder.");
                }

                Console.WriteLine("\n");
            }
        }

        private IEnumerable<ProxyPackageInfoV2> FindAllPackagesOfEvent<T>(IEnumerable<ProxyPackageInfoV2> testPackages)
        {
            List<ProxyPackageInfoV2> packagesWithDesiredEventType = new();

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
            List<ProxyPackageInfoV2> testPackages,
            string testName,
            string platformName
        )
        {
            this._previousTestPackages = new List<ProxyPackageInfoV2>(testPackages);

            IEnumerable<string> doublesSignatures = this.PackagesDoubleCheck(testPackages, testPackages);
            IEnumerable<ProxyPackageInfoV2> alContainingPackages = this.FindAllPackagesOfEvent<AlV2>(testPackages);
            IEnumerable<ProxyPackageInfoV2> ueContainingPackages = this.FindAllPackagesOfEvent<UeV2>(testPackages);
            IEnumerable<ProxyPackageInfoV2> caContainingPackages = this.FindAllPackagesOfEvent<CaV2>(testPackages);

            ProxyPackageInfoV2 packageWithFirstUeEvent = this.GetJumpedEventFromPreviousTestOfType<UeV2>(testPackages, platformName);
            ProxyPackageInfoV2 packageWithLastAlEvent = this.GetPackageWithLastEventOfType<AlV2, UeV2>(testPackages);
            ProxyPackageInfoV2 packageWithLastUeEvent = this.GetPackageWithLastEventOfType<UeV2, AlV2>(testPackages);

            List<ProxyPackageInfoV2> badCodePackages = new();
            bool containsZeroResponseCode = false;

            if (CounterSettings.IgnoreBadCodePackages)
            {
                foreach (ProxyPackageInfoV2 testPackage in testPackages.Where(e => e.ResponseCode == 0 || e.ResponseCode == 404).ToList())
                {
                    containsZeroResponseCode = testPackage.ResponseCode == 0;

                    badCodePackages.Add(testPackage);
                }
            }

            foreach (ProxyPackageInfoV2 badCodePackage in badCodePackages)
            {
                testPackages.Remove(badCodePackage);
            }

            bool previousTestContainsClean = this.platformCompletedTestSequenceList.Any(e => e.Key.Equals(platformName, StringComparison.OrdinalIgnoreCase) &&
                e.Value.Any(x => x.Item1.Equals(testName, StringComparison.OrdinalIgnoreCase))) && this.platformCompletedTestSequenceList[platformName].Find(e => e.Item1.Equals(testName, StringComparison.OrdinalIgnoreCase)).Item2;

            if (previousTestContainsClean && (CounterSettings.IgnoreLastAl || CounterSettings.IgnoreBadUe))
            {
                testPackages.Remove(packageWithLastUeEvent);
                testPackages.Remove(packageWithLastAlEvent);
                testPackages.Remove(packageWithFirstUeEvent);
            }

            IEnumerable<ProxyPackageInfoV2> attemptPackages = 
                new List<ProxyPackageInfoV2>(testPackages.Where(e => e.RequestJson is UserIdentificationRequest));
            IEnumerable<ProxyPackageInfoV2> sdkVersionPackages = 
                new List<ProxyPackageInfoV2>(testPackages.Where(e => e.RequestJson is SdkVersionRequest));

            /*
            foreach (ProxyPackageInfoV2 attemptPackage in attemptPackages)
            {
                testPackages.Remove(attemptPackage);
            }
            */

            List<string> eventCodes = new();

            int index = 1;

            if (previousTestContainsClean && (CounterSettings.IgnoreLastAl || CounterSettings.IgnoreBadUe))
            {
                if (packageWithFirstUeEvent != null)
                {
                    foreach (AbstractSdkEventV2 abstractSdkEvent in packageWithFirstUeEvent.AllEvents())
                    {
                        //If ignored - don't print.
                        /*
                        eventCodes.Add($"(first){abstractSdkEvent.Code}");
                        index++;
                        */
                    }
                }
            }

            /*
             * It was here at the beginning
            foreach (ProxyPackageInfoV2 testPackage in testPackages.Where(e => e.RequestJson is AnalyticsRequest).OrderBy(e => e.Timestamp))
            {
                foreach (AbstractSdkEventV2 abstractSdkEvent in testPackage.AllEvents().OrderBy(e => e.Code))
                {
                    eventCodes.Add($"({index}){abstractSdkEvent.Code}");
                }

                index++;
            }
            */

            foreach (ProxyPackageInfoV2 badCodePackage in badCodePackages)
            {
                testPackages.Add(badCodePackage);
            }

            foreach (ProxyPackageInfoV2 testPackage in testPackages.Where(e => e.RequestJson is AnalyticsRequest or null).OrderBy(e => e.Timestamp))
            {
                int? responseCode = testPackage.ResponseCode;
                string requestUrl = testPackage.RequestUrl;

                if (requestUrl == null)
                {
                    eventCodes.Add($"[(#{index})NullPackage(Code:{responseCode})]");
                    index++;
                    continue;
                }

                if (testPackage.RequestJson == null && !requestUrl.Contains("config") && !requestUrl.Contains("identification"))
                {
                    eventCodes.Add($"[(#{index})NullRequest(Code:{responseCode})]");
                    index++;
                    continue;
                }

                foreach (AbstractSdkEventV2 abstractSdkEvent in testPackage.AllEvents())
                {
                    if (abstractSdkEvent is IHasTimestamp timestampEvent)
                    {
                        var timestamp = timestampEvent.Timestamp;
                        eventCodes.Add($"[(#{index}){abstractSdkEvent.Code}:(Code:{responseCode})]({timestamp})");
                        continue;
                    }

                    eventCodes.Add($"[(#{index}){abstractSdkEvent.Code}:(Code:{responseCode})]");
                }

                index++;
            }

            if (previousTestContainsClean && (CounterSettings.IgnoreLastAl || CounterSettings.IgnoreBadUe))
            {
                if (packageWithLastUeEvent != null)
                {
                    testPackages.Add(packageWithLastUeEvent);

                    foreach (AbstractSdkEventV2 abstractSdkEvent in packageWithLastUeEvent.AllEvents())
                    {
                        //If ignored - don't print.
                        //eventCodes.Add($"(last){abstractSdkEvent.Code}");
                    }
                }

                if (packageWithLastAlEvent != null)
                {
                    testPackages.Add(packageWithLastAlEvent);

                    foreach (AbstractSdkEventV2 abstractSdkEvent in packageWithLastAlEvent.AllEvents())
                    {
                        //If ignored - don't print.
                        //eventCodes.Add($"(last){abstractSdkEvent.Code}");
                    }
                }

                if (packageWithFirstUeEvent != null)
                {
                    testPackages.Add(packageWithFirstUeEvent);
                }
            }

            //TODO: this boi isn't work
            bool containsDeserializationErrors = this.catchDeserializationErrorTests.Any(e => e.Key.Equals(platformName, StringComparison.OrdinalIgnoreCase) &&
                e.Value.Any(x => x.Equals(testName, StringComparison.OrdinalIgnoreCase)));

            TestPackagesData testPackagesData = new(
                originalPackagesCount: testPackages.Count,
                packagesCount: testPackages.Count,
                alPackagesCount: alContainingPackages.Count(),
                uePackagesCount: ueContainingPackages.Count(),
                caPackagesCount: caContainingPackages.Count(),
                attemptPackagesCount: attemptPackages.Count(),
                sdkVersionCount: sdkVersionPackages.Count(),
                isLastAlRemoved: (previousTestContainsClean && CounterSettings.IgnoreLastAl) && packageWithLastAlEvent != null,
                isLastUeRemoved: (previousTestContainsClean && CounterSettings.IgnoreBadUe) && packageWithLastUeEvent != null,
                isFirstUeRemoved: packageWithFirstUeEvent != null,
                isAllEventsOrdered: this.CheckEventsTimestampOrder(testPackages.Where(e => e.ResponseCode != 0 && e.ResponseCode != 404).ToList(), platformName),
                events: eventCodes,
                doublesSignatures: doublesSignatures,
                badCodesPackages: badCodePackages,
                containsZeroCodePackage: containsZeroResponseCode,
                previousTestContainsCleaning: previousTestContainsClean,
                containsDeserializationErrors: containsDeserializationErrors
            );
            /*
            testPackagesData = new TestPackagesData(
                testPackages,
                this.platformCompletedTestSequenceList,
                this._linkedPackages,
                testName,
                platformName
            );
            */
            return testPackagesData;
        }

        private bool CheckEventsTimestampOrder(List<ProxyPackageInfoV2> testPackages, string platformName)
        {
            LinkedList<ProxyPackageInfoV2> linkedPackages = this._linkedPackages[platformName];

            if (!linkedPackages.Any())
            {
                return true;
            }

            foreach (ProxyPackageInfoV2 currentPackage in testPackages)
            {
                if (currentPackage.RequestJson == null)
                {
                    continue;
                }

                IEnumerable<IHasTimestamp> currentPackageTimestampEvents = 
                    currentPackage.AllEvents().OfType<IHasTimestamp>().OrderBy(e => e.Timestamp);

                if (!currentPackageTimestampEvents.Any())
                {
                    continue;
                }

                ulong minimalCurrentPackageTimestamp = (ulong)currentPackageTimestampEvents.First().Timestamp;
                ulong maximalCurrentPackageTimestamp = (ulong)currentPackageTimestampEvents.Last().Timestamp;

                if (linkedPackages.Find(currentPackage) == null)
                {
                    continue;
                }

                ProxyPackageInfoV2 nextPackage = currentPackage != linkedPackages.Last()
                    ? linkedPackages.Find(currentPackage).Next.Value 
                    : currentPackage;

                IEnumerable<IHasTimestamp> nextPackageTimestampEvents =
                    nextPackage.AllEvents().OfType<IHasTimestamp>().OrderBy(e => e.Timestamp);

                if (nextPackageTimestampEvents.Any() && nextPackage != currentPackage)
                {
                    ulong minimalNextPackageTimestamp = (ulong)nextPackageTimestampEvents.First().Timestamp;

                    if (maximalCurrentPackageTimestamp >= minimalNextPackageTimestamp)
                    {
                        return false;
                    }
                }

                ProxyPackageInfoV2 previousPackage = currentPackage != linkedPackages.First()
                    ? linkedPackages.Find(currentPackage).Previous.Value
                    : currentPackage;

                IEnumerable<IHasTimestamp> previousPackageTimestampEvents =
                    previousPackage.AllEvents().OfType<IHasTimestamp>().OrderBy(e => e.Timestamp);

                if (previousPackageTimestampEvents.Any() && previousPackage != currentPackage)
                {
                    ulong maximalPreviousPackageTimestamp = (ulong)previousPackageTimestampEvents.Last().Timestamp;

                    if (maximalPreviousPackageTimestamp >= minimalCurrentPackageTimestamp)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private ProxyPackageInfoV2 GetJumpedEventFromPreviousTestOfType<T>(
            IEnumerable<ProxyPackageInfoV2> testPackages,
            string platformName
        )
        {
            if (!this._linkedPackages.ContainsKey(platformName))
            {
                return null;
            }

            IEnumerable<IHasTimestamp> timestampEvents = 
                testPackages.AllEventsOfType<IHasTimestamp>().OrderBy(e => e.Timestamp);

            if (!timestampEvents.Any())
            {
                return null;
            }

            if (timestampEvents.First() is UeV2 firstUeEvent && firstUeEvent.FindPackage(testPackages).AllEvents().Count() == 1)
            {
                ProxyPackageInfoV2 firstUePackage = firstUeEvent.FindPackage(testPackages);

                if (this._linkedPackages[platformName].Find(firstUePackage) == null)
                {
                    return null;
                }

                ProxyPackageInfoV2 previousTestPackage = 
                    this._linkedPackages[platformName].Find(firstUePackage).Previous.Value;

                while (previousTestPackage.RequestJson is not AnalyticsRequest)
                {
                    previousTestPackage = this._linkedPackages[platformName].Find(previousTestPackage).Previous.Value;
                }

                string previousTestPackageTestName = previousTestPackage.TestName;

                if (firstUePackage.TestName.Equals(previousTestPackageTestName, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                IEnumerable<ProxyPackageInfoV2> previousTestPackages = 
                    this._linkedPackages[platformName].Where(e => e.TestName.Equals(previousTestPackageTestName, StringComparison.OrdinalIgnoreCase));

                SsV2 previousTestSsEvent = previousTestPackages.AllEventsOfType<SsV2>().FirstOrDefault();

                if (previousTestSsEvent == null)
                {
                    IEnumerable<IHasSessionID> sessionIdEvents = previousTestPackages.AllEventsOfType<IHasSessionID>();

                    if (sessionIdEvents.Any(e => e.SessionId == firstUeEvent.SessionId))
                    {
                        return firstUePackage;
                    }

                    return null;
                }

                ulong? previousTestSessionID = previousTestSsEvent.Timestamp;

                if (firstUeEvent.SessionId == previousTestSessionID)
                {
                    return firstUePackage;
                }
            }

            return null;
        }

        private ProxyPackageInfoV2 GetPackageWithLastEventOfType<T1, T2>(IEnumerable<ProxyPackageInfoV2> testPackages)
        {
            IEnumerable<T1> desiredTypeEvents = testPackages.AllEventsOfType<T1>();
            IEnumerable<IHasTimestamp> timestampEvents = testPackages.AllEventsOfType<IHasTimestamp>().OrderBy(e => e.Timestamp);

            if (desiredTypeEvents.Count() != 0 && timestampEvents.Count() != 0)
            {
                IHasTimestamp lastEventByTimestamp = timestampEvents.Last();
                
                foreach (T1 desiredTypeEvent in desiredTypeEvents)
                {
                    if (desiredTypeEvent is IHasTimestamp timestampEvent
                        && timestampEvent == lastEventByTimestamp
                    )
                    {
                        lastEventByTimestamp = timestampEvent;
                    }
                }

                if (lastEventByTimestamp is T1 &&
                    lastEventByTimestamp is AbstractSdkEventV2 firstTypeEvent
                )
                {
                    ProxyPackageInfoV2 packageWithDesiredEvent = firstTypeEvent.FindPackage(testPackages);
                    return packageWithDesiredEvent.AllEvents().Count() == 1 ? packageWithDesiredEvent : null;
                }

                if (lastEventByTimestamp is T2)
                {
                    List<IHasTimestamp> timestampEventsWithoutLast = new(timestampEvents);
                    timestampEventsWithoutLast.Remove(lastEventByTimestamp);

                    IHasTimestamp secondLastEventByTimestamp = timestampEventsWithoutLast.Last();

                    if (secondLastEventByTimestamp is T1 &&
                        secondLastEventByTimestamp is AbstractSdkEventV2 secondTypeEvent
                    )
                    {
                        ProxyPackageInfoV2 packageWithDesiredEvent = secondTypeEvent.FindPackage(testPackages);
                        return packageWithDesiredEvent.AllEvents().Count() == 1 ? packageWithDesiredEvent : null;
                    }
                }
            }

            return null;
        }

        private List<ProxyPackageInfoV2> GetTestPackagesWithoutDoubles(
            List<ProxyPackageInfoV2> deserializedTestPackages
        )
        {
            List<ProxyPackageInfoV2> testPackages = deserializedTestPackages;

            PackageComparerV2 packageComparer = new();

            if (this._previousTestPackages.Count != 0)
            {
                List<ProxyPackageInfoV2> testPackagesToRemove = new();

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
