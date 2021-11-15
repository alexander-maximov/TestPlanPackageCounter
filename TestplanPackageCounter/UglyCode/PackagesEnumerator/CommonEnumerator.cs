using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using TestplanPackageCounter.Counter;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Testplan.Content;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal abstract class CommonEnumerator
    {
        protected Dictionary<string, List<string>> catchDeserializationErrorTests = new Dictionary<string, List<string>>();

        protected List<string> platformList = new List<string>();

        protected Dictionary<string, bool> testBeforeCleanDictionary = new Dictionary<string, bool>();

        protected List<TestSuite> testSuites = new List<TestSuite>();

        protected Dictionary<string, List<(string, bool)>> platformCompletedTestSequenceList = new Dictionary<string, List<(string, bool)>>();

        internal Dictionary<string, Dictionary<string, TestPackagesData>> PackagesStatusDictionary { get; set; }

        internal Dictionary<string, int> MaxUeDictionary { get; set; }

        internal Dictionary<string, List<string>> TestsList { get; set; }

        internal virtual void Enumerate() { }

        protected bool IsPreviousTestContainsCleaning(string fullTestName, LinkedList<string> testsSequense)
        {
            if (fullTestName == testsSequense.First())
            {
                return true;
            }

            string previousTestFullName = testsSequense.Find(fullTestName).Previous.Value;

            string testSuiteName = previousTestFullName.Substring(0, previousTestFullName.IndexOf("_"));
            string testName = previousTestFullName.Replace(testSuiteName, "").Substring(1);

            ParamsNulls testParams = (ParamsNulls)(
                                        from testSuite in this.testSuites
                                        where testSuite.Name.Equals(testSuiteName, StringComparison.OrdinalIgnoreCase)
                                        from test in testSuite.Tests
                                        where test.Name.Equals(testName, StringComparison.OrdinalIgnoreCase)
                                        select test.Params
                                    ).FirstOrDefault();

            if (testParams != null)
            {
                return testParams.RestartMode == "BeforeTest" || testParams.CleaningMode == "BeforeTest";
            }

            return false;
        }

        protected Dictionary<string, bool> GetToKnowCleaningTest()
        {
            Dictionary<string, bool> previousTestIsCleanDictionary = new Dictionary<string, bool>();

            bool firstTestPlug = true;

            string previousTestSuiteName = string.Empty;
            string previousTestName = string.Empty;

            foreach (TestSuite testSuite in this.testSuites)
            {
                string testSuiteName = testSuite.Name.ToUpper();

                foreach (Test test in testSuite.Tests)
                {
                    string testName = test.Name.ToUpper();

                    ParamsNulls testParams = (ParamsNulls)test.Params;

                    bool testContainsCleaning = false;

                    if (testParams != null)
                    {
                        testContainsCleaning = testParams.RestartMode == "BeforeTest"
                        || testParams.CleaningMode == "BeforeTest";
                    }

                    if (!string.IsNullOrEmpty(previousTestName)
                        && !string.IsNullOrEmpty(previousTestSuiteName)
                    )
                    {
                        string testEntryName = string.Concat(previousTestSuiteName, "_", previousTestName);

                        previousTestIsCleanDictionary.Add(testEntryName, testContainsCleaning);
                    }

                    previousTestName = testName;
                    previousTestSuiteName = testSuiteName;
                }
            }

            string finalTestEntryName = string.Concat(previousTestSuiteName, "_", previousTestName);

            previousTestIsCleanDictionary.Add(finalTestEntryName, false);

            if (firstTestPlug)
            {
                previousTestIsCleanDictionary["ALIVESUITE_ALIVEWITHTIMEOUT"] = firstTestPlug;
            }

            return previousTestIsCleanDictionary;
        }

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

        protected Dictionary<string, List<string>> GetTestList()
        {
            Dictionary<string, List<string>> testList = new Dictionary<string, List<string>>();

            foreach (TestSuite testSuite in this.testSuites)
            {
                string testSuiteName = testSuite.Name;

                foreach (Test test in testSuite.Tests)
                {
                    string testName = test.Name;
                    string fullTestName = string.Concat(testSuiteName, "_", testName);

                    if (testList.ContainsKey(fullTestName))
                    {
                        continue;
                    }

                    testList.Add(fullTestName.ToUpper(), new List<string>());
                }
            }

            return testList;
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

        protected List<string> PackagesDoubleCheck<T>(List<T> testPackages, List<T> testPackagesOriginal)
        {
            List<string> doublesSignaturesList = new List<string>();

            if (testPackagesOriginal.Count != testPackages.Count)
            {
                IEnumerable<T> doublesPackages = testPackagesOriginal.Except(testPackages);

                foreach (T doublePackage in doublesPackages)
                {
                    if (doublePackage is ProxyPackageInfo packageInfo && packageInfo.RequestUrl != null)
                    {
                        NameValueCollection paramsUrl =
                            HttpUtility.ParseQueryString(new UriBuilder(packageInfo.RequestUrl).Query);

                        doublesSignaturesList.Add(paramsUrl["s"]);
                    }
                }
            }

            return doublesSignaturesList;
        }
    }
}
