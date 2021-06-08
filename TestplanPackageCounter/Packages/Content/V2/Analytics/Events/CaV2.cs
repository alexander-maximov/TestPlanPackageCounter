using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public class CaV2 : AbstractSdkEventV2, IHasBasicValues, IHasTimestamp
    {
        [JsonProperty("level")]
        [DefaultValue(null)]
        public uint? Level { get; private set; }

        [JsonProperty("bought")]
        [DefaultValue(null)]
        public Dictionary<string, Dictionary<string, int>> Bought { get; private set; }

        [JsonProperty("earned")]
        [DefaultValue(null)]
        public Dictionary<string, Dictionary<string, int>> Earned { get; private set; }

        [JsonProperty("sessionId")]
        [DefaultValue(null)]
        public ulong? SessionId { get; private set; }

        [JsonProperty("timestamp")]
        [DefaultValue(null)]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("inExperiments")]
        [DefaultValue(null)]
        public IeEntry[] InExperiments { get; private set; }
    }
}
