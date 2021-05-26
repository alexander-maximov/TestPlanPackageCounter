using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestplanPackageCounter.General;

namespace TestplanPackageCounter.Packages.Content.V2.SdkVersion
{
    public class SdkVersionRequest : RequestJsonContainerV2
    {
        [JsonProperty("sdkVersion")]
        [DefaultValue(Constants.NotExistsString)]
        public string Version { get; private set; }

        [JsonProperty("appVersion")]
        [DefaultValue(Constants.NotExistsString)]
        public string AppVersion { get; private set; }

        [JsonProperty("sdkCodeVersion")]
        [DefaultValue(null)]
        public uint? SdkCodeVersion { get; private set; }

        [JsonProperty("excludeVersion")]
        [DefaultValue(null)]
        public long? ExcludeVersionHash { get; private set; }
    }
}
