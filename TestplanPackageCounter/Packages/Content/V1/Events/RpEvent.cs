namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;
    using TestplanPackageCounter.Packages.Content.V1.Events.Entries;

    public class RpEvent : AbstractSdkEventV1
    {
        [JsonProperty("name")]
        [JsonRequired]
        public string InAppName { get; private set; }

        [JsonProperty("entries")]
        [JsonRequired]
        public RpEntries[] Entries { get; private set; }
    }
}
