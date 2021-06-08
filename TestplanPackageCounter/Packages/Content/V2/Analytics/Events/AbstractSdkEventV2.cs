using Newtonsoft.Json;
using System.ComponentModel;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public abstract class AbstractSdkEventV2 : AbstractData
    {
        [JsonProperty("inProgress")]
        [DefaultValue(null)]
        public string[] InProgress { get; private set; }

        [JsonProperty("code")]
        [DefaultValue(Constants.NotExistsString)]
        public string Code { get; private set; }
    }
}
