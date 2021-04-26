namespace TestplanPackageCounter.Packages.Content.V1.Events.Entries
{
    using Newtonsoft.Json;

    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class CeEntryParam
    {
        [JsonRequired]
        public Dictionary<string, Dictionary<string, object>> T1 { get; private set; }

        [JsonIgnore]
        public bool Validated { get; private set; }
    }
}
