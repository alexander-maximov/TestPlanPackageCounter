﻿using Newtonsoft.Json;
using System.ComponentModel;
using TestplanPackageCounter.General;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public class SpV2 : AbstractSdkEventV2, IHasBasicValues, IHasTimestamp, IHasSessionID
    {
        [JsonProperty("level")]
        [DefaultValue(null)]
        public uint? Level { get; private set; }

        [JsonProperty("socialNetwork")]
        [DefaultValue(Constants.NotExistsString)]
        public string SocialNetwork { get; private set; }

        [JsonProperty("postReason")]
        [DefaultValue(Constants.NotExistsString)]
        public string PostReason { get; private set; }

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
