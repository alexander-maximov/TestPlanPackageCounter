using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public class VpV2 : AbstractSdkEventV2, IHasBasicValues, IHasTimestamp, IHasCodeValue
    {
        [JsonProperty("level")]
        [DefaultValue(null)]
        public uint? Level { get; private set; }

        [JsonProperty("purchaseAmount")]
        [DefaultValue(null)]
        public int? PurchaseAmount { get; private set; }

        [JsonProperty("purchasePrice")]
        [DefaultValue(null)]
        public Dictionary<string, long> PurchasePrice { get; private set; }

        [JsonProperty("purchaseType")]
        [DefaultValue(Constants.NotExistsString)]
        public string PurchaseType { get; private set; }

        [JsonProperty("purchaseId")]
        [DefaultValue(Constants.NotExistsString)]
        public string PurchaseId { get; private set; }

        [JsonProperty("sessionId")]
        [DefaultValue(null)]
        public ulong? SessionId { get; private set; }

        [JsonProperty("timestamp")]
        [DefaultValue(null)]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("inExperiments")]
        [DefaultValue(null)]
        public IeEntry[] InExperiments { get; private set; }

        [JsonProperty("code")]
        [DefaultValue(Constants.NotExistsString)]
        public string Code { get; private set; }
    }
}
