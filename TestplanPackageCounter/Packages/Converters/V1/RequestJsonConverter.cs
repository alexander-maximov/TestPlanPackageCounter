namespace TestplanPackageCounter.Packages.Converters.V1
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents;

    public class RequestJsonConverter : JsonConverter
    {
        private readonly Type _requestJsonType = typeof(RequestJsonContainerV1);
        private readonly Type _sdkVersionType = typeof(SdkVersionData);
        private readonly Type _luDataType = typeof(LuData);

        public override bool CanConvert(Type objectType) =>
            objectType == this._requestJsonType;

        public override object ReadJson(
            JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);

            object runtimeObject = serializer.Deserialize(
                jsonObject.CreateReader(),
                this.TypePicker(jsonObject)
            );

            return runtimeObject;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotImplementedException();

        private Type TypePicker(JObject jsonObject) =>
            jsonObject.Property("sdkVersion") != null
                ? this._sdkVersionType
                : this._luDataType;
    }
}
