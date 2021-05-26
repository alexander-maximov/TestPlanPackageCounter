namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;
    using System.ComponentModel;
    using TestplanPackageCounter.General;
    using TestplanPackageCounter.Packages.Content.General;

    public class AiEvent : AbstractSdkEvent, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("sdkVersion")]
        [JsonRequired]
        public string SdkVersion { get; private set; }

        [JsonProperty("engine")]
        [DefaultValue(Constants.NotExistsString)]
        public string Engine { get; private set; }

        [JsonProperty("appVersion")]
        [DefaultValue(Constants.NotExistsString)]
        public string AppVersion { get; private set; }

        [JsonProperty("codeVersion")]
        [DefaultValue(null)]
        public int? CodeVersion { get; private set; }

        [JsonProperty("isSandbox")]
        [DefaultValue(null)]
        public bool? IsSandbox { get; private set; }

        [JsonProperty("bundleId")]
        [DefaultValue(Constants.NotExistsString)]
        public string BundleId { get; private set; }
    }
}
