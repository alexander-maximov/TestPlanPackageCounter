namespace TestplanPackageCounter.TestplanContent
{
    using Newtonsoft.Json;
    using System.ComponentModel;

    internal class ParamsTest : ParamslessTest
    {
        [JsonProperty("Params", Order = 2)]
        [DefaultValue(null)]
        public Params Params { get; private set; }
    }
}
