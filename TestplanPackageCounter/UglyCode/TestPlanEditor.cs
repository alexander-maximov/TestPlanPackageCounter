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

        private readonly Dictionary<Platforms, List<string>> _patternsDictionary =
            new Dictionary<Platforms, List<string>>()
        {
            { Platforms.Android, new List<string>() { "api", "android" } },
            { Platforms.IOS, new List<string>() { "ios" } },
            { Platforms.MacOS, new List<string>() { "macos" } },
            { Platforms.Uwp, new List<string>() { "uwp" } },
            { Platforms.Windows, new List<string>() { "win" } }
        };

        private bool IsPlatformValid(List<string> patterns, string platformName) =>
            patterns.Any(e => platformName.ToLower().Contains(e.ToLower()));

        private bool IsAllPackagesCountsAreEqualsDefault(
            Dictionary<string, int> testPackages,
            int ueCount,
            int? defaultPackagesCount
        ) => testPackages.All(e => e.Value + ueCount == defaultPackagesCount);

        private int? GetPackagesCountFromResults(
            Dictionary<string, int> testPackagesByPlatform, 
            Platforms platform,
            int maxUe
        )
        {
            int maxCount = 0;

            foreach (var testPackages in testPackagesByPlatform)
            {
                if (this.IsPlatformValid(this._patternsDictionary[platform], testPackages.Key))
                {
                    int platformCount = testPackages.Value + maxUe;

                    maxCount = platformCount > maxCount ? platformCount : maxCount;
                }
            }

            if (maxCount - maxUe < 1)
            {
                return null;
            }

            return maxCount;
        }

        private int? GetPlatformPackagesCount(
            Platforms platform,
            Dictionary<string, int> testPackagesByPlatform,
            ParamsNulls testData,
            int ueCount
        )
        {
            if (platform == Platforms.Uwp)
            {
                //TODO: This is a plug!
                return null;
            }

            return this.GetPackagesCountFromResults(testPackagesByPlatform, platform, ueCount)
            ?? this.PackagesFromPlan(testData, platform);
        }

        private int? PackagesFromPlan(ParamsNulls testData, Platforms platform)
        {            
            if (testData.PlatformPackagesCount != null)
            {
                switch (platform)
                {
                    case Platforms.Android:
                        return testData.PlatformPackagesCount.Android;
                    case Platforms.IOS:
                        return testData.PlatformPackagesCount.Ios;
                    case Platforms.MacOS:
                        return testData.PlatformPackagesCount.MacOS;
                    case Platforms.Unreal:
                        //U dont made it lol
                        break;
                    case Platforms.Uwp:
                        return testData.PlatformPackagesCount.Uwp;
                    case Platforms.Windows:
                        return testData.PlatformPackagesCount.Windows;
                    default:
                        break;
                }
            }

            return testData.DefaultPackagesCount;
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

                    if (this.IsAllPackagesCountsAreEqualsDefault(this._packagesDictionary[fullTestname], maxUeCount, defaultPackagesCount))
                    {
                        testData.DefaultPackagesCount = defaultPackagesCount;
                        testData.PlatformPackagesCount = null;
                        continue;
                    }
                    else
                    {
                        Dictionary<string, int> testPackagesByPlatform = this._packagesDictionary[fullTestname];

                        PlatformPackagesCount platformPackages = new PlatformPackagesCount
                        {
                            AndroidPackages = this.GetPlatformPackagesCount(
                                Platforms.Android, testPackagesByPlatform, testData, maxUeCount
                            ),
                            IosPackages = this.GetPlatformPackagesCount(
                                Platforms.IOS, testPackagesByPlatform, testData, maxUeCount
                            ),
                            MacOsPackages = this.GetPlatformPackagesCount(
                                Platforms.MacOS, testPackagesByPlatform, testData, maxUeCount
                            ),
                            UwpPackages = this.GetPlatformPackagesCount(
                                Platforms.Uwp, testPackagesByPlatform, testData, maxUeCount
                            ),
                            WindowsPackages = this.GetPlatformPackagesCount(
                                Platforms.Windows, testPackagesByPlatform, testData, maxUeCount
                            ),
                        };

                        int? windowsPackagesCount = platformPackages.WindowsPackages;

                        testData.PlatformPackagesCount = new PlatformPackages
                        {
                            Android = platformPackages.AndroidPackages != windowsPackagesCount ? platformPackages.AndroidPackages : null,
                            Ios = platformPackages.IosPackages != windowsPackagesCount ? platformPackages.IosPackages : null,
                            MacOS = platformPackages.MacOsPackages != windowsPackagesCount ? platformPackages.MacOsPackages : null,
                            Uwp = platformPackages.UwpPackages != windowsPackagesCount ? platformPackages.UwpPackages : null,
                            Windows = null
                        };

                        if (testData.PlatformPackagesCount.IsAllPackageEqualsNull())
                        {
                            testData.PlatformPackagesCount = null;
                        }

                        if (windowsPackagesCount == null)
                        {
                            Console.WriteLine($"---{testSuite} {testName} Default packages has 0 value---");
                            testData.DefaultPackagesCount = 0;
                        }
                        else
                        {
                            testData.DefaultPackagesCount = (int)windowsPackagesCount;
                        }
                    }

                    test.Params = testData;
                }
            }
        }
    }
}
