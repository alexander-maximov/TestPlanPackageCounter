namespace TestplanPackageCounter.Packages.Content.V1.Events.Entries
{
    using Newtonsoft.Json;

    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class RpEntries
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong Timestamp { get; private set; }

        [JsonProperty("sessionId")]
        [JsonRequired]
        public ulong SessionId { get; private set; }

        [JsonProperty("orderId")]
        [JsonRequired]
        public string OrderId { get; private set; }

        [JsonProperty("price")]
        [JsonRequired]
        public float Price { get; private set; }

        [JsonProperty("currencyCode")]
        [JsonRequired]
        public string CurrencyCode { get; private set; }

        [JsonProperty("inProgress")]
        [DefaultValue(null)]
        public string[] InProgress { get; private set; }
    }
}
