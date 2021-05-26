using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestplanPackageCounter.Packages.Content.General;

namespace TestplanPackageCounter.Packages.Content.V2
{
    public class ProxyPackageInfoV2 : ProxyPackageInfo
    {
        [JsonProperty("RequestJson")]
        [DefaultValue(null)]
        public RequestJsonContainerV2 RequestJson { get; private set; }

        [JsonProperty("ResponseJson")]
        [DefaultValue(null)]
        public ResponseJsonContainerV2 ResponseJson { get; private set; }
    }
}
