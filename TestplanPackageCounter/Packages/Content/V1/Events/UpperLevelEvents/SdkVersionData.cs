namespace TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents
{
    using System.ComponentModel;
    using Newtonsoft.Json;
    using TestplanPackageCounter.General;

    public class SdkVersionData : RequestJsonContainerV1
    {
        [JsonProperty("timestamp")]
        [DefaultValue(null)]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("sdkVersion")]
        [DefaultValue(Constants.NotExistsString)]
        public string Version { get; private set; }

        [JsonProperty("appVersion")]
        [DefaultValue(Constants.NotExistsString)]
        public string AppVersion { get; private set; }

        [JsonProperty("configVersion")]
        [DefaultValue(Constants.NotExistsString)]
        public string ConfigVersion { get; private set; }

        [JsonProperty("locale")]
        [DefaultValue(Constants.NotExistsString)]
        public string Locale { get; private set; }

        [JsonProperty("pushCategoriesTimestamp ")]
        [DefaultValue(null)]
        public ulong? PushCategoriesTimestamp { get; private set; }
    }
}
