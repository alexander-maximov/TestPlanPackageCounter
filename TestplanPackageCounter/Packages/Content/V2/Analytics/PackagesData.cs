using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics
{
    public class PackagesData : AbstractData
    {
        [JsonProperty("language")]
        [DefaultValue(Constants.NotExistsString)]
        public string Language { get; private set; }

        [JsonProperty("ip")]
        [DefaultValue(Constants.NotExistsString)]
        public string Ip { get; private set; }

        [JsonProperty("appVersion")]
        [DefaultValue(Constants.NotExistsString)]
        public string AppVersion { get; private set; }

        [JsonProperty("appBuildVersion")]
        [DefaultValue(Constants.NotExistsString)]
        public string AppBuildVersion { get; private set; }

        [JsonProperty("sdkVersion")]
        [DefaultValue(Constants.NotExistsString)]
        public string SdkVersion { get; private set; }

        [JsonProperty("sdkCodeVersion")]
        [DefaultValue(null)]
        public uint? SdkCodeVersion { get; private set; }

        [JsonProperty("bundle")]
        [DefaultValue(Constants.NotExistsString)]
        public string Bundle { get; private set; }

        [JsonProperty("engine")]
        [DefaultValue(Constants.NotExistsString)]
        public string Engine { get; private set; }

        [JsonProperty("installationSource")]
        [DefaultValue(Constants.NotExistsString)]
        public string InstallationSource { get; private set; }

        [JsonProperty("events")]
        [DefaultValue(null)]
        public AbstractSdkEventV2[] Events { get; private set; }
    }
}
