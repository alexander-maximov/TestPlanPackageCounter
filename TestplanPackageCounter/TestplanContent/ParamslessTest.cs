namespace TestplanPackageCounter.TestplanContent
{
    using Newtonsoft.Json;

    internal class ParamslessTest
    {
        [JsonProperty("Name")]
        [JsonRequired]
        public string Name { get; private set; }

        [JsonProperty("Active")]
        [JsonRequired]
        public bool Active { get; private set; }
    }
}
