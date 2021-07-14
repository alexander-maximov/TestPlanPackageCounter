namespace TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using TestplanPackageCounter.Packages.Content.General;

    public class LuEvent : AbstractData
    {
        [JsonProperty("balance")]
        [DefaultValue(null)]
        public Dictionary<string, object> Balance { get; set; }

        [JsonProperty("spent")]
        [DefaultValue(null)]
        public Dictionary<string, object> Spent { get; set; }

        [JsonProperty("earned")]
        [DefaultValue(null)]
        public Dictionary<string, object> Earned { get; set; }

        [JsonProperty("bought")]
        [DefaultValue(null)]
        public Dictionary<string, object> Bought { get; set; }

        [JsonProperty("timestamp")]
        [DefaultValue(null)]
        public ulong? Timestamp { get; set; }

        [JsonProperty("sessionId")]
        [DefaultValue(null)]
        public ulong? SessionId { get; private set; }

        [JsonProperty("events")]
        [DefaultValue(null)]
        public Dictionary<EventType, AbstractSdkEventV1[]> Events { get; private set; }
    }
}
