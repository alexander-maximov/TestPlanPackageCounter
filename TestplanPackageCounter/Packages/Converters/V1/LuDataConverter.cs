namespace TestplanPackageCounter.Packages.Converters.V1
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents;

    public class LuDataConverter : JsonConverter
    {
        private readonly Type _levelDataType = typeof(LuData);

        public override bool CanConvert(Type objectType) =>
            objectType == this._levelDataType;

        public override object ReadJson(
            JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            Dictionary<int, LuEvent> luDictionary =
                serializer.Deserialize<Dictionary<int, LuEvent>>(jsonObject.CreateReader());

            LuData levelData = new LuData
            {
                LuEvents = luDictionary
            };

            return levelData;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}
