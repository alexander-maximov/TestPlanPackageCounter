﻿namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;
    using TestplanPackageCounter.Packages.Content.General;

    public class SsEvent : AbstractSdkEventV1, IHasTimestamp
    {
        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong? Timestamp { get; private set; }
    }
}
