namespace TestplanPackageCounter.Testplan.Converters
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using TestplanPackageCounter.Testplan.Content;

    class ParamsConverter : JsonConverter
    {
        private bool FillWithDefaultParams { get; }

        private readonly Type _paramsType = typeof(Params);
        private readonly Type _paramsNullsType = typeof(ParamsNulls);
        private readonly Type _paramsDefaultType = typeof(ParamsDefault);

        internal ParamsConverter(bool fillWithDefaultParams)
        {
            this.FillWithDefaultParams = fillWithDefaultParams;
        }

        public override bool CanConvert(Type objectType) =>
            objectType == this._paramsType;

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            JObject jsonObject = JObject.Load(reader);

            Type runtimeType = this.FillWithDefaultParams
                ? this._paramsDefaultType
                : this._paramsNullsType;

            object runtimeObject = Activator.CreateInstance(runtimeType);

            serializer.Populate(jsonObject.CreateReader(), runtimeObject);

            return runtimeObject;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
