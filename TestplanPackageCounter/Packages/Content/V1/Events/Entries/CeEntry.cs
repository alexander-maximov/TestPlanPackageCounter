namespace TestplanPackageCounter.Packages.Content.V1.Events.Entries
{
    using Newtonsoft.Json;
    using System.ComponentModel;

    public class CeEntry : AbstractSdkEvent
    {
        [JsonProperty("t1")]
        [JsonRequired]
        public ulong Timestamp { get; private set; }

        [JsonProperty("sessionId")]
        [JsonRequired]
        public ulong SessionId { get; private set; }

        [JsonProperty("p")]
        [DefaultValue(null)]
        public CeEntryParam Params { get; private set; }
    }
}
