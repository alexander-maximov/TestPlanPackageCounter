using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestplanPackageCounter.Counter;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal abstract class CommonEnumerator
    {
        protected CounterSettings _counterSettings;

        protected List<string> _platformList = new List<string>();

        protected Dictionary<string, bool> _testBeforeCleanDictionary = new Dictionary<string, bool>();

        internal Dictionary<string, Dictionary<string, TestPackagesData>> PackagesStatusDictionary { get; set; }

        internal Dictionary<string, Dictionary<string, int>> PackagesDictionary { get; set; }

        internal Dictionary<string, int> MaxUeDictionary { get; set; }

        internal virtual void Enumerate() { }

        /// <summary>
        /// Get list of platforms from results.
        /// </summary>
        /// <param name="resultsPath">Path to results to generate list of platforms based on folder names.</param>
        /// <returns>List of platforms.</returns>
        protected List<string> GetPlatformList(string resultsPath)
        {
            List<string> platformList = new List<string>();

            foreach (string directory in Directory.GetDirectories(resultsPath))
            {
                string platformName = Path.GetFileName(directory);

                platformList.Add(platformName);
            }

            return platformList;
        }

        /// <summary>
        /// Convert packages dictionary from platform separated dictionary of dictionary of test and packages count 
        /// to dictionary of tests and packages count, separated by platform.
        /// </summary>
        /// <param name="packagesDictionary">Source dictionary.</param>
        /// <param name="platformList">List of platforms to separate packages count.</param>
        /// <returns>Dictionary of tests and packages count, separated by platform.</returns>
        protected Dictionary<string, Dictionary<string, TestPackagesData>> ConvertPackageDictionary(
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
    }
}
