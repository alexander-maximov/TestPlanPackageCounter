namespace TestplanPackageCounter.Packages.Converters
{
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using System;
    using System.Collections.Generic;

    public abstract class EntryConverter : JsonConverter
    {
        public override object ReadJson(
            JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            //Null value handling may be better, perhaps
            serializer.MissingMemberHandling = MissingMemberHandling.Ignore;
            serializer.NullValueHandling = NullValueHandling.Ignore;

            JObject jsonObject = JObject.Load(reader);

            object runtimeObject = Activator.CreateInstance(this.GetRuntimeType(jsonObject, objectType));

            serializer.Populate(jsonObject.CreateReader(), runtimeObject);

            return runtimeObject;
        }

        private protected virtual Type GetRuntimeType(JObject jsonObject, Type runtimeType) => runtimeType;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}
