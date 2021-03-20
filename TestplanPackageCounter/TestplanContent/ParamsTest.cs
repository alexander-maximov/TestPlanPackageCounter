namespace TestplanPackageCounter.TestplanContent
{
    using Newtonsoft.Json;
    using System.ComponentModel;

    internal class ParamsTest : ParamslessTest
    {
        [JsonProperty("Params")]
        [DefaultValue(null)]
        public Params Params { get; private set; }
    }
}
