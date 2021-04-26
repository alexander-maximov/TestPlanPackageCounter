namespace TestplanPackageCounter.General
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using TestplanPackageCounter.Packages.Content.V1;
    using TestplanPackageCounter.Packages.Content.V1.Events;
    using TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents;

    public static class EventsExtensionsV1
    {
        public static IEnumerable<TEvent> GetAllSubeventsOfType<TEvent>(
            this IEnumerable<Dictionary<EventType, AbstractSdkEvent[]>> subEventsPackedList
        )
        {
            foreach (var packedSubEvents in subEventsPackedList)
            {
                foreach (var unpackedSubEvents in packedSubEvents)
                {
                    EventType eventType = unpackedSubEvents.Key;
                }
            }

            return null;
        }

        public static IEnumerable<AbstractSdkEvent> GetAllSubevents(
            this IEnumerable<Dictionary<EventType, AbstractSdkEvent[]>> subEventsPackedList
        )
        {
            return from packedSubEvents in subEventsPackedList
                   from subEventsUnpacked in packedSubEvents.Values
                   from subEvent in subEventsUnpacked
                   select subEvent;
        }

        public static IEnumerable<Dictionary<EventType, AbstractSdkEvent[]>> GetAllSubeventsByPackages(
            this IEnumerable<Dictionary<int, LuEvent>> luEventList
        )
        {
            return from luEvent in luEventList
                   from luEventContent in luEvent.Values
                   select luEventContent.Events;
        }

        public static IEnumerable<Dictionary<int, LuEvent>> GetAllLuEvents(
            this IEnumerable<ProxyPackageInfoV1> packagesList
        )
        {
            List<Dictionary<int, LuEvent>> luEventsList = new List<Dictionary<int, LuEvent>>();

            foreach (var package in packagesList)
            {
                if (package.RequestJson is LuData luData)
                {
                    luEventsList.Add(luData.LuEvents);
                }
            }

            return luEventsList;
        }
    }
}
