using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestplanPackageCounter.UglyCode
{
    internal class PackagesEnumerator
    {
        internal Dictionary<string, int> MaxUeDictionary { get; set; }

        internal Dictionary<string, Dictionary<string, string>> PackagesDictionary { get; set; }

        internal void Enumerate()
        {
            this.MaxUeDictionary = new Dictionary<string, int>();

            bool withoutSingleUeEvents = true;

            Dictionary<string, Dictionary<string, string>> packagesDictionary =
                    new Dictionary<string, Dictionary<string, string>>();

            List<string> platformList = new List<string>();

            string resultsPath = @"C:\Users\at\Downloads\results (1)";

            GetPlatformList(platformList, resultsPath);

            var csvContent = new StringBuilder();

            //Read in dict
            FillPackageDictionary(withoutSingleUeEvents, packagesDictionary, resultsPath);

            //Convert dict to another
            this.PackagesDictionary =
                ConvertPackageDictionary(packagesDictionary, platformList);

            //Write to csv.

            //WriteToCsv(resultsPath, csvContent, convertedDictionary);
        }

        private static void WriteToCsv(string resultsPath, StringBuilder csvContent, Dictionary<string, Dictionary<string, string>> convertedDictionary)
        {
            GenerateTitleLine(resultsPath, csvContent);
            GenerateRestContent(csvContent, convertedDictionary);

            string filePath = Path.Combine(resultsPath, "packagesCountNew.csv");

            File.WriteAllText(filePath, csvContent.ToString());
        }

        private static void GenerateRestContent(StringBuilder csvContent, Dictionary<string, Dictionary<string, string>> convertedDictionary)
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

        private static void GenerateTitleLine(string resultsPath, StringBuilder csvContent)
        {
            string titleLine = "-";

            foreach (string directory in Directory.GetDirectories(resultsPath))
            {
                titleLine += $",{Path.GetFileName(directory)}";
            }
            csvContent.AppendLine(titleLine);
        }

        private static Dictionary<string, Dictionary<string, string>> ConvertPackageDictionary(Dictionary<string, Dictionary<string, string>> packagesDictionary, List<string> platformList)
        {
            Dictionary<string, Dictionary<string, string>> convertedDictionary =
                            new Dictionary<string, Dictionary<string, string>>();

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

                    Dictionary<string, string> innerDictionary = new Dictionary<string, string>();
                    innerDictionary.Add(platform.Key, testName.Value);
                    convertedDictionary.Add(upperKey, innerDictionary);
                }
            }

            foreach (var testName in convertedDictionary)
            {
                foreach (var platformName in platformList)
                {
                    if (!testName.Value.Keys.Contains(platformName))
                    {
                        testName.Value.Add(platformName, "not exist on platform");
                    }
                }
            }

            return convertedDictionary;
        }

        private void FillPackageDictionary(bool withoutSingleUeEvents, Dictionary<string, Dictionary<string, string>> packagesDictionary, string resultsPath)
        {
            foreach (string directory in Directory.GetDirectories(resultsPath))
            {
                string platformName = Path.GetFileName(directory);                

                Dictionary<string, string> testPackagesDictionary = new Dictionary<string, string>();

                foreach (string subDirectory in Directory.GetDirectories(directory))
                {
                    string testName = Path.GetFileName(subDirectory);

                    int maxUeCount = 0;

                    string[] jsonFiles = Directory.GetFiles(subDirectory, "*.json");

                    string packagesCount =
                        jsonFiles.Length.ToString();                    

                    if (withoutSingleUeEvents)
                    {
                        int withoutUeCount = Convert.ToInt32(packagesCount);
                        int ueCount = 0;
                        foreach (var jsonFile in jsonFiles)
                        {
                            IEnumerable<string> ueLines = File.ReadLines(jsonFile).Skip(4).Take(9);

                            if (ueLines.First().Contains("ue")
                                && ueLines.Last().Contains("}")
                            )
                            {
                                withoutUeCount--;
                                ueCount++;
                            }
                            continue;
                        }
                        packagesCount = $"{withoutUeCount}";

                        if (ueCount > maxUeCount)
                        {
                            maxUeCount = ueCount;
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
        }

        private static void GetPlatformList(List<string> platformList, string resultsPath)
        {
            foreach (string directory in Directory.GetDirectories(resultsPath))
            {
                string platformName = Path.GetFileName(directory);
                platformList.Add(platformName);
            }
        }
    }
}
