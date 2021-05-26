using Newtonsoft.Json;
using System.ComponentModel;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public class AlV2 : AbstractSdkEventV2, IHasBasicValues, IHasTimestamp, IHasCodeValue
    {
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
