namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using TestplanPackageCounter.Packages.Content.General;

    public class PeEvent : AbstractSdkEventV1, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("id")]
        [JsonRequired]
        public string Id { get; private set; }

        [JsonProperty("sessionId")]
        [JsonRequired]
        public ulong SessionId { get; private set; }

        [JsonProperty("params")]
        [JsonRequired]
        public Dictionary<string, object> Params { get; private set; }

        [JsonProperty("earned")]
        [DefaultValue(null)]
        public Dictionary<string, int> RawEarned { get; private set; }

        [JsonProperty("spent")]
        [DefaultValue(null)]
        public Dictionary<string, int> RawSpend { get; private set; }

        [JsonIgnore]
        public Dictionary<string, int> Earned { get; private set; }

        [JsonIgnore]
        public Dictionary<string, int> Spend { get; private set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (this.RawEarned == null || this.RawSpend == null)
            {
                return;
            }

            if (this.RawEarned != null)
            {
                this.Earned = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int> item in this.RawEarned)
                {
                    this.Earned.Add(Uri.UnescapeDataString(item.Key), item.Value);
                }
            }

            if (this.RawSpend != null)
            {
                this.Spend = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int> item in this.RawSpend)
                {
                    this.Spend.Add(Uri.UnescapeDataString(item.Key), item.Value);
                }
            }
        }

    }
}
