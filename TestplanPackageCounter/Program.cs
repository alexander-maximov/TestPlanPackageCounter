namespace TestplanPackageCounter
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
                pathToResults: @"C:\Users\at\Downloads\mergeResults\results",
                sdkVersion: SdkVersions.V1,
                rewriteTestplan: true,
                ignoreUePackages: true,
                ignoreLastUe: true,
                calculateWithMaxUe: true,
                fillWithDefaultParams: false,
                writeToCsv: true
            );

            Console.WriteLine("Settings loaded.");
            Console.Write("Calculating packages count...");

            PackagesEnumerator packagesEnumerator = new PackagesEnumerator(counterSettings);
            packagesEnumerator.Enumerate();

            Console.WriteLine("Done.");

            Dictionary<string, Dictionary<string, int>> packagesDictionary =
                packagesEnumerator.PackagesDictionary;

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings 
            {
                DefaultValueHandling = DefaultValueHandling.Populate,
                NullValueHandling = NullValueHandling.Ignore
            };

            serializerSettings.Converters.Add(
                new ParamsConverter(counterSettings.FillWithDefaultParams)
            );

            string testplanContent = File.ReadAllText(counterSettings.PathToTestplan);

            if (counterSettings.RewriteTestplan)
            {
                List<TestSuite> testSuites =
                    JsonConvert.DeserializeObject<List<TestSuite>>(testplanContent, serializerSettings);

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
            //TODO: write to csv
            //TODO: determine by name what is android, what is IOS etc.
        }
    }
}
