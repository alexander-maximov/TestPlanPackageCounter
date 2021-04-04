using System;
using System.Collections.Generic;
using System.Linq; 
using TestplanPackageCounter.TestplanContent;

namespace TestplanPackageCounter.UglyCode
{
    internal class TestPlanEditor
    {
        private List<TestSuite> _testSuites;

        private Dictionary<string, Dictionary<string, int>> _packagesDictionary;
        private Dictionary<string, int> _maxUeDictionary;

        internal List<TestSuite> EditedTestSuites { get; set; }

        internal TestPlanEditor(
            List<TestSuite> testSuites,
            Dictionary<string, Dictionary<string, int>> packagesDictionary,
            Dictionary<string, int> maxUeDictionary
        )
        {
            this._testSuites = testSuites;
            this._packagesDictionary = packagesDictionary;
            this._maxUeDictionary = maxUeDictionary;
        }

        private readonly Dictionary<Platforms, string> patternsDictionary =
            new Dictionary<Platforms, string>()
        {
            { Platforms.Android, "API" },
            { Platforms.IOS, "iOS" },
            { Platforms.MacOS, "MacOs" },
            { Platforms.Uwp, "uwp" },
            { Platforms.Windows, "win" }
        };

        private int? GetPackagesCountByPlatform(
            Dictionary<string, int> testPackagesByPlatform, 
            Platforms platform
        )
        {
            int maxCount = 0;

            foreach (var testPackages in testPackagesByPlatform)
            {
                if (testPackages.Key.Contains(patternsDictionary[platform]))
                {
                    int platformCount = testPackages.Value;

                    maxCount = platformCount > maxCount ? platformCount : maxCount;
                }
            }

            if (maxCount == 0)
            {
                return null;
            }

            return maxCount;
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
                    string fullTestname = string.Concat(testSuiteName, "_", testName).ToUpper();

                    if (test.Params == null)
                    {
                        continue;
                    }

                    ParamsNulls testData = (ParamsNulls)test.Params;

                    int defaultPackagesCount = testData.DefaultPackagesCount;
                    int maxUeCount = this._maxUeDictionary.ContainsKey(fullTestname)
                        ? this._maxUeDictionary[fullTestname]
                        : 0;

                    if(!this._packagesDictionary.ContainsKey(fullTestname))
                    {
                        continue;
                    }

                    bool allPackagesCountAreEqual = this._packagesDictionary[fullTestname].All(
                        packages => Convert.ToInt32(packages.Value) + maxUeCount == defaultPackagesCount
                    );

                    if (allPackagesCountAreEqual)
                    {
                        continue;
                    }
                    else
                    {
                        PlatformPackages platformPackages = new PlatformPackages
                        {
                            //TODO: under function
                            /*
                            AndroidPackages = this._packagesDictionary[fullTestname]["TestResults_API29"],                           
                            IosPackages = this._packagesDictionary[fullTestname]["TestResults_iOS_5S_12"],
                            MacOsPackages = this._packagesDictionary[fullTestname]["TestResults_MacOs"],
                            UwpPackages = this._packagesDictionary[fullTestname]["TestResults_uwpx64_NET_XAML"],
                            WindowsPackages = this._packagesDictionary[fullTestname]["TestResults_winx86_64_IL2CPP"]
                            */
                            AndroidPackages = this.GetPackagesCountByPlatform(this._packagesDictionary[fullTestname], Platforms.Android),
                            IosPackages = this.GetPackagesCountByPlatform(this._packagesDictionary[fullTestname], Platforms.IOS),
                            MacOsPackages = this.GetPackagesCountByPlatform(this._packagesDictionary[fullTestname], Platforms.MacOS),
                            UwpPackages = this.GetPackagesCountByPlatform(this._packagesDictionary[fullTestname], Platforms.Uwp),
                            WindowsPackages = this.GetPackagesCountByPlatform(this._packagesDictionary[fullTestname], Platforms.Windows)
                        };

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

                        bool allPackagesAreNull = 
                            testData.PlatformPackagesCount.Android == null
                            && testData.PlatformPackagesCount.Ios == null
                            && testData.PlatformPackagesCount.MacOS == null
                            && testData.PlatformPackagesCount.Uwp == null
                            && testData.PlatformPackagesCount.Windows == null;

                        if (allPackagesAreNull)
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
