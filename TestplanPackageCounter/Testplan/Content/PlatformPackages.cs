namespace TestplanPackageCounter.Testplan.Content
{
    using Newtonsoft.Json;
    using System.ComponentModel;

    internal class PlatformPackages
    {
        [JsonProperty("Android")]
        [DefaultValue(null)]
        public int? Android { get; set; }

        [JsonProperty("IOS")]
        [DefaultValue(null)]
        public int? Ios { get; set; }

        [JsonProperty("MacOS")]
        [DefaultValue(null)]
        public int? MacOS { get; set; }

        [JsonProperty("UWP")]
        [DefaultValue(null)]
        public int? Uwp { get; set; }

        [JsonProperty("Windows")]
        [DefaultValue(null)]
        public int? Windows { get; set; }
    }
}
