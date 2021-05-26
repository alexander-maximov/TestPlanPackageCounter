using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestplanPackageCounter.Packages.Content.General;

namespace TestplanPackageCounter.Packages.Content.V2.SdkVersion
{
    public class ExcludedEvents : AbstractData
    {
        [JsonProperty("pl")]
        [DefaultValue(null)]
        public string[] Pl { get; private set; }

        [JsonProperty("ce")]
        [DefaultValue(null)]
        public string[] Ce { get; private set; }

        [JsonProperty("tr")]
        [DefaultValue(null)]
        public int[] Tr { get; private set; }

        [JsonProperty("vp")]
        [DefaultValue(null)]
        public string[] Vp { get; private set; }

        [JsonProperty("sc")]
        [DefaultValue(null)]
        public string[] Sc { get; private set; }

        [JsonProperty("sp")]
        [DefaultValue(null)]
        public string[] Sp { get; private set; }
    }
}
