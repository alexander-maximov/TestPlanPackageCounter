using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TestplanPackageCounter.Counter;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Testplan.Content;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal class ReportGenerator
    {
        private readonly Dictionary<string, Dictionary<string, TestPackagesData>> _packagesDictionary;
        private readonly List<string> _platformList;
        private readonly List<TestSuite> _testSuites;

        internal ReportGenerator(
            Dictionary<string, Dictionary<string, TestPackagesData>> packagesDictionary,
            List<string> platformList,
            List<TestSuite> testSuites
        )
        {
            //TODO: Write to excel
            this._packagesDictionary = packagesDictionary;
            this._platformList = platformList;
            this._testSuites = testSuites;
        }

        internal void OverwhelmingOne()
        {
            string filePath = Path.Combine(CounterSettings.PathToResults, "Overwhelming_packagesData.csv");

            StringBuilder csvContent = new();

            this._GenerateTitleLine("Test name", csvContent, true, "Platform");

            int equalsCount = 0;
            ulong eventsCountTotal = 0;

            foreach (KeyValuePair<string, Dictionary<string, TestPackagesData>> packageData in this._packagesDictionary)
            {
                string testName = packageData.Key;
                bool breakCircle = false;
                bool commonExist = false;

                if (this.CompareEventCodes(packageData.Value))
                {
                    equalsCount++;
                    Console.Write($"\rEquals tests count: {equalsCount}");
                    breakCircle = true;
                }

                IEnumerable<IEnumerable<string>> eventCodesCollection = from packageInfo in packageData.Value
                                                                        select this.TrimTimestamp(packageInfo.Value.EventCodes);

                IEnumerable<string> eventCodesWithoutTimestamp = this.Mode(eventCodesCollection, out int commonCount);

                foreach (KeyValuePair<string, TestPackagesData> platformPackageData in packageData.Value)
                {
                    string platformName = breakCircle ? "All" : platformPackageData.Key.Replace("TestResults_", "");

                    TestPackagesData testPackagesData = platformPackageData.Value;
                    eventsCountTotal += (ulong)testPackagesData.EventCodes.Count();

                    if (!breakCircle && Enumerable.SequenceEqual(eventCodesWithoutTimestamp, this.TrimTimestamp(testPackagesData.EventCodes)))
                    {
                        if (commonExist)
                        {
                            continue;
                        }

                        platformName = $"Common x{commonCount} ({platformName} like)";
                        commonExist = true;
                    }

                    if (testPackagesData.PackagesCountWithoutIgnored == 999)
                    {
                        continue;
                    }

                    csvContent.Append(testName);
                    csvContent.Append($";{platformName}");
                    
                    this._GeneratePackagesData(csvContent, testPackagesData);

                    csvContent.AppendLine();

                    if (breakCircle)
                    {
                        breakCircle = false;
                        break;
                    }
                }

                if (!packageData.Value.All(e => e.Value.PackagesCountWithoutIgnored == 999))
                {
                    csvContent.AppendLine();
                }
            }

            Console.WriteLine($"\nTotal events count in report: {eventsCountTotal}");

            File.WriteAllText(filePath, csvContent.ToString());
        }

        private IEnumerable<string> Mode(IEnumerable<IEnumerable<string>> list, out int max)
        {
            max = 0;
            // Initialize the return value
            IEnumerable<string> mode = default;

            // Test for a null reference and an empty list
            if (list != null && list.Count() > 0)
            {
                // Store the number of occurences for each element
                Dictionary<IEnumerable<string>, int> counts = new();

                // Add one to the count for the occurence of a character
                foreach (IEnumerable<string> element in list)
                {
                    if (counts.FirstOrDefault(e => Enumerable.SequenceEqual(e.Key, element)) is KeyValuePair<IEnumerable<string>, int> keyValuePair && keyValuePair.Key is not null)
                    {
                        counts[keyValuePair.Key]++;
                        continue;
                    }
                        
                    counts.Add(element, 1);
                }

                // Loop through the counts of each element and find the 
                // element that occurred most often

                foreach (KeyValuePair<IEnumerable<string>, int> count in counts)
                {
                    if (count.Value > max)
                    {
                        // Update the mode
                        mode = count.Key;
                        max = count.Value;
                    }
                }
            }
            return mode;
        }

        private bool CompareEventCodes(Dictionary<string, TestPackagesData> testPackagesDataDict)
        {
            IEnumerable<TestPackagesData> defaultFreePackagesData = testPackagesDataDict.Values.Where(e => e.OriginalPackagesCount != 999);

            if (!defaultFreePackagesData.Any())
            {
                return false;
            }

            TestPackagesData firstPackageData = defaultFreePackagesData.FirstOrDefault(e => e.OriginalPackagesCount != 999);

            if (firstPackageData is null)
            {
                return false;
            }

            IEnumerable<string> firstEventCodes = this.TrimTimestamp(firstPackageData.EventCodes);

            return defaultFreePackagesData.All(
                e => Enumerable.SequenceEqual(
                    this.TrimTimestamp(e.EventCodes), 
                    this.TrimTimestamp(firstEventCodes)
                )
            );
        }

        private IEnumerable<string> TrimTimestamp(IEnumerable<string> eventCodes)
        {
            foreach (string eventCode in eventCodes)
            {
                yield return Regex.Match(eventCode, "\\[(.)*\\]").ToString();
            }
        }

        private void _GeneratePackagesData(StringBuilder csvContent, TestPackagesData testPackagesData)
        {
            csvContent.Append($";{testPackagesData.OriginalPackagesCount}");
            csvContent.Append($";{testPackagesData.PackagesCount}");
            csvContent.Append($";{testPackagesData.UePackagesCount}");
            csvContent.Append($";{testPackagesData.IsLastUeEventRemoved}");
            csvContent.Append($";{testPackagesData.AlPackagesCount}");
            csvContent.Append($";{testPackagesData.IsLastAlEventRemoved}");
            csvContent.Append($";{testPackagesData.SdkVersionCount}");
            csvContent.Append($";{testPackagesData.AttemptPackagesCount}");
            csvContent.Append($";{testPackagesData.BadCodesPackages.Count()}");
            csvContent.Append($";{testPackagesData.IgnoredPackagesCount}");
            csvContent.Append($";{testPackagesData.EventsCount}");
            csvContent.Append($";{testPackagesData.IsAllEventsOrdered}");
            csvContent.Append($";{testPackagesData.ContainsZeroCodePackage}");
            csvContent.Append($";{testPackagesData.PreviousTestContainsCleaning}");
            csvContent.Append($";{testPackagesData.ContainsDeserializationErrors}");

            if (testPackagesData.EventCodes == null)
            {
                return;
            }

            foreach (string eventCode in testPackagesData.EventCodes)
            {
                csvContent.Append($";{eventCode}");
            }
        }

        /// <summary>
        /// Generate csv report.
        /// </summary>
        internal void WriteToCsv()
        {
            StringBuilder csvContent = new();
            _GenerateTitleLine(csvContent);
            _GenerateRestPackageContent(csvContent, this._packagesDictionary);
            string filePath = Path.Combine(CounterSettings.PathToResults, "packagesCountNew.csv");
            File.WriteAllText(filePath, csvContent.ToString());
        }

        internal void EasyTestplanReport()
        {
            StringBuilder csvContent = new();

            csvContent.Append("Testplan packages");
            csvContent.Append(";Default count");

            foreach (Platforms platform in Enum.GetValues(typeof(Platforms)))
            {
                switch (platform)
                {
                    case Platforms.Android:
                    case Platforms.IOS:
                    case Platforms.MacOS:
                    case Platforms.Uwp:
                    case Platforms.Windows:
                        csvContent.Append($";{platform}");
                        break;
                    default:
                        continue;
                }
            }

            csvContent.AppendLine();

            foreach (TestSuite testSuite in this._testSuites)
            {
                string testSuiteName = testSuite.Name;

                foreach (Test test in testSuite.Tests)
                {
                    string testName = test.Name;

                    string fullTestName = string.Concat(testSuiteName, "_", testName);

                    if (!this._packagesDictionary.ContainsKey(fullTestName.ToUpper()))
                    {
                        continue;
                    }

                    if (test.Params == null)
                    {
                        continue;
                    }

                    ParamsNulls testParams = (ParamsNulls)test.Params;

                    if (testParams.DefaultPackagesCount == null)
                    {
                        continue;
                    }

                    int defaultCount = (int)testParams.DefaultPackagesCount;
                    (int android, int ios, int macOs, int uwp, int windows) platformCount = (0, 0, 0, 0, 0);
                    
                    if (testParams.PlatformPackagesCount != null)
                    {
                        PlatformPackages platformPackages = testParams.PlatformPackagesCount;

                        platformCount.android = (int)(platformPackages.Android != null ? platformPackages.Android : 0);
                        platformCount.ios = (int)(platformPackages.Ios != null ? platformPackages.Ios : 0);
                        platformCount.macOs = (int)(platformPackages.MacOS != null ? platformPackages.MacOS : 0);
                        platformCount.uwp = (int)(platformPackages.Uwp != null ? platformPackages.Uwp : 0);
                        platformCount.windows = (int)(platformPackages.Windows != null ? platformPackages.Windows : 0);
                    }                    

                    csvContent.Append($"{fullTestName};");
                    csvContent.Append($"{defaultCount};");
                    csvContent.Append($"{platformCount.android};");
                    csvContent.Append($"{platformCount.ios};");
                    csvContent.Append($"{platformCount.macOs};");
                    csvContent.Append($"{platformCount.uwp};");
                    csvContent.AppendLine($"{platformCount.windows};");
                }
            }

            string filePath = Path.Combine(CounterSettings.PathToResults, "EasyTestplanView.csv");
            File.WriteAllText(filePath, csvContent.ToString());
        }

        private void _GenerateTitleLine(string heading, StringBuilder csvContent, bool doubleHeading = false, string doubleHeadingLine = "")
        {
            csvContent.Append(heading);

            if (doubleHeading)
            {
                csvContent.Append($";{doubleHeadingLine}");
            }

            csvContent.Append(";Testplan packages count");
            csvContent.Append(";Packages count in result");
            csvContent.Append(";Ue packages count");
            csvContent.Append(";Ue removed");
            csvContent.Append(";Al packages count");
            csvContent.Append(";Al removed");
            csvContent.Append(";SdkVersion count");
            csvContent.Append(";Attempt packages count");
            csvContent.Append(";Bad code packages count");
            csvContent.Append(";Ignored packages count");
            csvContent.Append(";Events count");
            csvContent.Append(";Is events ordered");
            csvContent.Append(";Contains code 0");
            csvContent.Append(";Next test contains cleaning");
            csvContent.AppendLine(";Contains deserialization errors");
        }

        /// <summary>
        /// Fill csv with values.
        /// </summary>
        /// <param name="csvContent">Csv contetn builder to fill.</param>
        /// <param name="packagesDataDictionary">Set of platforms and tests with packages counting.</param>
        private void _GenerateRestPackageContent(
            StringBuilder csvContent,
            Dictionary<string, Dictionary<string, TestPackagesData>> packagesDataDictionary
        )
        {
            foreach (var packagesData in packagesDataDictionary)
            {
                string nameofTest = packagesData.Key;

                string packagesCount = "";

                Dictionary<string, TestPackagesData> packagesDataByPlatforms = packagesData.Value;

                foreach (string platformName in this._platformList)
                {
                    if (packagesDataByPlatforms.ContainsKey(platformName))
                    {
                        TestPackagesData testPackagesData = packagesDataByPlatforms[platformName];

                        int packgesCountWithoutDoublesAndLastEvents =
                                (
                                    CounterSettings.IgnoreUserIdentificationPackages 
                                    ? testPackagesData.PackagesCountWithoutAlCaUe - testPackagesData.AttemptPackagesCount
                                    : testPackagesData.PackagesCountWithoutAlCaUe
                                )
                                + testPackagesData.UePackagesCountWithoutIgnored
                                + testPackagesData.AlPackagesCountWithoutIgnored;

                        packagesCount += $";{packgesCountWithoutDoublesAndLastEvents}";

                        continue;
                    }

                    packagesCount += ";;";
                }
                string newLine = string.Format("{0}{1}", nameofTest, packagesCount);
                csvContent.AppendLine(newLine);
            }
        }
        /// <summary>
        /// Adds title line to csv.
        /// </summary>        
        /// <param name="csvContent">Csv contetn builder to fill.</param>
        private void _GenerateTitleLine(StringBuilder csvContent)
        {
            string titleLine = "";

            foreach (string directory in this._platformList)
            {
                titleLine += $";{Path.GetFileName(directory)}";
            }
            csvContent.AppendLine(titleLine);
        }
    }
}
