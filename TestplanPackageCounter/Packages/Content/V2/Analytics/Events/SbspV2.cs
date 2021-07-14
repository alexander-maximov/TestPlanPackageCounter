using Newtonsoft.Json;
using System.ComponentModel;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public class SbspV2 : AbstractSdkEventV2, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [DefaultValue(null)]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("products")]
        [DefaultValue(null)]
        public SbspEntry[] Products { get; private set; }
    }
}
