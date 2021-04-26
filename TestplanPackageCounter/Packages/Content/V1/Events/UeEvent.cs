namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;

    public class UeEvent : AbstractSdkEvent, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong Timestamp { get; private set; }

        [JsonProperty("length")]
        [JsonRequired]
        public ushort Length { get; private set; }

        [JsonProperty("sessionId")]
        [JsonRequired]
        public ulong SessionId { get; private set; }
    }
}
