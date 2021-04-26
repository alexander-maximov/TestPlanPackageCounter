namespace TestplanPackageCounter.Packages.Content.General
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using TestplanPackageCounter.General;

    public abstract class ProxyPackageInfo : AbstractData
    {
        [JsonProperty("RequestHeaders")]
        [DefaultValue(null)]
        public Dictionary<string, string[]> RequestHeaders { get; private set; }

        [JsonProperty("RequestUrl")]
        [DefaultValue(Constants.NotExistsString)]
        public string RequestUrl { get; private set; }

        [JsonProperty("RequestBodyLength")]
        [DefaultValue(null)]
        public uint? RequestBodyLength { get; private set; }

        [JsonProperty("ResponseCode")]
        [DefaultValue(null)]
        public int? ResponseCode { get; private set; }

        [JsonProperty("ResponseHeaders")]
        [DefaultValue(null)]
        public Dictionary<string, string[]> ResponseHeaders { get; private set; }

        [JsonProperty("ResponseContentHeaders")]
        [DefaultValue(null)]
        public Dictionary<string, string[]> ResponseContentHeaders { get; private set; }

        [JsonProperty("TestName")]
        [DefaultValue(Constants.NotExistsString)]
        public string TestName { get; private set; }
    }
}

