using Newtonsoft.Json;
using System.ComponentModel;

namespace TestplanPackageCounter.TestplanContent
{
    internal class ParamsNulls : Params
    {
        [JsonProperty("AnalyticsInitMode", Order = 0)]
        [DefaultValue(null)]
        public string AnalyticsInitMode { get; private set; }

        [JsonProperty("AppProject", Order = 1)]
        [DefaultValue(null)]
        public string AppProject { get; private set; }

        [JsonProperty("ChangeUserID", Order = 2)]
        [DefaultValue(null)]
        public string ChangeUserID { get; private set; }

        [JsonProperty("CleaningMode", Order = 3)]
        [DefaultValue(null)]
        public string CleaningMode { get; private set; }

        [JsonProperty("MethodAfterTest", Order = 4)]
        [DefaultValue(null)]
        public string MethodAfterTest { get; private set; }

        [JsonProperty("RestartMode", Order = 5)]
        [DefaultValue(null)]
        public string RestartMode { get; private set; }

        [JsonProperty("RestartTimeout", Order = 6)]
        [DefaultValue(null)]
        public int? RestartTimeout { get; private set; }

        [JsonProperty("ReverseAdvertisment", Order = 7)]
        [DefaultValue(null)]
        public string ReverseAdvertisment { get; private set; }        

        [JsonProperty("DefaultPackagesCount", Order = 8)]
        [JsonRequired]
        public int DefaultPackagesCount { get; set; }

        [JsonProperty("PlatformPackagesCount", Order = 9)]
        [DefaultValue(null)]
        public PlatformPackages PlatformPackagesCount { get; set; }

        [JsonProperty("TimeOut", Order = 10)]
        [DefaultValue(null)]
        public int? TimeOut { get; private set; }
    }
}
