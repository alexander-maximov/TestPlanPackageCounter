using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public class AdrvV2 : AbstractSdkEventV2, IHasTimestamp, IHasSessionID
    {
        [JsonProperty("timestamp")]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("sessionId")]
        public ulong? SessionId { get; private set; }

        [JsonProperty("source")]
        [DefaultValue(Constants.NotExistsString)]
        public string Source { get; private set; }

        [JsonProperty("ad_network")]
        public string AdNetwork { get; private set; }

        [JsonProperty("placement")]
        [DefaultValue(Constants.NotExistsString)]
        public string Placement { get; private set; }

        [JsonProperty("ad_unit")]
        [DefaultValue(Constants.NotExistsString)]
        public string AdUnit { get; private set; }

        [JsonProperty("revenue")]
        public double Revenue { get; private set; }
    }
}
