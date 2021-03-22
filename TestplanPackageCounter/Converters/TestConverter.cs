namespace TestplanPackageCounter.Converters
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using TestplanPackageCounter.TestplanContent;

    public class TestConverter : JsonConverter
    {
        private readonly Type _paramsTestType = typeof(ParamsTest);
        private readonly Type _paramlessTestType = typeof(ParamslessTest);

        public override bool CanConvert(Type objectType) =>
            objectType == this._paramlessTestType;

        public override object ReadJson(
            JsonReader reader, 
            Type objectType, 
            object existingValue, 
            JsonSerializer serializer
        )
        {
            JObject jsonObject = JObject.Load(reader);

            Type runtimeType = jsonObject.Property("Params") != null
                ? this._paramsTestType
                : this._paramlessTestType;

            object runtimeObject = Activator.CreateInstance(runtimeType);

            serializer.Populate(jsonObject.CreateReader(), runtimeObject);

            return runtimeObject;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken jToken = JToken.FromObject(value);

            jToken.WriteTo(writer);
        }
    }
}
