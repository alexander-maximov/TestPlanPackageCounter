using Newtonsoft.Json;
using System.ComponentModel;

namespace TestplanPackageCounter.Packages.Content.V2.UserIdentification
{
    public class RetryAfterResponse : ResponseJsonContainerV2
    {
        [JsonProperty("retryAfter")]
        [DefaultValue(null)]
        public int? RetryAfter { get; private set; }
    }
}
