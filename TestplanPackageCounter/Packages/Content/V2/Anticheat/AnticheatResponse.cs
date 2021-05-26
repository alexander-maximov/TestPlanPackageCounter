using Newtonsoft.Json;
using System.ComponentModel;
using TestplanPackageCounter.General;

namespace TestplanPackageCounter.Packages.Content.V2.Anticheat
{
    public class AnticheatResponse : ResponseJsonContainerV2
    {
        [JsonProperty("verificationResult")]
        [DefaultValue(Constants.NotExistsString)]
        public string VerificationResult { get; private set; }

        [JsonProperty("status")]
        [DefaultValue(null)]
        public int? Status { get; private set; }
    }
}
