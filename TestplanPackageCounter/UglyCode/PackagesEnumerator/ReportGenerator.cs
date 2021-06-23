using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TestplanPackageCounter.Counter;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Testplan.Content;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal class ReportGenerator
    {
        private readonly Dictionary<string, Dictionary<string, TestPackagesData>> convertedPackageDictionary;
        private readonly Dictionary<string, Dictionary<string, TestPackagesData>> _packagesDictionary;
        private readonly CounterSettings _counterSettings;
        private readonly List<string> _platformList;
        private readonly Dictionary<string, List<string>> _testsList;
        private readonly List<TestSuite> _testSuites;

        internal ReportGenerator(
            CounterSettings counterSettings, 
            Dictionary<string, Dictionary<string, TestPackagesData>> packagesDictionary,
            List<string> platformList,
            Dictionary<string, List<string>> testsList,
            List<TestSuite> testSuites
        )
        {
            //TODO: Write to excel

            this._counterSettings = counterSettings;
            this._packagesDictionary = packagesDictionary;
            this._platformList = platformList;
            this._testsList = testsList;
            this._testSuites = testSuites;
            this.convertedPackageDictionary = this.ConvertPackageDictionary(packagesDictionary);
        }

        internal void OverwhelmingOne()
        {
            string filePath = Path.Combine(this._counterSettings.PathToResults, "Overwhelming_packagesData.csv");

            StringBuilder csvContent = new StringBuilder();

            //TODO: separate platform and test
            this.GenerateTitleLine("Test name", csvContent, true, "Platform");

            foreach (var packageData in this._packagesDictionary)
            {
                string testName = packageData.Key;

                foreach (var platformPackageData in packageData.Value)
                {
                    string platformName = platformPackageData.Key.Replace("TestResults_", "");

                    TestPackagesData testPackagesData = platformPackageData.Value;

                    if (testPackagesData.PackagesCountWithoutIgnored == 999)
                    {
                        continue;
                    }

                    csvContent.Append(testName);
                    csvContent.Append($";{platformName}");
                    
                    this.GeneratePackagesData(csvContent, testPackagesData);

                    csvContent.AppendLine();
                }
            }

            File.WriteAllText(filePath, csvContent.ToString());
        }

        private void GeneratePackagesData(StringBuilder csvContent, TestPackagesData testPackagesData)
        {
            csvContent.Append($";{testPackagesData.PackagesCountWithoutIgnored}");
            csvContent.Append($";{testPackagesData.OriginalPackagesCount}");
            csvContent.Append($";{testPackagesData.UePackagesCount}");
            csvContent.Append($";{testPackagesData.IsLastUeEventRemoved}");
            csvContent.Append($";{testPackagesData.AlPackagesCount}");
            csvContent.Append($";{testPackagesData.IsLastAlEventRemoved}");
            csvContent.Append($";{testPackagesData.AttemptPackagesCount}");
            csvContent.Append($";{testPackagesData.BadCodesPackages.Count()}");
            csvContent.Append($";{testPackagesData.IgnoredPackagesCount}");
            csvContent.Append($";{testPackagesData.Events.Count()}");
            csvContent.Append($";{testPackagesData.IsAllEventsOrdered}");
            csvContent.Append($";{testPackagesData.ContainsZeroCodePackage}");
            
            foreach (string eventCode in testPackagesData.Events.OrderBy(e => e))
            {
                csvContent.Append($";{eventCode}");
            }
        }

        /// <summary>
        /// Generate csv report.
        /// </summary>
        internal void WriteToCsv()
        {
            StringBuilder csvContent = new StringBuilder();
            GenerateTitleLine(csvContent);
            GenerateRestPackageContent(csvContent, this._packagesDictionary);
            string filePath = Path.Combine(this._counterSettings.PathToResults, "packagesCountNew.csv");
            File.WriteAllText(filePath, csvContent.ToString());
        }

        internal void EasyTestplanReport()
        {
            StringBuilder csvContent = new StringBuilder();

            csvContent.Append("Testplan packages");

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

                        platformCount.android = (int)(platformPackages.Android != null ? platformPackages.Android : defaultCount);
                        platformCount.ios = (int)(platformPackages.Ios != null ? platformPackages.Ios : defaultCount);
                        platformCount.macOs = (int)(platformPackages.MacOS != null ? platformPackages.MacOS : defaultCount);
                        platformCount.uwp = (int)(platformPackages.Uwp != null ? platformPackages.Uwp : defaultCount);
                        platformCount.windows = (int)(platformPackages.Windows != null ? platformPackages.Windows : defaultCount);
                    }                    

                    csvContent.Append($"{fullTestName};");
                    csvContent.Append($"{platformCount.android};");
                    csvContent.Append($"{platformCount.ios};");
                    csvContent.Append($"{platformCount.macOs};");
                    csvContent.Append($"{platformCount.uwp};");
                    csvContent.AppendLine($"{platformCount.windows};");
                }
            }

            string filePath = Path.Combine(this._counterSettings.PathToResults, "EasyTestplanView.csv");
            File.WriteAllText(filePath, csvContent.ToString());
        }

        private Dictionary<string, Dictionary<string, TestPackagesData>> ConvertPackageDictionary(
            Dictionary<string, Dictionary<string, TestPackagesData>> packagesDictionary
        )
        {
            Dictionary<string, Dictionary<string, TestPackagesData>> convertedDictionary =
                new Dictionary<string, Dictionary<string, TestPackagesData>>();

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

                    Dictionary<string, TestPackagesData> innerDictionary = new Dictionary<string, TestPackagesData>
                    {
                        { platform.Key, testName.Value }
                    };

                    convertedDictionary.Add(upperKey, innerDictionary);
                }
            }

            return convertedDictionary;
        }

        internal void WriteToEachPlatform()
        {
            foreach (string platformName in this._platformList)
            {
                string filePath = Path.Combine(this._counterSettings.PathToResults, $"{platformName}_packagesCount.csv");

                StringBuilder csvContent = new StringBuilder();
                //title line
                this.GenerateTitleLine(platformName, csvContent);

                //rest content
                foreach (var testName in this._testsList)
                {
                    csvContent.AppendLine();
                    csvContent.Append(testName.Key);

                    if (this.convertedPackageDictionary[platformName.ToUpper()].ContainsKey(testName.Key.ToUpper()))
                    {
                        TestPackagesData testPackagesData = this.convertedPackageDictionary[platformName.ToUpper()][testName.Key.ToUpper()];

                        this.GeneratePackagesData(csvContent, testPackagesData);
                    }
                }

                File.WriteAllText(filePath, csvContent.ToString());
            }
        }

        private void GenerateTitleLine(string heading, StringBuilder csvContent, bool doubleHeading = false, string doubleHeadingLine = "")
        {
            csvContent.Append(heading);

            if (doubleHeading)
            {
                csvContent.Append($";{doubleHeadingLine}");
            }

            csvContent.Append(";packages count");
            csvContent.Append(";original packages count");
            csvContent.Append(";ue packages count");
            csvContent.Append(";ue removed");
            csvContent.Append(";al packages count");
            csvContent.Append(";al removed");
            csvContent.Append(";attempt packages count");
            csvContent.Append(";bad code packages count");
            csvContent.Append(";ignored packages count");
            csvContent.Append(";events count");
            csvContent.Append(";is events ordered");
            csvContent.AppendLine(";contains code 0");
        }

        private IEnumerable<string> FindExtraEvents(string testName, string platformName)
        {
            Dictionary<string, TestPackagesData> testData = this._packagesDictionary[testName];

            int min = int.MaxValue;

            foreach (TestPackagesData testPackagesData in testData.Values)
            {
                if (testPackagesData.Events.Count() < min)
                {
                    min = testPackagesData.Events.Count();
                }
            }

            IEnumerable<string> smallestEventsList = testData.First(e => e.Value.Events.Count() == min).Value.Events;

            if (testData.ContainsKey(platformName))
            {
                TestPackagesData testPackagesData = testData[platformName];

                return smallestEventsList.Except(testPackagesData.Events);
            }

            return null;
        }

        /// <summary>
        /// Fill csv with values.
        /// </summary>
        /// <param name="csvContent">Csv contetn builder to fill.</param>
        /// <param name="packagesDataDictionary">Set of platforms and tests with packages counting.</param>
        private void GenerateRestPackageContent(
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
                                    this._counterSettings.IgnoreUserIdentificationPackages 
                                    ? testPackagesData.PackagesCountWithoutUeAndAl - testPackagesData.AttemptPackagesCount
                                    : testPackagesData.PackagesCountWithoutUeAndAl
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
        private void GenerateTitleLine(StringBuilder csvContent)
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
