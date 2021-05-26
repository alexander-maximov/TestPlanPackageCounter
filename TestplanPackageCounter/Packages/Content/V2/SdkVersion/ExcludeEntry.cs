using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TestplanPackageCounter.Packages.Content.General;

namespace TestplanPackageCounter.Packages.Content.V2.SdkVersion
{
    public class ExcludeEntry : AbstractData
    {
        [JsonProperty("events")]
        [DefaultValue(null)]
        public ExcludedEvents Events { get; private set; }

        [JsonProperty("version")]
        [DefaultValue(null)]
        public long? Version { get; private set; }
    }
}
