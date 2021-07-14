using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using TestplanPackageCounter.General;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries
{
    [DataContract]
    public class SbspEntry
    {
        [JsonProperty("currencyCode")]
        [DefaultValue(Constants.NotExistsString)]
        public string CurrencyCode { get; private set; }

        [JsonProperty("productId")]
        [DefaultValue(Constants.NotExistsString)]
        public string ProductID { get; private set; }

        [JsonProperty("price")]
        [DefaultValue(null)]
        public decimal? Price { get; private set; }

        [JsonProperty("introductoryPrice")]
        [DefaultValue(null)]
        public decimal? IntroductoryPrice { get; private set; }

        [JsonProperty("discounts")]
        [DefaultValue(null)]
        public Dictionary<string, decimal> Discounts { get; private set; }
    }
}
