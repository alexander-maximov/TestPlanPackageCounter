namespace TestplanPackageCounter.Packages.Converters.V1
{
    using System;
    using System.Collections.Generic;
    using TestplanPackageCounter.Packages.Content.V1.Events;
    using TestplanPackageCounter.Packages.Content.V1.Events.Entries;
    using TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents;

    public class CommonConverter : EntryConverter
    {
        public override bool CanConvert(Type objectType) =>
            this._availableTypes.Contains(objectType);

        private readonly List<Type> _availableTypes = new List<Type>
        {
            typeof(ProxyPackageInfoV1),
            typeof(SdkVersionData),
            typeof(AlEvent),
            typeof(DiEvent),
            typeof(AiEvent),
            typeof(CeEvent),
            typeof(IpEvent),
            typeof(PeEvent),
            typeof(PlEvent),
            typeof(RfEvent),
            typeof(RpEvent),
            typeof(ScEvent),
            typeof(SpEvent),
            typeof(SsEvent),
            typeof(TrEvent),
            typeof(TsEvent),
            typeof(UeEvent),
            typeof(UiEvent),
            typeof(Dictionary<uint, LuEvent>),
            typeof(CeEntry),
            typeof(CeEntryParam)
        };
    }
}
