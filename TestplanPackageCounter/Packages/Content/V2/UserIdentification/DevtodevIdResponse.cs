using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestplanPackageCounter.Packages.Content.V2.UserIdentification
{
    public class DevtodevIdResponse : ResponseJsonContainerV2
    {
        [JsonProperty("devtodevId")]
        [DefaultValue(null)]
        public long? DevtodevId { get; private set; }

        [JsonProperty("crossPlatformDevtodevId")]
        [DefaultValue(null)]
        public long? CrossPlatformDevtodevId { get; private set; }

        [JsonProperty("paymentCount")]
        [DefaultValue(null)]
        public int? PaymentCount { get; private set; }

        [JsonProperty("paymentSum")]
        [DefaultValue(null)]
        public double? PaymentSum { get; private set; }

        [JsonProperty("created")]
        [DefaultValue(null)]
        public long? Created { get; private set; }

        [JsonProperty("isNew")]
        [DefaultValue(null)]
        public bool? IsNew { get; private set; }
    }
}
