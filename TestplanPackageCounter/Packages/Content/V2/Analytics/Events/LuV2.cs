using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public class LuV2 : AbstractSdkEventV2, IHasBasicValues, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [DefaultValue(null)]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("level")]
        [DefaultValue(null)]
        public int? Level { get; set; }

        [JsonProperty("balance")]
        [DefaultValue(null)]
        public Dictionary<string, long> Balance { get; private set; }

        [JsonProperty("spent")]
        [DefaultValue(null)]
        public Dictionary<string, int> Spent { get; private set; }

        [JsonProperty("earned")]
        [DefaultValue(null)]
        public Dictionary<string, int> Earned { get; private set; }

        [JsonProperty("bought")]
        [DefaultValue(null)]
        public Dictionary<string, int> Bought { get; private set; }

        [JsonProperty("sessionId")]
        [DefaultValue(null)]
        public ulong? SessionId { get; private set; }

        [JsonProperty("inExperiments")]
        [DefaultValue(null)]
        public IeEntry[] InExperiments { get; private set; }
    }
}
