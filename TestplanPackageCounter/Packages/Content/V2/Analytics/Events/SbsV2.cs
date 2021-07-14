using Newtonsoft.Json;
using System.ComponentModel;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public class SbsV2 : AbstractSdkEventV2, IHasTimestamp, IHasSessionID
    {
        [JsonProperty("timestamp")]
        [DefaultValue(null)]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("sessionId")]
        [DefaultValue(null)]
        public ulong? SessionId { get; private set; }

        [JsonProperty("level")]
        [DefaultValue(null)]
        public int? Level { get; private set; }

        [JsonProperty("productId")]
        [DefaultValue(Constants.NotExistsString)]
        public string ProductId { get; private set; }

        [JsonProperty("orderId")]
        [DefaultValue(Constants.NotExistsString)]
        public string OrderId { get; private set; }

        [JsonProperty("price")]
        [DefaultValue(null)]
        public double? Price { get; private set; }

        [JsonProperty("currencyCode")]
        [DefaultValue(Constants.NotExistsString)]
        public string CurrencyCode { get; private set; }
    }
}
