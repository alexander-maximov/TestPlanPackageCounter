using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Serialization;
using TestplanPackageCounter.General;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries
{
    [DataContract]
    public class SbsrEntry
    {
        [JsonProperty("productId")]
        [DefaultValue(Constants.NotExistsString)]
        public string ProductID { get; private set; }

        [JsonProperty("transactionIds")]
        [DefaultValue(null)]
        public string[] TransactionIds { get; private set; }
    }
}
