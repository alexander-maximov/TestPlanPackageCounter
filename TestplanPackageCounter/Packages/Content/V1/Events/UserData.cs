namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    [JsonObject(MissingMemberHandling = MissingMemberHandling.Ignore)]
    public class UserData
    {
        [JsonProperty("age")]
        public int? Age { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("email")]
        public string Email { get; private set; }

        [JsonProperty("phone")]
        public string Phone { get; private set; }

        [JsonProperty("photo")]
        public string Photo { get; private set; }

        [JsonProperty("gender")]
        public int? Gender { get; private set; }

        [JsonProperty("cheater")]
        public bool? Cheater { get; private set; }

        [JsonExtensionData]
        public JObject RawCustomData { get; private set; }

        [JsonIgnore]
        public Dictionary<string, object> CustomData { get; private set; }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (this.RawCustomData == null)
            {
                return;
            }
            this.CustomData = new Dictionary<string, object>();
            foreach (KeyValuePair<string, JToken> item in this.RawCustomData)
            {
                string decodedKey = Uri.UnescapeDataString(item.Key);
                object decodedValue;
                switch (item.Value.Type)
                {
                    case JTokenType.Integer:
                        decodedValue = item.Value.Value<int>();
                        break;
                    case JTokenType.Float:
                        decodedValue = item.Value.Value<double>();
                        break;
                    case JTokenType.Boolean:
                        decodedValue = item.Value.Value<bool>();
                        break;
                    case JTokenType.Array:
                        decodedValue = ((JArray)item.Value)
                            .ToObject<IEnumerable<object>>()
                            .Select(e =>
                            {
                                if (e is string strElem)
                                {
                                    return Uri.UnescapeDataString(strElem);
                                }
                                return e;
                            })
                            .OrderBy(o => o?.GetHashCode() ?? 0)
                            .ToArray();
                        break;
                    case JTokenType.Null:
                        decodedValue = null;
                        break;
                    default:
                        decodedValue = Uri.UnescapeDataString(item.Value.Value<string>());
                        break;
                }
                this.CustomData.Add(decodedKey, decodedValue);
            }
        }

        [JsonIgnore]
        public bool Validated { get; private set; }
    }
}
