namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;
    using TestplanPackageCounter.Packages.Content.V1.Events.Entries;

    public class RpEvent : AbstractSdkEvent
    {
        [JsonProperty("name")]
        [JsonRequired]
        public string InAppName { get; private set; }

        [JsonProperty("entries")]
        [JsonRequired]
        public RpEntries[] Entries { get; private set; }
    }
}
