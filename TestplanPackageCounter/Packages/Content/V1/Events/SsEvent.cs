namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;

    public class SsEvent : AbstractSdkEvent, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong Timestamp { get; private set; }
    }
}
