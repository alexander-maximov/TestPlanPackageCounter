using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestplanPackageCounter.Packages.Content.V2.SdkVersion
{
    public class SdkVersionResponse : ResponseJsonContainerV2
    {
        [JsonProperty("timeForRequest")]
        [DefaultValue(null)]
        public ulong? TimeForRequest { get; private set; }

        [JsonProperty("countForRequest")]
        [DefaultValue(null)]
        public uint? CountForRequest { get; private set; }

        [JsonProperty("eventParamsCount")]
        [DefaultValue(null)]
        public int? EventParamsCount { get; private set; }

        [JsonProperty("sessionTimeout")]
        [DefaultValue(null)]
        public int? SessionTimeout { get; private set; }

        [JsonProperty("serverTime")]
        [DefaultValue(null)]
        public ulong? ServerTime { get; private set; }

        [JsonProperty("aliveTimeout")]
        [DefaultValue(null)]
        public ulong? AliveTimeout { get; private set; }

        [JsonProperty("exclude")]
        [DefaultValue(null)]
        public ExcludeEntry Exclude { get; private set; }

        [JsonProperty("userCounting")]
        [DefaultValue(null)]
        public bool? UserCounting { get; private set; }

        [JsonProperty("userProperties")]
        [DefaultValue(null)]
        public Dictionary<string, object> UserProperties { get; private set; }

        [JsonProperty("currencyAggregationTimeout")]
        [DefaultValue(null)]
        public ulong? CurrencyAggregationTimeout { get; private set; }
    }
}
