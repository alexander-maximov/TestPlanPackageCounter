namespace TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents
{
    using Newtonsoft.Json;
    using System.ComponentModel;
    using TestplanPackageCounter.General;
    using TestplanPackageCounter.Packages.Content.General;

    public class ResponseJsonDataV1 : AbstractData
    {
        [JsonProperty("worker")]
        [DefaultValue(Constants.NotExistsString)]
        public string Worker { get; private set; }

        [JsonProperty("timeForRequest")]
        [DefaultValue(null)]
        public int? TimeForRequest { get; private set; }

        [JsonProperty("countForRequest")]
        [DefaultValue(null)]
        public int? CountForRequest { get; private set; }

        [JsonProperty("sessionDelay")]
        [DefaultValue(null)]
        public int? SessionDelay { get; private set; }

        [JsonProperty("sessionTimeout")]
        [DefaultValue(null)]
        public int? SessionTimeout { get; private set; }

        [JsonProperty("eventParamsCount")]
        [DefaultValue(null)]
        public int? EventParamsCount { get; private set; }

        [JsonProperty("aliveTimeout")]
        [DefaultValue(null)]
        public int? AliveTimeout { get; private set; }

        [JsonProperty("serverTime")]
        [DefaultValue(null)]
        public int? ServerTime { get; private set; }

        [JsonProperty("result")]
        [DefaultValue(null)]
        public int? Result { get; private set; }

        [JsonProperty("configVersion")]
        [DefaultValue(Constants.NotExistsString)]
        public string ConfigVersion { get; private set; }

        [JsonProperty("useCustomUDID")]
        [DefaultValue(null)]
        public bool? UseCustomUdid { get; private set; }
    }
}
