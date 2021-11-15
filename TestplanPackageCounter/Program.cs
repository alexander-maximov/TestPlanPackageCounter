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

            string pathToResults = @"C:\Users\at\Documents\devtodev\TestResults\205 run\Full";
            //TODO: pick default values from testplan option, otherwise 999
            //TODO: null packages removal for v2
            CounterSettings.PathToTestplan = Path.Combine(pathToResults, "testplan.json");
            CounterSettings.OutcomingPath = Path.Combine(pathToResults, "testplanV2edited.json");
            CounterSettings.PathToResults = pathToResults;
            CounterSettings.SdkVersion = SdkVersions.V2;
            CounterSettings.IgnoreLastAl = true;
            CounterSettings.FillMissingTestPackagesCount = true;
            CounterSettings.CalculatePackagesWithMaxUe = false;
            CounterSettings.IgnoreBadUe = true;


            JsonSerializerSettings serializerSettings = new()
            {
                DefaultValueHandling = DefaultValueHandling.Populate,
                NullValueHandling = NullValueHandling.Ignore
            };

            serializerSettings.Converters.Add(
                new ParamsConverter(CounterSettings.FillWithDefaultParams)
            );

            string testplanContent = File.ReadAllText(CounterSettings.PathToTestplan);

            List<TestSuite> testSuites =
                    JsonConvert.DeserializeObject<List<TestSuite>>(testplanContent, serializerSettings);

            if (CounterSettings.SortOnly)
            {
                string serializedJson =
                    JsonConvert.SerializeObject(testSuites, Formatting.Indented, serializerSettings);

                File.WriteAllText(CounterSettings.OutcomingPath, serializedJson);

                return;
            }

            CommonEnumerator commonEnumerator;

            if (CounterSettings.SdkVersion == SdkVersions.V1)
            {
                commonEnumerator = new PackagesEnumeratorV1(testSuites);
            }
            else
            {
                commonEnumerator = new PackagesEnumeratorV2(testSuites);
            }

            commonEnumerator.Enumerate();

            if (CounterSettings.RewriteTestplan)
            {
                TestPlanEditor testSuiteEditor = new(
                        testSuites,
                        commonEnumerator.PackagesStatusDictionary
                    );

                testSuiteEditor.EditTestPlan();

                string serializedJson =
                    JsonConvert.SerializeObject(testSuites, Formatting.Indented, serializerSettings);

                File.WriteAllText(CounterSettings.OutcomingPath, serializedJson);
            }

            if (CounterSettings.WriteToCsv)
            {
                ReportGenerator reportGenerator = new(
                    packagesDictionary: commonEnumerator.PackagesStatusDictionary,
                    platformList: GetPlatformList(CounterSettings.PathToResults),
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
            List<string> platformList = new();

            foreach (string directory in Directory.GetDirectories(resultsPath))
            {
                string platformName = Path.GetFileName(directory);

                platformList.Add(platformName);
            }

            return platformList;
        }
    }
}
