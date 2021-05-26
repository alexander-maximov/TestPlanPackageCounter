namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;
    using TestplanPackageCounter.Packages.Content.General;

    public class TrEvent : AbstractSdkEvent, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("sessionId")]
        [JsonRequired]
        public ulong SessionId { get; private set; }

        [JsonProperty("step")]
        [JsonRequired]
        public int Step { get; private set; }
    }
}
