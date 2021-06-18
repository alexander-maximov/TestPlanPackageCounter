using System;
using System.Collections.Generic;
using System.Linq;
using TestplanPackageCounter.Counter;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Testplan.Content;
using TestplanPackageCounter.UglyCode.PackagesEnumerator;

namespace TestplanPackageCounter.UglyCode
{
    internal class TestPlanEditor
    {
        private readonly List<TestSuite> _testSuites;

        private readonly Dictionary<string, Dictionary<string, TestPackagesData>> _packagesDictionary;

        private readonly CounterSettings _counterSettings;

        internal List<TestSuite> EditedTestSuites { get; set; }

        internal TestPlanEditor(
            List<TestSuite> testSuites,
            Dictionary<string, Dictionary<string, TestPackagesData>> packagesDictionary,
            CounterSettings counterSettings
        )
        {
            this._testSuites = testSuites;
            this._packagesDictionary = packagesDictionary;
            this._counterSettings = counterSettings;
        }

        private int CountMaxUeAmongPlatforms(string testName)
        {
            int maxUeCount = 0;

            if (!this._packagesDictionary.ContainsKey(testName))
            {
                return maxUeCount;
            }

            foreach (TestPackagesData platformTestData in this._packagesDictionary[testName].Values)
            {
                int platformUePackages = platformTestData.UePackagesCountWithoutIgnored;

                maxUeCount = platformUePackages > maxUeCount ? platformUePackages : maxUeCount;
            }

            return maxUeCount;
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
            Dictionary<string, TestPackagesData> testPackages,
            int ueCount, //TODO: ueMax!
            int? defaultPackagesCount
        )
        {
            if (this._counterSettings.CalculatePackagesWithMaxUe)
            {
                return testPackages.All(
                  e => (
                      e.Value.PackagesCountWithoutUeAndAl
                      + ueCount
                      + e.Value.AlPackagesCountWithoutIgnored
                  ) == defaultPackagesCount
              );
            }

            if (testPackages.All(e => e.Value.UePackagesCountWithoutIgnored == testPackages.First().Value.UePackagesCountWithoutIgnored))
            {
                return testPackages.All(
                    e => (
                        e.Value.PackagesCountWithoutUeAndAl
                        + e.Value.UePackagesCountWithoutIgnored
                        + e.Value.AlPackagesCountWithoutIgnored
                    ) == defaultPackagesCount
                );
            }

            return false;
        }

        private int? GetPackagesCountFromResults(
            Dictionary<string, TestPackagesData> testPackagesByPlatform, 
            Platforms platform,
            int maxUe
        )
        {
            int maxCount = 0;

            foreach (var testPackages in testPackagesByPlatform)
            {
                string platformName = testPackages.Key;

                if (this.IsPlatformValid(this._patternsDictionary[platform], platformName))
                {
                    TestPackagesData testPackagesData = testPackages.Value;

                    if (this._counterSettings.IgnoreBadCodePackages
                        && testPackagesData.ContainsZeroCodePackage
                    )
                    {
                        continue;
                    }

                    int ueCount = this._counterSettings.CalculatePackagesWithMaxUe ? maxUe : testPackagesData.UePackagesCountWithoutIgnored;
                    int packagesCountWithoutUeAndAl =
                        this._counterSettings.IgnoreUserIdentificationPackages
                        ? testPackagesData.PackagesCountWithoutUeAndAl - testPackagesData.AttemptPackagesCount
                        : testPackagesData.PackagesCountWithoutUeAndAl;
                    int platformCount = packagesCountWithoutUeAndAl + testPackagesData.AlPackagesCountWithoutIgnored + ueCount;

                    maxCount = platformCount != 999 && platformCount > maxCount ? platformCount : maxCount;
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
            Dictionary<string, TestPackagesData> testPackagesByPlatform,
            ParamsNulls testData,
            int ueCount
        )
        {
            if (!testPackagesByPlatform.Keys.Any(e => this.IsPlatformValid(this._patternsDictionary[platform], e)))
            {
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

                IEnumerable<Test> existingResultTests = 
                    testSuite.Tests.Where(e => e.Params is ParamsNulls paramsNulls && paramsNulls.DefaultPackagesCount != 999);

                foreach (var test in testSuite.Tests)
                {
                    string testName = test.Name;
                    string fullTestname = string.Concat(testSuiteName, "_", testName).ToUpper();

                    if (test.Params == null)
                    {
                        continue;
                    }

                    ParamsNulls testData = (ParamsNulls)test.Params;

                    int? defaultPackagesCount = testData.DefaultPackagesCount;

                    int maxUeCount = this.CountMaxUeAmongPlatforms(fullTestname);

                    if (!existingResultTests.Any(e => e.Name.Equals(testName, StringComparison.OrdinalIgnoreCase)))
                    {
                        //TODO: bullshit! Needs to search key in other collection!
                        if (this._counterSettings.FillMissingTestPackagesCount)
                        {
                            testData.DefaultPackagesCount = 999;
                            testData.PlatformPackagesCount = null;
                            test.Params = testData;
                        }

                        continue;
                    }

                    if (this.IsAllPackagesCountsAreEqualsDefault(this._packagesDictionary[fullTestname], maxUeCount, defaultPackagesCount))
                    {
                        testData.DefaultPackagesCount = defaultPackagesCount;
                        testData.PlatformPackagesCount = null;
                        test.Params = testData;

                        continue;
                    }

                    Dictionary<string, TestPackagesData> testDataByPlatform = this._packagesDictionary[fullTestname];

                    PlatformPackagesCount platformPackages = new PlatformPackagesCount
                    {
                        AndroidPackages = this.GetPlatformPackagesCount(
                            Platforms.Android, testDataByPlatform, testData, maxUeCount
                        ),
                        IosPackages = this.GetPlatformPackagesCount(
                            Platforms.IOS, testDataByPlatform, testData, maxUeCount
                        ),
                        MacOsPackages = this.GetPlatformPackagesCount(
                            Platforms.MacOS, testDataByPlatform, testData, maxUeCount
                        ),
                        UwpPackages = this.GetPlatformPackagesCount(
                            Platforms.Uwp, testDataByPlatform, testData, maxUeCount
                        ),
                        WindowsPackages = this.GetPlatformPackagesCount(
                            Platforms.Windows, testDataByPlatform, testData, maxUeCount
                        ),
                    };

                    int? windowsPackagesCount = platformPackages.WindowsPackages;

                    if (windowsPackagesCount == null)
                    {
                        windowsPackagesCount = this.FindMinPackagesCount(platformPackages);
                    }

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
                        testData.DefaultPackagesCount = this.FindMinPackagesCount(platformPackages);
                    }
                    else
                    {
                        testData.DefaultPackagesCount = (int)windowsPackagesCount;
                    }

                    test.Params = testData;
                }
            }
        }

        private int FindMinPackagesCount(PlatformPackagesCount platformPackages)
        {
            int minCount = int.MaxValue;

            if (platformPackages.AndroidPackages < minCount && platformPackages.AndroidPackages != 0 && platformPackages.AndroidPackages != null)
            {
                minCount = (int)platformPackages.AndroidPackages;
            }
            if (platformPackages.IosPackages < minCount && platformPackages.IosPackages != 0 && platformPackages.IosPackages != null)
            {
                minCount = (int)platformPackages.IosPackages;
            }
            if (platformPackages.MacOsPackages < minCount && platformPackages.MacOsPackages != 0 && platformPackages.MacOsPackages != null)
            {
                minCount = (int)platformPackages.MacOsPackages;
            }
            if (platformPackages.UwpPackages < minCount && platformPackages.UwpPackages != 0 && platformPackages.UwpPackages != null)
            {
                minCount = (int)platformPackages.UwpPackages;
            }

            return minCount;
        }
    }
}
