namespace TestplanPackageCounter.TestplanContent
{
    using Newtonsoft.Json;
    using System.ComponentModel;

    internal class Test
    {
        [JsonProperty("Name", Order = 0)]
        [JsonRequired]
        public string Name { get; private set; }

        [JsonProperty("Active", Order = 1)]
        [JsonRequired]
        public bool Active { get; private set; }

        [JsonProperty("Params", Order = 2)]
        [DefaultValue(null)]
        public Params Params { get; set; }
    }
}
