namespace TestplanPackageCounter.TestplanContent
{
    using Newtonsoft.Json;
    using System.ComponentModel;

    internal class PlatformPackages
    {
        [JsonProperty("Android")]
        [DefaultValue(null)]
        public int? Android { get; private set; }

        [JsonProperty("IOS")]
        [DefaultValue(null)]
        public int? Ios { get; private set; }

        [JsonProperty("MacOS")]
        [DefaultValue(null)]
        public int? MacOS { get; private set; }

        [JsonProperty("UWP")]
        [DefaultValue(null)]
        public int? Uwp { get; private set; }

        [JsonProperty("Windows")]
        [DefaultValue(null)]
        public int? Windows { get; private set; }
    }
}
