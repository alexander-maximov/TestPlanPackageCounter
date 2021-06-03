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
                pathToTestplan: @"C:\Users\at\Documents\Backup\testplan.json",
                outcomingPath: @"C:\Users\at\Documents\Backup\testplan_edited.json",
                pathToResults: @"C:\Users\at\Downloads\BigData",
                sdkVersion: SdkVersions.V2,
                rewriteTestplan: true,
                ignoreUePackages: true,
                ignoreLastUe: true,
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
            Dictionary<string, Dictionary<string, int>> packagesDictionary = new Dictionary<string, Dictionary<string, int>>();

            //PLUG!
            Dictionary<string, int> MaxUeDictionary = new Dictionary<string, int>();
            //PLUG!

            if (counterSettings.SdkVersion == SdkVersions.V1)
            {
                PackagesEnumeratorV1 packagesEnumerator = new PackagesEnumeratorV1(counterSettings.PathToResults);
                packagesEnumerator.Enumerate(testBeforeCleanDictionary);
                packagesDictionary = packagesEnumerator.PackagesDictionary;
                //PLUG!
                MaxUeDictionary = packagesEnumerator.MaxUeDictionary;
            }
            else
            {
                PackagesEnumeratorV2 packagesEnumerator = new PackagesEnumeratorV2(counterSettings.PathToResults);
                packagesEnumerator.Enumerate(testBeforeCleanDictionary);
                packagesDictionary = packagesEnumerator.PackagesDictionary;

                MaxUeDictionary = packagesEnumerator.MaxUeDictionary;
            }

            if (counterSettings.RewriteTestplan)
            {
                TestPlanEditor testSuiteEditor = new TestPlanEditor(
                        testSuites,
                        packagesDictionary,
                        MaxUeDictionary
                    );
                testSuiteEditor.EditTestPlan();

                string serializedJson =
                    JsonConvert.SerializeObject(testSuites, Formatting.Indented, serializerSettings);

                File.WriteAllText(counterSettings.OutcomingPath, serializedJson);
            }

            if (counterSettings.WriteToCsv)
            {
                /*
                packagesEnumerator.WriteToCsv();
                packagesEnumerator.CheckMaxUe();
                */
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
    }
}
