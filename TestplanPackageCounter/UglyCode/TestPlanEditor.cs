using System;
using System.Collections.Generic;
using System.Linq;
using TestplanPackageCounter.Counter;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Testplan.Content;

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
            Platforms platform,
            int maxUe
        )
        {
            int maxCount = 0;

            foreach (var testPackages in testPackagesByPlatform)
            {
                if (testPackages.Key.Contains(patternsDictionary[platform]))
                {
                    int platformCount = testPackages.Value + maxUe;

                    maxCount = platformCount > maxCount ? platformCount : maxCount;
                }
            }

            if (maxCount == 0)
            {
                return null;
            }

            return maxCount;
        }

        internal void EditTestPlan()
        {
            this.EditedTestSuites = new List<TestSuite>();

            foreach (var testSuite in this._testSuites)
            {
                string testSuiteName = testSuite.Name;

                foreach (var test in testSuite.Tests)
                {
                    string testName = test.Name;
                    #region debug section
                    //TODO: remove me
                    string compareString = "StartAfterSwitchBeforeInit".ToLower();

                    if (testName.ToLower().Contains(compareString))
                    {
                        Console.Write("");
                    }
                    #endregion
                    string fullTestname = string.Concat(testSuiteName, "_", testName).ToUpper();

                    if (test.Params == null)
                    {
                        continue;
                    }

                    ParamsNulls testData = (ParamsNulls)test.Params;

                    int? defaultPackagesCount = testData.DefaultPackagesCount;
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
                        testData.DefaultPackagesCount = defaultPackagesCount;
                        testData.PlatformPackagesCount = null;
                        continue;
                    }
                    else
                    {
                        PlatformPackagesCount platformPackages = new PlatformPackagesCount
                        {
                            AndroidPackages = this.GetPackagesCountByPlatform(this._packagesDictionary[fullTestname], Platforms.Android, maxUeCount),
                            IosPackages = this.GetPackagesCountByPlatform(this._packagesDictionary[fullTestname], Platforms.IOS, maxUeCount),
                            MacOsPackages = this.GetPackagesCountByPlatform(this._packagesDictionary[fullTestname], Platforms.MacOS, maxUeCount),
                            UwpPackages = this.GetPackagesCountByPlatform(this._packagesDictionary[fullTestname], Platforms.Uwp, maxUeCount),
                            WindowsPackages = this.GetPackagesCountByPlatform(this._packagesDictionary[fullTestname], Platforms.Windows, maxUeCount)
                        };

                        #region find minimum

                        int? min = platformPackages.WindowsPackages;

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

                        if (platformPackages.AndroidPackages < min)
                        {
                            min = platformPackages.WindowsPackages;
                        }

                        #endregion

                        //TODO: if there is no packages for some platform - needs to take packages from platform or from default

                        testData.PlatformPackagesCount = new PlatformPackages
                        {
                            Android = platformPackages.AndroidPackages != min ? platformPackages.AndroidPackages : null,
                            Ios = platformPackages.IosPackages != min ? platformPackages.IosPackages : null,
                            MacOS = platformPackages.MacOsPackages != min ? platformPackages.MacOsPackages : null,
                            Uwp = platformPackages.UwpPackages != min ? platformPackages.UwpPackages : null,
                            Windows = null
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

                        if (min == null)
                        {
                            Console.WriteLine($"---{testSuite} {testName} Default packages has 0 value---");
                        }

                        testData.DefaultPackagesCount = min == null ? 0 : (int)min;
                    }

                    test.Params = testData;
                }
            }
        }
    }
}
