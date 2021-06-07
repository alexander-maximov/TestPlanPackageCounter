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
                            stringBuilder.Append($"Al total: {testPackagesData.AlPackagesCount} |");

                            if (testPackagesData.LastAlEventRemoved)
                            {
                                stringBuilder.Append($"Without last: {testPackagesData.AlPackagesCountWithoutIgnored} |");
                            }
                        }

                        if (testPackagesData.UePackagesCount > 0)
                        {
                            stringBuilder.Append($"Ue total: {testPackagesData.UePackagesCount} |");

                            if (testPackagesData.LastUeEventRemoved)
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
