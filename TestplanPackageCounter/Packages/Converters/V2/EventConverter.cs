using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TestplanPackageCounter.Packages.Content.V2;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events;
using TestplanPackageCounter.Packages.Converters.General;

namespace TestplanPackageCounter.Packages.Converters.V2
{
    public class EventConverter : EntryConverter
    {
        private readonly Type _eventsType = typeof(EventTypeV2);

        private readonly Type _abstractSdkEventType = typeof(AbstractSdkEventV2);

        private readonly Dictionary<EventTypeV2, Type> eventTypeToArrayTypeMap =
            new Dictionary<EventTypeV2, Type>
            {
                {EventTypeV2.Al, typeof(AlV2)},
                {EventTypeV2.Ca, typeof(CaV2)},
                {EventTypeV2.Ce, typeof(CeV2)},
                {EventTypeV2.Di, typeof(DiV2)},
                {EventTypeV2.Lu, typeof(LuV2)},
                {EventTypeV2.Pe, typeof(PeV2)},
                {EventTypeV2.Pl, typeof(PlV2)},
                {EventTypeV2.Rf, typeof(RfV2)},
                {EventTypeV2.Rp, typeof(RpV2)},
                {EventTypeV2.Sc, typeof(ScV2)},
                {EventTypeV2.Sp, typeof(SpV2)},
                {EventTypeV2.Ss, typeof(SsV2)},
                {EventTypeV2.Tr, typeof(TrV2)},
                {EventTypeV2.Ts, typeof(TsV2)},
                {EventTypeV2.Ue, typeof(UeV2)},
                {EventTypeV2.Vp, typeof(VpV2)},
            };

        public override bool CanConvert(Type objectType) =>
            objectType == this._abstractSdkEventType;

        private protected override Type GetRuntimeType(JObject jsonObject, Type _)
        {
            JProperty eventCodeProperty = jsonObject.Property("code");

            string eventTypeName = eventCodeProperty.Value.ToString();
            EventTypeV2 eventType = (EventTypeV2)Enum.Parse(this._eventsType, eventTypeName, true);

            Type runtimeType = this.eventTypeToArrayTypeMap[eventType];

            return runtimeType;
        }
    }
}
