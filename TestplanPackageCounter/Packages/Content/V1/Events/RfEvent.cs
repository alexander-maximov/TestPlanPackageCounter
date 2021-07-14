namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;

    using System.ComponentModel;
    using TestplanPackageCounter.General;
    using TestplanPackageCounter.Packages.Content.General;

    public class RfEvent : AbstractSdkEventV1, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("publisher")]
        [DefaultValue(Constants.NotExistsString)]
        public string Publisher { get; private set; }

        [JsonProperty("subpublisher")]
        [DefaultValue(Constants.NotExistsString)]
        public string SubPublisher { get; private set; }

        [JsonProperty("subad")]
        [DefaultValue(Constants.NotExistsString)]
        public string Subad { get; private set; }

        [JsonProperty("subadgroup")]
        [DefaultValue(Constants.NotExistsString)]
        public string SubadGroup { get; private set; }

        [JsonProperty("subcampaign")]
        [DefaultValue(Constants.NotExistsString)]
        public string SubCampaign { get; private set; }

        [JsonProperty("subplacement")]
        [DefaultValue(Constants.NotExistsString)]
        public string SubPlacement { get; private set; }

        [JsonProperty("subkeyword")]
        [DefaultValue(Constants.NotExistsString)]
        public string SubKeyword { get; private set; }

        [JsonProperty("referral")]
        [DefaultValue(Constants.NotExistsString)]
        public string Referral { get; private set; }

        [JsonProperty("name")]
        [DefaultValue(Constants.NotExistsString)]
        public string Name { get; private set; }
    }
}
