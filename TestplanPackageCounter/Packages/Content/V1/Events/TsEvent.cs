namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;
    using TestplanPackageCounter.Packages.Content.General;

    public class TsEvent : AbstractSdkEvent, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("isTrackingAllowed")]
        [JsonRequired]
        public bool IsTrackingAllowed { get; private set; }
    }
}
