namespace TestplanPackageCounter
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
            Stopwatch watch = Stopwatch.StartNew();

            //TODO: pick default values from testplan option, otherwise 999
            //TODO: null packages removal for v2
            CounterSettings counterSettings = new CounterSettings(
                pathToTestplan: @"C:\Users\at\Downloads\AndroidAndIosOnly\testplan.json",
                outcomingPath: @"C:\Users\at\Downloads\AndroidAndIosOnly\testplan_edited.json",
                pathToResults: @"C:\Users\at\Downloads\AndroidAndIosOnly",
                sdkVersion: SdkVersions.V2,
                ignoreLastAl: true,
                fillMissingTestPackagesCount: true,
                calculateWithMaxUe: false
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

            CommonEnumerator commonEnumerator;

            if (counterSettings.SdkVersion == SdkVersions.V1)
            {
                commonEnumerator = new PackagesEnumeratorV1(counterSettings, testSuites);
            }
            else
            {
                commonEnumerator = new PackagesEnumeratorV2(counterSettings, testSuites);
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
                    counterSettings: counterSettings,
                    packagesDictionary: commonEnumerator.PackagesStatusDictionary,
                    platformList: GetPlatformList(counterSettings.PathToResults),
                    testSuites: testSuites
                );

                reportGenerator.WriteToCsv();
                reportGenerator.EasyTestplanReport();
                reportGenerator.OverwhelmingOne();
            }

            watch.Stop();

            long elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Elapsed time: {elapsedMs} ms");
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
