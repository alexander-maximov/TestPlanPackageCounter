namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;
    using TestplanPackageCounter.Packages.Content.General;

    public class IpEvent : AbstractSdkEventV1, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("purchaseAmount")]
        [JsonRequired]
        public int PurchaseAmount { get; private set; }

        [JsonProperty("purchasePrice")]
        [JsonRequired]
        public int PurchasePrice { get; private set; }

        [JsonProperty("purchasePriceCurrency")]
        [JsonRequired]
        public string PurchasePriceCurrency { get; private set; }

        [JsonProperty("purchaseType")]
        [JsonRequired]
        public string PurchaseType { get; private set; }

        [JsonProperty("purchaseId")]
        [JsonRequired]
        public string PurchaseId { get; private set; }

        [JsonProperty("sessionId")]
        [JsonRequired]
        public ulong SessionId { get; private set; }
    }
}
