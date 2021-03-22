namespace TestplanPackageCounter
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.IO;
    using TestplanPackageCounter.Converters;
    using TestplanPackageCounter.TestplanContent;
    using TestplanPackageCounter.UglyCode;

    class Program
    {
        static void Main(string[] args)
        {
            CounterSettings counterSettings = new CounterSettings(
                pathToTestplan: @"C:\Users\at\Documents\Backup\testplan.json",
                outcomingPath: @"C:\Users\at\Documents\Backup\testplan_edited.json",
                pathToResults: @"null",
                rewriteTestplan: false,
                ignoreUePackages: false,
                ignoreAlPackages: false,
                calculateWithMaxUe: false,
                fillWithDefaultParams: false
            );

            PackagesEnumerator packagesEnumerator = new PackagesEnumerator();
            packagesEnumerator.Enumerate();

            Dictionary<string, Dictionary<string, string>> packagesDictionary =
                packagesEnumerator.PackagesDictionary;

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings 
            {
                DefaultValueHandling = DefaultValueHandling.Populate,
                NullValueHandling = NullValueHandling.Ignore
            };

            serializerSettings.Converters.Add(new ParamsConverter(counterSettings.FillWithDefaultParams));

            string testplanContent = File.ReadAllText(counterSettings.PathToTestplan);

            List<TestSuite> testSuites = 
                JsonConvert.DeserializeObject<List<TestSuite>>(testplanContent, serializerSettings);

            TestSuiteEditor testSuiteEditor = 
                new TestSuiteEditor(testSuites, packagesDictionary, packagesEnumerator.MaxUeDictionary);
            testSuiteEditor.EditTestSuites();

            string serializedJson = JsonConvert.SerializeObject(testSuites, Formatting.Indented, serializerSettings);

            File.WriteAllText(counterSettings.OutcomingPath, serializedJson);

            //TODO: count without Al
            //TODO: write to csv
            //TODO: determine by name what is android, what is IOS etc.
        }
    }
}
