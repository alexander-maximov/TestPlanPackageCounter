namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using TestplanPackageCounter.Packages.Content.General;

    public abstract class AbstractSdkEvent : AbstractData
    {
        [JsonIgnore]
        public bool Validated { get; set; }

        public virtual IEnumerable<AbstractSdkEvent> ToPlain()
        {
            yield return this;
        }

        [JsonProperty("inProgress")]
        [DefaultValue(null)]
        public string[] InProgress { get; private set; }
    }
}
