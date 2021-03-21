namespace TestplanPackageCounter
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.IO;
    using TestplanPackageCounter.Converters;
    using TestplanPackageCounter.TestplanContent;

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
                fillDefaultParams: false
            );

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings { };

            serializerSettings.Converters.Add(new TestConverter());

            string testplanContent = File.ReadAllText(counterSettings.PathToTestplan);

            List<TestSuite> testSuites = 
                JsonConvert.DeserializeObject<List<TestSuite>>(testplanContent, serializerSettings);

            string serializedJson = JsonConvert.SerializeObject(testSuites, Formatting.Indented);

            File.WriteAllText(counterSettings.OutcomingPath, serializedJson);

            //TODO: write without null fields and [NonExistString]
            //TODO: count without Ue
            //TODO: count without Al
            //TODO: count with max Ue among all tests.
            //TODO: write to csv
            //TODO: determine by name what is android, what is IOS etc.
            //TODO: fillWithParams converter.
        }
    }
}
