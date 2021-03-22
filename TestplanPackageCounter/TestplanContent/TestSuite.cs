namespace TestplanPackageCounter.TestplanContent
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    internal class TestSuite
    {
        [JsonProperty("Name", Order = 0)]
        [JsonRequired]
        public string Name { get; private set; }

        [JsonProperty("Active", Order = 1)]
        [JsonRequired]
        public bool Active { get; private set; }

        [JsonProperty("Tests", Order = 2)]
        [JsonRequired]
        public List<Test> Tests { get; private set; }
    }
}
