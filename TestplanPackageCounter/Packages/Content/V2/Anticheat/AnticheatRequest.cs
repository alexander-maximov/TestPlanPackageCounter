using Newtonsoft.Json;
using System.ComponentModel;
using TestplanPackageCounter.General;

namespace TestplanPackageCounter.Packages.Content.V2.Anticheat
{
    public class AnticheatRequest : RequestJsonContainerV2
    {
        [JsonProperty("platform")]
        [DefaultValue(Constants.NotExistsString)]
        public string Platform { get; private set; }

        [JsonProperty("receipt")]
        [DefaultValue(Constants.NotExistsString)]
        public string Receipt { get; private set; }

        [JsonProperty("key")]
        [DefaultValue(Constants.NotExistsString)]
        public string Key { get; private set; }

        [JsonProperty("sig")]
        [DefaultValue(Constants.NotExistsString)]
        public string Sig { get; private set; }

        [JsonProperty("bundle")]
        [DefaultValue(Constants.NotExistsString)]
        public string Bundle { get; private set; }
    }
}
