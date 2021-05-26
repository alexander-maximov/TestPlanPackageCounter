using Newtonsoft.Json;
using System.ComponentModel;
using TestplanPackageCounter.Packages.Content.General;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics
{
    public class ReportsData : AbstractData
    {
        [JsonProperty("packages")]
        [DefaultValue(null)]
        public PackagesData[] Packages { get; private set; }
    }
}
