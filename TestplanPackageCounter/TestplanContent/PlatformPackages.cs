namespace TestplanPackageCounter.TestplanContent
{
    using Newtonsoft.Json;
    using System.ComponentModel;

    internal class PlatformPackages
    {
        [JsonProperty("Android")]
        [DefaultValue(Constants.NonExistString)]
        public int? Android { get; private set; }

        [JsonProperty("IOS")]
        [DefaultValue(Constants.NonExistString)]
        public int? Ios { get; private set; }

        [JsonProperty("MacOS")]
        [DefaultValue(Constants.NonExistString)]
        public int? MacOS { get; private set; }

        [JsonProperty("UWP")]
        [DefaultValue(Constants.NonExistString)]
        public int? Uwp { get; private set; }

        [JsonProperty("Windows")]
        [DefaultValue(Constants.NonExistString)]
        public int? Windows { get; private set; }
    }
}
