using System;
using System.Collections.Generic;
using System.Linq; 
using TestplanPackageCounter.TestplanContent;

namespace TestplanPackageCounter.UglyCode
{
    internal class TestSuiteEditor
    {
        private List<TestSuite> _testSuites;
        private Dictionary<string, Dictionary<string, string>> _packagesDictionary;
        private Dictionary<string, int> _maxUeDictionary;

        internal List<TestSuite> EditedTestSuites { get; set; }

        internal TestSuiteEditor(List<TestSuite> testSuites, Dictionary<string, Dictionary<string, string>> packagesDictionary, Dictionary<string, int> maxUeDictionary)
        {
            this._testSuites = testSuites;
            this._packagesDictionary = packagesDictionary;
            this._maxUeDictionary = maxUeDictionary;
        }

        internal void EditTestSuites()
        {
            this.EditedTestSuites = new List<TestSuite>();

            foreach (var testSuite in this._testSuites)
            {
                string testSuiteName = testSuite.Name;

                foreach (var test in testSuite.Tests)
                {
                    string testName = test.Name;

                    string fullname = string.Concat(testSuiteName, "_", testName).ToUpper();

                    if (test.Params == null)
                    {
                        continue;
                    }

                    ParamsNulls testData = (ParamsNulls)test.Params;

                    int defaultPackagesCount = testData.DefaultPackagesCount;

                    int maxUeCount = this._maxUeDictionary.ContainsKey(fullname)
                        ? this._maxUeDictionary[fullname]
                        : 0;

                    if(!this._packagesDictionary.ContainsKey(fullname))
                    {
                        continue;
                    }

                    if (this._packagesDictionary[fullname].All(packages => Convert.ToInt32(packages.Value) + maxUeCount == defaultPackagesCount))
                    {
                        continue;
                    }
                    else
                    {
                        PlatformPackages platformPackages = new PlatformPackages();
                        platformPackages.AndroidPackages = Convert.ToInt32(this._packagesDictionary[fullname]["TestResults_API29"]) + maxUeCount;
                        platformPackages.IosPackages = Convert.ToInt32(this._packagesDictionary[fullname]["TestResults_iOS_5S_12"]) + maxUeCount;
                        platformPackages.MacOsPackages = Convert.ToInt32(this._packagesDictionary[fullname]["TestResults_MacOs"]) + maxUeCount;
                        platformPackages.UwpPackages = Convert.ToInt32(this._packagesDictionary[fullname]["TestResults_uwpx64_NET_XAML"]) + maxUeCount;
                        platformPackages.WindowsPackages = Convert.ToInt32(this._packagesDictionary[fullname]["TestResults_winx86_64_IL2CPP"]) + maxUeCount;

                        //Find minimum

                        int? min = platformPackages.AndroidPackages;

                        if (platformPackages.IosPackages < min)
                        {
                            min = platformPackages.IosPackages;
                        }
                        if (platformPackages.MacOsPackages < min)
                        {
                            min = platformPackages.MacOsPackages;
                        }
                        if (platformPackages.UwpPackages < min)
                        {
                            min = platformPackages.UwpPackages;
                        }
                        if (platformPackages.WindowsPackages < min)
                        {
                            min = platformPackages.WindowsPackages;
                        }

                        //Find minimum ends

                        testData.PlatformPackagesCount = new TestplanContent.PlatformPackages
                        {
                            Android = platformPackages.AndroidPackages != min ? platformPackages.AndroidPackages : null,
                            Ios = platformPackages.IosPackages != min ? platformPackages.IosPackages : null,
                            MacOS = platformPackages.MacOsPackages != min ? platformPackages.MacOsPackages : null,
                            Uwp = platformPackages.UwpPackages != min ? platformPackages.UwpPackages : null,
                            Windows = platformPackages.WindowsPackages != min ? platformPackages.WindowsPackages: null
                        };

                        if (testData.PlatformPackagesCount.Android == null
                            && testData.PlatformPackagesCount.Ios == null
                            && testData.PlatformPackagesCount.MacOS == null
                            && testData.PlatformPackagesCount.Uwp == null
                            && testData.PlatformPackagesCount.Windows == null
                        )
                        {
                            testData.PlatformPackagesCount = null;
                        }

                        testData.DefaultPackagesCount = (int)min;

                        test.Params = testData;
                    }
                }
            }
        }
    }
}
