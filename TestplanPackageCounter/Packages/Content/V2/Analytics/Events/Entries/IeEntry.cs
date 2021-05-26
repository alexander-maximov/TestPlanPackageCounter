using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Serialization;
using TestplanPackageCounter.General;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries
{
    [DataContract]
    public class IeEntry
    {
        [JsonProperty("id")]
        [DefaultValue(null)]
        public ulong? Id { get; }

        [JsonProperty("group")]
        [DefaultValue(Constants.NotExistsString)]
        public string Group { get; }
    }
}
