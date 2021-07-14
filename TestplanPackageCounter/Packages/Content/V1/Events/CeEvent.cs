namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;
    using TestplanPackageCounter.Packages.Content.V1.Events.Entries;

    public class CeEvent : AbstractSdkEventV1
    {
        [JsonProperty("name")]
        [JsonRequired]
        public string Name { get; private set; }

        [JsonProperty("entries")]
        [JsonRequired]
        public CeEntry[] Entries { get; private set; }
    }
}
