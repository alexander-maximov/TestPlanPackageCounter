namespace TestplanPackageCounter
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.IO;
    using TestplanPackageCounter.Counter;
    using TestplanPackageCounter.General;
    using TestplanPackageCounter.Testplan.Content;
    using TestplanPackageCounter.Testplan.Converters;
    using TestplanPackageCounter.UglyCode;
    using TestplanPackageCounter.UglyCode.PackagesEnumerator;

    class Program
    {
        static void Main(string[] args)
        {
            CounterSettings counterSettings = new CounterSettings(
                pathToTestplan: @"C:\Users\at\Documents\Backup\testplanV2.json",
                outcomingPath: @"C:\Users\at\Documents\Backup\testplan_editedV2.json",
                pathToResults: @"C:\Users\at\Downloads\BigData",
                sdkVersion: SdkVersions.V2,
                rewriteTestplan: true,
                ignoreUePackages: true,
                ignoreLastAl: true,
                ignoreLastUe: true,
                ignoreUserIdentification: true,
                calculateWithMaxUe: true,
                fillWithDefaultParams: false,
                writeToCsv: true,
                sortOnly: false
            );

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

            if (counterSettings.SortOnly)
            {
                string serializedJson =
                    JsonConvert.SerializeObject(testSuites, Formatting.Indented, serializerSettings);

                File.WriteAllText(counterSettings.OutcomingPath, serializedJson);

                return;
            }
            
            Dictionary<string, bool> testBeforeCleanDictionary = GetToKnowCleaningTest(testSuites);

            CommonEnumerator commonEnumerator;

            if (counterSettings.SdkVersion == SdkVersions.V1)
            {
                commonEnumerator = new PackagesEnumeratorV1(counterSettings, testBeforeCleanDictionary);
            }
            else
            {
                commonEnumerator = new PackagesEnumeratorV2(counterSettings, testBeforeCleanDictionary);
            }

            commonEnumerator.Enumerate();

            if (counterSettings.RewriteTestplan)
            {
                TestPlanEditor testSuiteEditor = new TestPlanEditor(
                        testSuites,
                        commonEnumerator.PackagesStatusDictionary,
                        counterSettings
                    );
                testSuiteEditor.EditTestPlan();

                string serializedJson =
                    JsonConvert.SerializeObject(testSuites, Formatting.Indented, serializerSettings);

                File.WriteAllText(counterSettings.OutcomingPath, serializedJson);
            }

            if (counterSettings.WriteToCsv)
            {
                ReportGenerator reportGenerator = new ReportGenerator(
                    counterSettings,
                    commonEnumerator.PackagesStatusDictionary,
                    GetPlatformList(counterSettings.PathToResults)
                );

                reportGenerator.WriteToCsv();
            }
        }

        private static Dictionary<string, bool> GetToKnowCleaningTest(List<TestSuite> testSuites)
        {
            Dictionary<string, bool> previousTestIsCleanDictionary = new Dictionary<string, bool>();

            bool aliveSuitePlug = true;

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

            if (aliveSuitePlug)
            {
                previousTestIsCleanDictionary["ALIVESUITE_ALIVEWITHTIMEOUT"] = aliveSuitePlug;
            }

            return previousTestIsCleanDictionary;
        }

        /// <summary>
        /// Get list of platforms from results.
        /// </summary>
        /// <param name="resultsPath">Path to results to generate list of platforms based on folder names.</param>
        /// <returns>List of platforms.</returns>
        private static List<string> GetPlatformList(string resultsPath)
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
