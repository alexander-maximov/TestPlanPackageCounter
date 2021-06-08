using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TestplanPackageCounter.Counter;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal class ReportGenerator
    {
        private readonly Dictionary<string, Dictionary<string, TestPackagesData>> _packagesDictionary;
        private readonly CounterSettings _counterSettings;
        private readonly List<string> _platformList;
        private readonly IEnumerable<string> _testsList;

        internal ReportGenerator(
            CounterSettings counterSettings, 
            Dictionary<string, Dictionary<string, TestPackagesData>> packagesDictionary,
            List<string> platformList,
            IEnumerable<string> testsList
        )
        {
            this._counterSettings = counterSettings;
            this._packagesDictionary = packagesDictionary;
            this._platformList = platformList;
            this._testsList = testsList;
        }

        /// <summary>
        /// Generate csv report.
        /// </summary>
        internal void WriteToCsv()
        {
            StringBuilder csvContent = new StringBuilder();
            GenerateTitleLine(csvContent);
            GenerateRestContent(csvContent, this._packagesDictionary);
            string filePath = Path.Combine(this._counterSettings.PathToResults, "packagesCountNew.csv");
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
            Dictionary<string, Dictionary<string, TestPackagesData>> convertedPackageDictionary =
                this.ConvertPackageDictionary(this._packagesDictionary);

            foreach (string platformName in this._platformList)
            {
                string filePath = Path.Combine(this._counterSettings.PathToResults, $"{platformName}_packagesCount.csv");

                StringBuilder csvContent = new StringBuilder();
                //title line
                csvContent.Append(platformName);
                csvContent.Append(";packages count");
                csvContent.Append(";original packages count");
                csvContent.Append(";ue packages count");
                csvContent.Append(";ue removed");
                csvContent.Append(";al packages count");
                csvContent.Append(";al removed");
                csvContent.Append(";attempt packages count");
                csvContent.Append(";ignored packages count");
                csvContent.Append(";extra events");
                csvContent.Append(";is events ordered");

                //rest content
                foreach (string testName in this._testsList)
                {
                    csvContent.AppendLine();
                    csvContent.Append(testName);

                    if (convertedPackageDictionary[platformName.ToUpper()].ContainsKey(testName.ToUpper()))
                    {
                        TestPackagesData testPackagesData = convertedPackageDictionary[platformName.ToUpper()][testName.ToUpper()];

                        csvContent.Append($";{testPackagesData.PackagesCountWithoutIgnored}");
                        csvContent.Append($";{testPackagesData.OriginalPackagesCount}");
                        csvContent.Append($";{testPackagesData.UePackagesCount}");
                        csvContent.Append($";{testPackagesData.IsLastUeEventRemoved}");
                        csvContent.Append($";{testPackagesData.AlPackagesCount}");
                        csvContent.Append($";{testPackagesData.IsLastAlEventRemoved}");
                        csvContent.Append($";{testPackagesData.AttemptPackagesCount}");
                        csvContent.Append($";{testPackagesData.IgnoredPackagesCount}");

                        //TODO: Screw csv! Write in ods or xls
                        //TODO: Screw this! Count events!
                        IEnumerable<string> extraEvents = this.FindExtraEvents(testName, platformName);

                        csvContent.Append(";");
                        if (extraEvents != null && extraEvents.Any())
                        {
                            csvContent.Append($"{string.Join("|", extraEvents)}");
                        }

                        csvContent.Append($";{testPackagesData.IsAllEventsOrdered}");
                    }
                }

                File.WriteAllText(filePath, csvContent.ToString());
            }
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
        private void GenerateRestContent(
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

                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.Append($"Original count: {testPackagesData.OriginalPackagesCount} |");

                        if (testPackagesData.IsDoublesRemoved)
                        {
                            stringBuilder.AppendLine($"Doubles count: {testPackagesData.DoublesSignatures.LongCount()} |");

                            foreach (string signature in testPackagesData.DoublesSignatures)
                            {
                                stringBuilder.Append($" - {signature} |");
                            }
                        }

                        if (testPackagesData.AlPackagesCount > 0)
                        {
                            stringBuilder.Append($"Al total: {testPackagesData.AlPackagesCount} |");

                            if (testPackagesData.IsLastAlEventRemoved)
                            {
                                stringBuilder.Append($"Without last: {testPackagesData.AlPackagesCountWithoutIgnored} |");
                            }
                        }

                        if (testPackagesData.UePackagesCount > 0)
                        {
                            stringBuilder.Append($"Ue total: {testPackagesData.UePackagesCount} |");

                            if (testPackagesData.IsLastUeEventRemoved)
                            {
                                stringBuilder.Append($"Without last: {testPackagesData.UePackagesCountWithoutIgnored} |");
                            }
                        }

                        if (testPackagesData.AttemptPackagesCount > 0)
                        {
                            string ignored = this._counterSettings.IgnoreUserIdentificationPackages ? "ignored" : "";

                            stringBuilder.Append($"Attempts {ignored}: {testPackagesData.AttemptPackagesCount} |");
                        }

                        int packgesCountWithoutDoublesAndLastEvents =
                                (
                                    this._counterSettings.IgnoreUserIdentificationPackages 
                                    ? testPackagesData.PackagesCountWithoutUeAndAl - testPackagesData.AttemptPackagesCount
                                    : testPackagesData.PackagesCountWithoutUeAndAl
                                )
                                + testPackagesData.UePackagesCountWithoutIgnored
                                + testPackagesData.AlPackagesCountWithoutIgnored;

                        packagesCount += $";{packgesCountWithoutDoublesAndLastEvents};{stringBuilder}";

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
                titleLine += $";{Path.GetFileName(directory)};Events data";
            }
            csvContent.AppendLine(titleLine);
        }
    }
}
