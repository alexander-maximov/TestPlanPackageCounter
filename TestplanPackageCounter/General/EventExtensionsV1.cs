namespace TestplanPackageCounter.General
{
    using System.Collections.Generic;
    using System.Linq;
    using TestplanPackageCounter.Packages.Content.V1;
    using TestplanPackageCounter.Packages.Content.V1.Events;
    using TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents;

    public static class EventsExtensionsV1
    {
        /// <summary>
        /// Get all subevents of certain type.
        /// </summary>
        /// <typeparam name="T">Certain type.</typeparam>
        /// <param name="luEventList">Set of Lu events.</param>
        /// <returns>Set of subevents.</returns>
        public static IEnumerable<T> AllSubeventsOfType<T>(
            this IEnumerable<Dictionary<int, LuEvent>> luEventList
        ) => luEventList.GetAllLevelSubevents().GetAllSubevents().OfType<T>();

        /// <summary>
        /// Gets the package to which the Lu event belongs.
        /// </summary>
        /// <param name="packagesList">List of packages in which to search for an desired Lu event.</param>
        /// <param name="desiredluEvent">Desired Lu event.</param>
        /// <returns>Package, containing desired Lu Event.</returns>
        public static ProxyPackageInfoV1 FindPackageForLuEvent(
            this IEnumerable<ProxyPackageInfoV1> packagesList,
            Dictionary<int, LuEvent> desiredluEvent
        )
        {
            if (desiredluEvent == null || desiredluEvent.Count() == 0)
            {
                return null;
            }

            foreach (ProxyPackageInfoV1 package in packagesList)
            {
                if (package.RequestJson is LuData luData)
                {
                    if (luData.LuEvents.Equals(desiredluEvent))
                    {
                        return package;
                    }
                }
            }

            return null;
        }

        public static ProxyPackageInfoV1 FindPackageForSubEvent(
            this AbstractSdkEvent desiredEvent,
            IEnumerable<ProxyPackageInfoV1> packages
        )
        {
            Dictionary<EventType, AbstractSdkEvent[]> subEventPack = 
                packages.GetAllLuEvents().GetAllLevelSubevents().FindSubeventPackForEvent(desiredEvent);
            Dictionary<int, LuEvent> luEvent = packages.GetAllLuEvents().FindLuEventForEventsPack(subEventPack);

            return packages.FindPackageForLuEvent(luEvent);
        }

        /// <summary>
        /// Finds LuEvent for certain group of level subevents.
        /// </summary>
        /// <param name="luEventList">List of LuEvents in which to search for an desired value.</param>
        /// <param name="desiredLevelSubevents">Desired value.</param>
        /// <param name="isLuEventContainsOnlyDesiredPack">Condition to find LuEvent only desired value.</param>
        /// <returns>LuEvent contains desired value or null if unable to find.</returns>
        public static Dictionary<int, LuEvent> FindLuEventForEventsPack(
            this IEnumerable<Dictionary<int, LuEvent>> luEventList,
            Dictionary<EventType, AbstractSdkEvent[]> desiredLevelSubevents,
            bool isLuEventContainsOnlyDesiredPack = true
        )
        {
            if (desiredLevelSubevents == null || desiredLevelSubevents.Count == 0)
            {
                return null;
            }

            foreach (var luEvent in luEventList)
            {
                foreach (var levelLuEvent in luEvent)
                {
                    LuEvent currentLuEvent = levelLuEvent.Value;

                    bool searchCondition = isLuEventContainsOnlyDesiredPack
                        ? (currentLuEvent.Events == desiredLevelSubevents) && (luEvent.Count == 1)
                        : currentLuEvent.Events == desiredLevelSubevents;

                    if (searchCondition)
                    {
                        return luEvent;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a level subevents for certain subevent.
        /// </summary>
        /// <param name="levelSubevents">Level subevents pack in which to search for an desired value.</param>
        /// <param name="desiredEvent">Desired value.</param>
        /// <param name="isPackContainsOnlyDesiredEvent">Condition to find level subevent with only desired value.</param>
        /// <returns>Level subevent contains desired value or null if unable to find.</returns>
        public static Dictionary<EventType, AbstractSdkEvent[]> FindSubeventPackForEvent(
            this IEnumerable<Dictionary<EventType, AbstractSdkEvent[]>> levelSubevents,
            AbstractSdkEvent desiredEvent,
            bool isPackContainsOnlyDesiredEvent = true
        )
        {
            if (desiredEvent == null)
            {
                return null;
            }

            foreach (Dictionary<EventType, AbstractSdkEvent[]> packedSubevent in levelSubevents)
            {
                if (packedSubevent == null)
                {
                    continue;
                }
                foreach (AbstractSdkEvent[] unpackedEventsArray in packedSubevent.Values)
                {
                    bool searchCondition = isPackContainsOnlyDesiredEvent
                        ? (unpackedEventsArray.Contains(desiredEvent) && unpackedEventsArray.Length == 1)
                        : unpackedEventsArray.Contains(desiredEvent);

                    if (searchCondition)
                    {
                        return packedSubevent;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets list contains only subevents content.
        /// </summary>
        /// <param name="levelSubevents">level subevents to extract subevents content.</param>
        /// <returns>List contains subevents content.</returns>
        public static IEnumerable<AbstractSdkEvent> GetAllSubevents(
            this IEnumerable<Dictionary<EventType, AbstractSdkEvent[]>> levelSubevents
        )
        {
            List<AbstractSdkEvent> subEventsList = new List<AbstractSdkEvent>();

            foreach (var packedSubEvents in levelSubevents)
            {
                if (packedSubEvents == null)
                {
                    continue;
                }

                foreach (var subEventsUnpacked in packedSubEvents.Values)
                {
                    foreach (var subEvent in subEventsUnpacked)
                    {
                        subEventsList.Add(subEvent);
                    }
                }
            }

            return subEventsList;
        }

        /// <summary>
        /// Gets list contains only subevents from luEvents.
        /// </summary>
        /// <param name="luEventList">LuEvents to extract subevents.</param>
        /// <returns>List of subevents.</returns>
        public static IEnumerable<Dictionary<EventType, AbstractSdkEvent[]>> GetAllLevelSubevents(
            this IEnumerable<Dictionary<int, LuEvent>> luEventList
        ) => 
            from luEvent in luEventList
            from luEventContent in luEvent.Values
            select luEventContent.Events;

        /// <summary>
        /// Gets list of LuEvents from packages.
        /// </summary>
        /// <param name="packagesList">Packages to extract LuEvents.</param>
        /// <returns>List of LuEvents.</returns>
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

        public static IEnumerable<Dictionary<int, LuEvent>> GetAllLuEvents(
            this ProxyPackageInfoV1 package
        )
        {
            if (package.RequestJson is LuData luData)
            {
                return new List<Dictionary<int, LuEvent>>() { luData.LuEvents };
            }

            return null;
        }
    }
}
