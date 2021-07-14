namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using System.ComponentModel;
    using Newtonsoft.Json;
    using TestplanPackageCounter.Packages.Content.General;

    public class PlEvent : AbstractSdkEventV1, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("sessionId")]
        [DefaultValue(null)]
        public ulong? SessionId { get; private set; }

        [JsonProperty("data")]
        [DefaultValue(null)]
        public UserData Data { get; private set; }
    }
}
