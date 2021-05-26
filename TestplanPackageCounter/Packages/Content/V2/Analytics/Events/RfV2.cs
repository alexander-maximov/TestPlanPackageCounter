using Newtonsoft.Json;
using System.ComponentModel;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public class RfV2 : AbstractSdkEventV2, IHasBasicValues, IHasTimestamp, IHasCodeValue
    {
        [JsonProperty("source")]
        [DefaultValue(Constants.NotExistsString)]
        public string Source { get; private set; }

        [JsonProperty("campaign")]
        [DefaultValue(Constants.NotExistsString)]
        public string Campaign { get; private set; }

        [JsonProperty("content")]
        [DefaultValue(Constants.NotExistsString)]
        public string Content { get; private set; }

        [JsonProperty("medium")]
        [DefaultValue(Constants.NotExistsString)]
        public string Medium { get; private set; }

        [JsonProperty("term")]
        [DefaultValue(Constants.NotExistsString)]
        public string Term { get; private set; }

        [JsonProperty("sessionId")]
        [DefaultValue(null)]
        public ulong? SessionId { get; private set; }

        [JsonProperty("timestamp")]
        [DefaultValue(null)]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("inExperiments")]
        [DefaultValue(null)]
        public IeEntry[] InExperiments { get; private set; }

        [JsonProperty("code")]
        [DefaultValue(Constants.NotExistsString)]
        public string Code { get; private set; }
    }
}
