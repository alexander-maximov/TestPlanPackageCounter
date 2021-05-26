using Newtonsoft.Json;
using System.ComponentModel;

namespace TestplanPackageCounter.Packages.Content.V2.UserIdentification
{
    public class UserIdentificationRequest : RequestJsonContainerV2
    {
        [JsonProperty("attempt")]
        [DefaultValue(null)]
        public int? Attempt { get; private set; }
    }
}
