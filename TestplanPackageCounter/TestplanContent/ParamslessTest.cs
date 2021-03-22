namespace TestplanPackageCounter.TestplanContent
{
    using Newtonsoft.Json;

    internal class ParamslessTest
    {
        [JsonProperty("Name", Order = 0)]
        [JsonRequired]
        public string Name { get; private set; }

        [JsonProperty("Active", Order = 1)]
        [JsonRequired]
        public bool Active { get; private set; }
    }
}
