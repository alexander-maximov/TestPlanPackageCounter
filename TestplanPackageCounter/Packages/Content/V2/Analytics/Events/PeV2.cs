﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public class PeV2 : AbstractSdkEventV2, IHasBasicValues, IHasTimestamp, IHasSessionID
    {
        [JsonProperty("level")]
        [DefaultValue(null)]
        public uint? Level { get; private set; }

        [JsonProperty("parameters")]
        [DefaultValue(null)]
        public PeEntry Parameters { get; private set; }

        [JsonProperty("spent")]
        [DefaultValue(null)]
        public Dictionary<string, long> Spent { get; private set; }

        [JsonProperty("earned")]
        [DefaultValue(null)]
        public Dictionary<string, long> Earned { get; private set; }

        [JsonProperty("name")]
        [DefaultValue(Constants.NotExistsString)]
        public string Name { get; private set; }

        [JsonProperty("sessionId")]
        [DefaultValue(null)]
        public ulong? SessionId { get; private set; }

        [JsonProperty("timestamp")]
        [DefaultValue(null)]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("inExperiments")]
        [DefaultValue(null)]
        public IeEntry[] InExperiments { get; private set; }
    }
}
