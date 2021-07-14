namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using TestplanPackageCounter.Packages.Content.General;

    public abstract class AbstractSdkEventV1 : AbstractSdkEvent
    {
        [JsonIgnore]
        public bool Validated { get; set; }

        public virtual IEnumerable<AbstractSdkEventV1> ToPlain()
        {
            yield return this;
        }

        [JsonProperty("inProgress")]
        [DefaultValue(null)]
        public string[] InProgress { get; private set; }
    }
}
