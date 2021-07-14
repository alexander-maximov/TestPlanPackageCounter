namespace TestplanPackageCounter.Packages.Converters.V1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TestplanPackageCounter.Packages.Content.V1;
    using TestplanPackageCounter.Packages.Content.V1.Events;

    public class EventsArrayConverter : JsonConverter
    {
        private readonly Type _eventsDictionaryType =
            typeof(Dictionary<EventType, AbstractSdkEventV1[]>);

        private readonly Type _eventType = typeof(EventType);

        private readonly Dictionary<EventType, Type> _arrayTypeMap =
            new Dictionary<EventType, Type>
            {
                {EventType.Al, typeof(AlEvent[])},
                {EventType.Di, typeof(DiEvent[])},
                {EventType.Ai, typeof(AiEvent[])},
                {EventType.Ce, typeof(CeEvent[])},
                {EventType.Ip, typeof(IpEvent[])},
                {EventType.Pe, typeof(PeEvent[])},
                {EventType.Pl, typeof(PlEvent[])},
                {EventType.Rf, typeof(RfEvent[])},
                {EventType.Rp, typeof(RpEvent[])},
                {EventType.Sc, typeof(ScEvent[])},
                {EventType.Sp, typeof(SpEvent[])},
                {EventType.Ss, typeof(SsEvent[])},
                {EventType.Tr, typeof(TrEvent[])},
                {EventType.Ts, typeof(TsEvent[])},
                {EventType.Ue, typeof(UeEvent[])},
                {EventType.Ui, typeof(UiEvent[])}
            };

        public override bool CanConvert(Type objectType) =>
            objectType == this._eventsDictionaryType;

        public override object ReadJson(
            JsonReader reader,
            Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject eventsObject = JObject.Load(reader);

            Dictionary<string, AbstractSdkEventV1[]> unconvertedEventsDictionary =
                new Dictionary<string, AbstractSdkEventV1[]>();

            foreach (JProperty property in eventsObject.Properties())
            {
                string eventName = property.Name;
                EventType eventType = (EventType)Enum.Parse(this._eventType, eventName, true);

                JToken eventValue = property.Value;
                JArray eventObject = JArray.Load(eventValue.CreateReader());

                AbstractSdkEventV1[] eventsArray = (AbstractSdkEventV1[])serializer.Deserialize(
                    eventObject.CreateReader(),
                    this._arrayTypeMap[eventType]
                );

                unconvertedEventsDictionary.Add(eventName, eventsArray);
            }

            //Convert to enum. Easier to catch syncax mistake in case of event code mismatch
            Dictionary<EventType, AbstractSdkEventV1[]> eventsDictionary =
                unconvertedEventsDictionary.ToDictionary(
                    item => (EventType)Enum.Parse(typeof(EventType), item.Key, true),
                    item => item.Value
                );

            return eventsDictionary;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotImplementedException();
    }
}
