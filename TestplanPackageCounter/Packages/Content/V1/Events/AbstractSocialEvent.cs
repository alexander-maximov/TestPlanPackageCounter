namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;
    using TestplanPackageCounter.Packages.Content.General;

    public abstract class AbstractSocialEvent : AbstractSdkEvent, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("socialNetwork")]
        [JsonRequired]
        public string SocialNetwork { get; private set; }

        [JsonProperty("sessionId")]
        [JsonRequired]
        public ulong SessionId { get; private set; }
    }
}
