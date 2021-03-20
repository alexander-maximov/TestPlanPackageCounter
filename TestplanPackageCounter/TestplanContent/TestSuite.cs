namespace TestplanPackageCounter.TestplanContent
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    internal class TestSuite
    {
        [JsonProperty("Name")]
        [JsonRequired]
        public string Name { get; private set; }

        [JsonProperty("Active")]
        [JsonRequired]
        public bool Active { get; private set; }

        [JsonProperty("Tests")]
        [JsonRequired]
        public List<ParamslessTest> Tests { get; private set; }
    }
}
