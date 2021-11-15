using Newtonsoft.Json;
using System.ComponentModel;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public abstract class AbstractSdkEventV2 : AbstractSdkEvent
    {
        [JsonProperty("inProgress")]
        [DefaultValue(null)]
        public string[] InProgress { get; private set; }

        [JsonProperty("code")]
        [DefaultValue(Constants.NotExistsString)]
        public EventTypeV2 Code { get; private set; }
    }
}
