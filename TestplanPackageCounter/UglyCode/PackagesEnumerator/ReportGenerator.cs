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
        private List<string> _platformList;

        internal ReportGenerator(
            CounterSettings counterSettings, 
            Dictionary<string, Dictionary<string, TestPackagesData>> packagesDictionary,
            List<string> platformList
        )
        {
            this._counterSettings = counterSettings;
            this._packagesDictionary = packagesDictionary;
            this._platformList = platformList;
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
        /// <summary>
        /// Fill csv with values.
        /// </summary>
        /// <param name="csvContent">Csv contetn builder to fill.</param>
        /// <param name="convertedDictionary">Set of platforms and tests with packages counting.</param>
        private static void GenerateRestContent(
            StringBuilder csvContent,
            Dictionary<string, Dictionary<string, TestPackagesData>> convertedDictionary
        )
        {
            foreach (var testName in convertedDictionary)
            {
                string nameofTest = testName.Key;
                string packagesCount = "";
                foreach (var platformName in testName.Value.OrderBy(test => test.Key))
                {
                    TestPackagesData testPackagesData = platformName.Value;

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"Original count: {testPackagesData.OriginalPackagesCount} |");

                    if (testPackagesData.DoublesRemoved)
                    {
                        stringBuilder.AppendLine($"Doubles count: {testPackagesData.DoublesSignatures.Count} |");

                        foreach (string signature in testPackagesData.DoublesSignatures)
                        {
                            stringBuilder.Append($" - {signature} |");
                        }
                    }

                    if (testPackagesData.AlPackagesCount > 0)
                    {
                        stringBuilder.Append($"Al count: {testPackagesData.AlPackagesCount} |");
                        stringBuilder.Append($"Removed: {testPackagesData.AlPackagesCount - testPackagesData.AlPackagesCountWithoutIgnored} |");
                    }

                    if (testPackagesData.UePackagesCount > 0)
                    {
                        stringBuilder.Append($"Ue count: {testPackagesData.UePackagesCount} |");
                        stringBuilder.Append($"Removed: {testPackagesData.UePackagesCount - testPackagesData.UePackagesCountWithoutIgnored} |");
                    }

                    int packgesCountWithoutDoublesAndLastEvents = testPackagesData.PackagesCountWithoutUeAndAl + testPackagesData.UePackagesCountWithoutIgnored + testPackagesData.AlPackagesCountWithoutIgnored;

                    packagesCount += $";{packgesCountWithoutDoublesAndLastEvents};{stringBuilder.ToString()}";
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
