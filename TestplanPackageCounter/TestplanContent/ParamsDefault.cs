using Newtonsoft.Json;
using System.ComponentModel;

namespace TestplanPackageCounter.TestplanContent
{
    internal class ParamsDefault : Params
    {
        [JsonProperty("AnalyticsInitMode", Order = 0)]
        [DefaultValue(Constants.AnalyticsInitMode)]
        public string AnalyticsInitMode { get; private set; }

        [JsonProperty("AppProject", Order = 1)]
        [DefaultValue(Constants.AppProject)]
        public string AppProject { get; private set; }

        [JsonProperty("ChangeUserID", Order = 2)]
        [DefaultValue(Constants.Change)]
        public string ChangeUserID { get; private set; }

        [JsonProperty("CleaningMode", Order = 3)]
        [DefaultValue(Constants.None)]
        public string CleaningMode { get; private set; }        

        [JsonProperty("MethodAfterTest", Order = 4)]
        [DefaultValue(null)]
        public string MethodAfterTest { get; private set; }

        [JsonProperty("RestartMode", Order = 5)]
        [DefaultValue(Constants.None)]
        public string RestartMode { get; private set; }

        [JsonProperty("RestartTimeout", Order = 6)]
        [DefaultValue(Constants.IntegerZero)]
        public int? RestartTimeout { get; private set; }

        [JsonProperty("ReverseAdvertisment", Order = 7)]
        [DefaultValue(Constants.NotChange)]
        public string ReverseAdvertisment { get; private set; }

        [JsonProperty("DefaultPackagesCount", Order = 8)]
        [JsonRequired]
        public int DefaultPackagesCount { get; private set; }

        [JsonProperty("PlatformPackagesCount", Order = 9)]
        [DefaultValue(null)]
        public PlatformPackages PlatformPackagesCount { get; private set; }

        [JsonProperty("TimeOut", Order = 10)]
        [DefaultValue(null)]
        public int? TimeOut { get; private set; }
    }
}
