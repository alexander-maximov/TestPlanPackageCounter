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
            CounterSettings settings = new CounterSettings(
                pathToTestplan: @"C:\Users\at\Documents\Backup\testplan.json",
                pathToResults: @"null",
                rewriteTestplan: false,
                ignoreUePackages: false,
                ignoreAlPackages: false,
                calculateWithMaxUe: false
            );

            JsonSerializerSettings _settings = new JsonSerializerSettings { };

            _settings.Converters.Add(new TestConverter());

            string testplanContent = File.ReadAllText(settings.PathToTestplan);

            List<TestSuite> testSuites = new List<TestSuite>();

            testSuites = JsonConvert.DeserializeObject<List<TestSuite>>(testplanContent, _settings);

            //TODO: write to file
            //TODO: write without null fields and [NonExistString]
            //TODO: count without Ue
            //TODO: count without Al
            //TODO: count with max Ue among all tests.
            //TODO: write to csv
            //TODO: determine by name what is android, what is IOS etc.
        }
    }
}
