using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Serialization;
using TestplanPackageCounter.General;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries
{
    [DataContract]
    public class PeEntry
    {
        [JsonProperty("source")]
        [DefaultValue(Constants.NotExistsString)]
        public string Source { get; }

        [JsonProperty("difficulty")]
        [DefaultValue(null)]
        public int? Difficulty { get; }

        [JsonProperty("success")]
        [DefaultValue(null)]
        public bool? Success { get; }

        [JsonProperty("duration")]
        [DefaultValue(null)]
        public uint? Duration { get; }
    }
}
