using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestplanPackageCounter.UglyCode
{
    internal class PackagesEnumerator
    {
        private readonly CounterSettings _counterSettings;

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

            //Read in dict
            Dictionary<string, Dictionary<string, int>> packagesDictionary = 
                this.FillPackageDictionary();

            //Convert dict to another
            this.PackagesDictionary =
                ConvertPackageDictionary(packagesDictionary, platformList);

            if (this._counterSettings.WriteToCsv)
            {
                WriteToCsv();
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
            string titleLine = "";

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
