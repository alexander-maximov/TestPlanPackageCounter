using Newtonsoft.Json;
using System.ComponentModel;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics
{
    public class AnalyticsRequest : RequestJsonContainerV2
    {
        [JsonProperty("reports")]
        [DefaultValue(null)]
        public ReportsData[] Reports { get; private set; }
    }
}
