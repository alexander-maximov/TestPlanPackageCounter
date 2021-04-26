namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;

    public class SpEvent : AbstractSocialEvent
    {
        [JsonProperty("postReason")]
        [JsonRequired]
        public string PostReason { get; private set; }
    }
}
