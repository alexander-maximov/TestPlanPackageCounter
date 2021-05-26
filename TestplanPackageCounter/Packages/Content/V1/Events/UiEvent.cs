namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;
    using System.ComponentModel;
    using TestplanPackageCounter.General;
    using TestplanPackageCounter.Packages.Content.General;

    public class UiEvent : AbstractSdkEvent, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("locale")]
        [JsonRequired]
        public string Locale { get; private set; }

        [JsonProperty("crossUid")]
        [JsonRequired]
        public string CrossUid { get; private set; }

        [JsonProperty("carrier")]
        [DefaultValue(Constants.NotExistsString)]
        public string Carrier { get; private set; }

        [JsonProperty("isRooted")]
        [DefaultValue(null)]
        public int? IsRooted { get; private set; }

        [JsonProperty("userAgent")]
        [DefaultValue(Constants.NotExistsString)]
        public string UserAgent { get; private set; }

        [JsonProperty("registered")]
        [DefaultValue(null)]
        public ulong? Registered { get; private set; }
    }
}
