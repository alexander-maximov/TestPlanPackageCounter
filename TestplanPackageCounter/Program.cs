﻿namespace TestplanPackageCounter
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using TestplanPackageCounter.Counter;
    using TestplanPackageCounter.General;
    using TestplanPackageCounter.Testplan.Content;
    using TestplanPackageCounter.Testplan.Converters;
    using TestplanPackageCounter.UglyCode;

    class Program
    {
        static void Main(string[] args)
        {
            CounterSettings counterSettings = new CounterSettings(
                pathToTestplan: @"C:\Users\at\Documents\Backup\testplan.json",
                outcomingPath: @"C:\Users\at\Documents\Backup\testplan_edited.json",
                pathToResults: @"C:\Users\at\Downloads\results (7)",
                sdkVersion: SdkVersions.V1,
                rewriteTestplan: true,
                ignoreUePackages: true,
                ignoreLastUe: true,
                calculateWithMaxUe: true,
                fillWithDefaultParams: false,
                writeToCsv: true
            );

            #region get testplan data.
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Populate,
                NullValueHandling = NullValueHandling.Ignore
            };

            serializerSettings.Converters.Add(
                new ParamsConverter(counterSettings.FillWithDefaultParams)
            );

            string testplanContent = File.ReadAllText(counterSettings.PathToTestplan);

            List<TestSuite> testSuites =
                    JsonConvert.DeserializeObject<List<TestSuite>>(testplanContent, serializerSettings);

            //Fill that dictionary!
            Dictionary<string, bool> testBeforeCleanDictionary = GetToKnowCleaningTest(testSuites);
            #endregion

            PackagesEnumerator packagesEnumerator = new PackagesEnumerator(counterSettings);
            packagesEnumerator.Enumerate(testBeforeCleanDictionary);

            Dictionary<string, Dictionary<string, int>> packagesDictionary =
                packagesEnumerator.PackagesDictionary;

            if (counterSettings.RewriteTestplan)
            {               
                TestPlanEditor testSuiteEditor = new TestPlanEditor(
                    testSuites,
                    packagesDictionary,
                    packagesEnumerator.MaxUeDictionary
                );
                testSuiteEditor.EditTestSuites();

                string serializedJson =
                    JsonConvert.SerializeObject(testSuites, Formatting.Indented, serializerSettings);

                File.WriteAllText(counterSettings.OutcomingPath, serializedJson);
            }

            //TODO: count without Al
            //TODO: determine by name what is android, what is IOS etc.
        }

        private static Dictionary<string, bool> GetToKnowCleaningTest(List<TestSuite> testSuites)
        {
            Dictionary<string, bool> previousTestIsCleanDictionary = new Dictionary<string, bool>();

            string previousTestSuiteName = string.Empty;
            string previousTestName = string.Empty;

            foreach (TestSuite testSuite in testSuites)
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

            return previousTestIsCleanDictionary;
        }
    }
}
