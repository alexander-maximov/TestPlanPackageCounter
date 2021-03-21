using Newtonsoft.Json;
using System.ComponentModel;

namespace TestplanPackageCounter.TestplanContent
{
    internal class ParamsDefault
    {
        [JsonProperty("AppProject")]
        [DefaultValue(Constants.AppProject)]
        public string AppProject { get; private set; }

        [JsonProperty("CleaningMode")]
        [DefaultValue(Constants.CleaningMode)]
        public string CleaningMode { get; private set; }

        [JsonProperty("RestartMode")]
        [DefaultValue(Constants.RestartMode)]
        public string RestartMode { get; private set; }

        [JsonProperty("AnalyticsInitMode")]
        [DefaultValue(Constants.AnalyticsInitMode)]
        public string AnalyticsInitMode { get; private set; }

        [JsonProperty("MethodAfterTest")]
        [DefaultValue(null)]
        public string MethodAfterTest { get; private set; }

        [JsonProperty("PlatformPackagesCount")]
        [DefaultValue(null)]
        public PlatformPackages PlatformPackagesCount { get; private set; }

        [JsonProperty("RestartTimeout")]
        [DefaultValue(Constants.RestartTimeOut)]
        public int RestartTimeout { get; private set; }

        [JsonProperty("ReverseAdvertisment")]
        [DefaultValue(Constants.ReverseAdvertisment)]
        public string ReverseAdvertisment { get; private set; }

        [JsonProperty("TimeOut")]
        [DefaultValue(null)]
        public int? TimeOut { get; private set; }

        [JsonProperty("DefaultPackagesCount")]
        [DefaultValue(Constants.DefaultPackagesCount)]
        public int DefaultPackagesCount { get; private set; }
    }
}
