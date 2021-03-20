namespace TestplanPackageCounter.TestplanContent
{
    using Newtonsoft.Json;
    using System.ComponentModel;

    internal class Params
    {
        [JsonProperty("AppProject")]
        [DefaultValue(Constants.NonExistString)]
        public string AppProject { get; private set; }

        [JsonProperty("CleaningMode")]
        [DefaultValue(Constants.NonExistString)]
        public string CleaningMode { get; private set; }

        [JsonProperty("RestartMode")]
        [DefaultValue(Constants.NonExistString)]
        public string RestartMode { get; private set; }

        [JsonProperty("AnalyticsInitMode")]
        [DefaultValue(Constants.NonExistString)]
        public string AnalyticsInitMode { get; private set; }

        [JsonProperty("MethodAfterTest")]
        [DefaultValue(Constants.NonExistString)]
        public string MethodAfterTest { get; private set; }

        [JsonProperty("PlatformPackagesCount")]
        [DefaultValue(Constants.NonExistString)]
        public PlatformPackages PlatformPackagesCount { get; private set; }

        [JsonProperty("RestartTimeout")]
        [DefaultValue(Constants.NonExistString)]
        public string RestartTimeout { get; private set; }

        [JsonProperty("ReverseAdvertisment")]
        [DefaultValue(Constants.NonExistString)]
        public string ReverseAdvertisment { get; private set; }

        [JsonProperty("TimeOut")]
        [DefaultValue(Constants.NonExistString)]
        public string TimeOut { get; private set; }

        [JsonProperty("DefaultPackagesCount")]
        [JsonRequired]
        public int DefaultPackagesCount { get; private set; }
    }
}
