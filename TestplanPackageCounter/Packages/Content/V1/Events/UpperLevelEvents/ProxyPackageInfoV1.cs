namespace TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents
{
    using Newtonsoft.Json;
    using System.ComponentModel;
    using TestplanPackageCounter.Packages.Content.General;

    public class ProxyPackageInfoV1 : ProxyPackageInfo
    {
        [JsonProperty("RequestJson")]
        [DefaultValue(null)]
        public RequestJsonContainerV1 RequestJson { get; private set; }

        [JsonProperty("ResponseJson")]
        [DefaultValue(null)]
        public ResponseJsonDataV1 ResponseJson { get; private set; }
    }
}
