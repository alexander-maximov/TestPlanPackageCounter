using System;
using System.Collections.Generic;
using System.Linq;
using TestplanPackageCounter.Counter;
using TestplanPackageCounter.General.EventExtensions;
using TestplanPackageCounter.Packages.Content.General;
using TestplanPackageCounter.Packages.Content.V2;
using TestplanPackageCounter.Packages.Content.V2.Analytics;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events;
using TestplanPackageCounter.Packages.Content.V2.SdkVersion;
using TestplanPackageCounter.Packages.Content.V2.UserIdentification;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal class TestPackagesData
    {
        private Dictionary<string, LinkedList<ProxyPackageInfoV2>> _linkedPackages = new();

        internal int OriginalPackagesCount { get; }

        internal int PackagesCount { get; }

        internal int PackagesCountWithoutAlCaUe { get; }

        internal int SdkVersionCount { get; }

        internal int UePackagesCount { get; }

        internal int AlPackagesCount { get; }

        internal int CaPackagesCount { get; }

        internal bool IsLastUeEventRemoved { get; }

        internal bool IsLastAlEventRemoved { get; }

        internal bool IsFirstUeRemoved { get; }

        internal int IgnoredPackagesCount { get; }

        internal bool IsDoublesRemoved { get; }

        internal IEnumerable<string> DoublesSignatures { get; }

        internal IEnumerable<ProxyPackageInfo> BadCodesPackages { get; } = new List<ProxyPackageInfo>();

        internal IEnumerable<string> EventCodes { get; } = new List<string>();

        internal int EventsCount { get; }

        internal int UePackagesCountWithoutIgnored { get; }

        internal int AlPackagesCountWithoutIgnored { get; }

        internal int PackagesCountWithoutIgnored { get; }

        internal int AttemptPackagesCount { get; }

        internal bool IsAllEventsOrdered { get; }

        internal bool ContainsZeroCodePackage { get; private set; }

        internal bool ContainsNotDeliveredPackage { get; private set; }

        internal bool PreviousTestContainsCleaning { get; }

        internal bool ContainsDeserializationErrors { get; }

        internal TestPackagesData(
            int originalPackagesCount,
            int packagesCount,
            int alPackagesCount,
            int uePackagesCount,
            int caPackagesCount,
            int attemptPackagesCount,
            int sdkVersionCount,
            List<string> events,
            IEnumerable<string> doublesSignatures,
            IEnumerable<ProxyPackageInfo> badCodesPackages,
            bool isLastAlRemoved,
            bool isLastUeRemoved,
            bool isFirstUeRemoved,
            bool isAllEventsOrdered,
            bool previousTestContainsCleaning,
            bool containsDeserializationErrors,
            bool containsZeroCodePackage = false
        )
        {
            this.OriginalPackagesCount = originalPackagesCount;
            this.PackagesCount = packagesCount;
            this.AlPackagesCount = alPackagesCount;
            this.UePackagesCount = uePackagesCount;
            this.CaPackagesCount = caPackagesCount;
            this.IsLastAlEventRemoved = isLastAlRemoved;
            this.IsLastUeEventRemoved = isLastUeRemoved;
            this.IsAllEventsOrdered = isAllEventsOrdered;
            this.DoublesSignatures = doublesSignatures;
            this.BadCodesPackages = badCodesPackages;
            this.IsDoublesRemoved = this.DoublesSignatures.Any();            
            this.AlPackagesCountWithoutIgnored =
                this.IsLastAlEventRemoved ? this.AlPackagesCount - 1 : this.AlPackagesCount;
            this.UePackagesCountWithoutIgnored =
                this.IsLastUeEventRemoved ? this.UePackagesCount - 1 : this.UePackagesCount;
            this.UePackagesCountWithoutIgnored =
                isFirstUeRemoved ? this.UePackagesCount - 1 : this.UePackagesCount;
            this.PackagesCountWithoutAlCaUe =
                this.PackagesCount - (this.AlPackagesCount + this.UePackagesCount + this.CaPackagesCount);
            this.AttemptPackagesCount = attemptPackagesCount;
            this.SdkVersionCount = sdkVersionCount;
            this.IgnoredPackagesCount =
                (this.IsLastAlEventRemoved ? 1 : 0)
                + (this.IsLastUeEventRemoved ? 1 : 0)
                + (isFirstUeRemoved ? 1 : 0)
                + this.DoublesSignatures.Count()
                + this.AttemptPackagesCount
                + this.BadCodesPackages.Count();

            this.EventCodes = events;

            //TODO: this is sdkV1 plug
            this.EventsCount = events != null ? events.Where(e => !e.Contains("err") && !e.Contains("ign")).Count() : 0;

            this.PackagesCountWithoutIgnored = this.PackagesCount - this.IgnoredPackagesCount;
            this.ContainsZeroCodePackage = containsZeroCodePackage;

            this.PreviousTestContainsCleaning = previousTestContainsCleaning;
            this.ContainsDeserializationErrors = containsDeserializationErrors;
        }

        internal TestPackagesData()
        {
            this.OriginalPackagesCount = 999;
            this.PackagesCount = 999;
            this.AlPackagesCount = 0;
            this.UePackagesCount = 0;
            this.IsLastAlEventRemoved = false;
            this.IsLastUeEventRemoved = false;
            this.IsAllEventsOrdered = true;
            this.DoublesSignatures = null;
            this.IsDoublesRemoved = false;
            this.AlPackagesCountWithoutIgnored =
                this.IsLastAlEventRemoved ? this.AlPackagesCount - 1 : this.AlPackagesCount;
            this.UePackagesCountWithoutIgnored =
                this.IsLastUeEventRemoved ? this.UePackagesCount - 1 : this.UePackagesCount;
            this.PackagesCountWithoutAlCaUe =
                this.PackagesCount - (this.AlPackagesCount + this.UePackagesCount);
            this.AttemptPackagesCount = 0;
            this.IgnoredPackagesCount = 0;

            this.PackagesCountWithoutIgnored = this.PackagesCount - this.IgnoredPackagesCount;
        }

        internal TestPackagesData(
            List<ProxyPackageInfoV2> testPackages,
            Dictionary<string, List<(string, bool)>> platformCompletedTestSequenceList,
            Dictionary<string, LinkedList<ProxyPackageInfoV2>> linkedPackages,
            string testName,
            string platformName
        )
        {
            this._linkedPackages = linkedPackages;

            this.OriginalPackagesCount = testPackages.Count;
            this.PackagesCount = testPackages.Count;

            this.AlPackagesCount = this.FindAllPackagesOfEvent<AlV2>(testPackages).Count();
            this.UePackagesCount = this.FindAllPackagesOfEvent<UeV2>(testPackages).Count();
            this.CaPackagesCount = this.FindAllPackagesOfEvent<CaV2>(testPackages).Count();

            this.IsLastAlEventRemoved = this.GetPackageWithLastEventOfType<AlV2, UeV2>(testPackages) != null;
            this.IsLastUeEventRemoved = this.GetPackageWithLastEventOfType<UeV2, AlV2>(testPackages) != null;

            this.IsFirstUeRemoved = this.GetJumpedEventFromPreviousTestOfType<UeV2>(testPackages, platformName) != null;

            this.IsAllEventsOrdered = this.CheckEventsTimestampOrder(testPackages, platformName);

            this.BadCodesPackages = 
                CounterSettings.IgnoreBadCodePackages 
                    ? new List<ProxyPackageInfoV2>() 
                    : this.GetBadCodePackages(testPackages);

            this.PreviousTestContainsCleaning = this.IsPreviousTestContainsCleaning(platformCompletedTestSequenceList, testName, platformName);

            this.AttemptPackagesCount =
                new List<ProxyPackageInfoV2>(testPackages.Where(e => e.RequestJson is UserIdentificationRequest)).Count;
            this.SdkVersionCount =
                new List<ProxyPackageInfoV2>(testPackages.Where(e => e.RequestJson is SdkVersionRequest)).Count;

            this.EventCodes = this.GetEventCodes(testPackages, platformName);

            //TODO: this boi isn't work
            /*
            bool containsDeserializationErrors = this.catchDeserializationErrorTests.Any(e => e.Key.Equals(platformName, StringComparison.OrdinalIgnoreCase) &&
                e.Value.Any(x => x.Equals(testName, StringComparison.OrdinalIgnoreCase)));
            */
        }

        private IEnumerable<string> GetEventCodes(
            List<ProxyPackageInfoV2> testPackages,
            string platformName
        )
        {
            int index = 1;
            List<string> eventCodes = new();

            List<ProxyPackageInfoV2> packagesToChooseFrom = new(
                testPackages.Where(
                    e => e != this.GetPackageWithLastEventOfType<AlV2, UeV2>(testPackages)
                        && e != this.GetPackageWithLastEventOfType<UeV2, AlV2>(testPackages)
                        && e != this.GetJumpedEventFromPreviousTestOfType<UeV2>(testPackages, platformName)
                        && !this.GetBadCodePackages(testPackages).Contains(e)
                )
            );

            if (this.PreviousTestContainsCleaning && (CounterSettings.IgnoreLastAl || CounterSettings.IgnoreBadUe))
            {
                if (this.IsFirstUeRemoved)
                {
                    foreach (AbstractSdkEventV2 abstractSdkEvent in this.GetJumpedEventFromPreviousTestOfType<UeV2>(testPackages, platformName).AllEvents())
                    {
                        eventCodes.Add($"(first){abstractSdkEvent.Code}");
                        index++;
                    }
                }
            }

            foreach (ProxyPackageInfoV2 testPackage in packagesToChooseFrom.Where(e => e.RequestJson is AnalyticsRequest).OrderBy(e => e.Timestamp))
            {
                foreach (AbstractSdkEventV2 abstractSdkEvent in testPackage.AllEvents().OrderBy(e => e.Code))
                {
                    eventCodes.Add($"({index}){abstractSdkEvent.Code}");
                }

                index++;
            }

            foreach (ProxyPackageInfoV2 badCodePackage in this.BadCodesPackages)
            {
                foreach (AbstractSdkEventV2 abstractSdkEvent in badCodePackage.AllEvents())
                {
                    eventCodes.Add($"(err){abstractSdkEvent.Code}");
                }
            }

            if (this.PreviousTestContainsCleaning && (CounterSettings.IgnoreLastAl || CounterSettings.IgnoreBadUe))
            {
                if (this.IsLastUeEventRemoved)
                {
                    ProxyPackageInfoV2 lastUeEvent = this.GetPackageWithLastEventOfType<UeV2, AlV2>(testPackages);

                    foreach (AbstractSdkEventV2 abstractSdkEvent in lastUeEvent.AllEvents())
                    {
                        eventCodes.Add($"(last){abstractSdkEvent.Code}");
                    }
                }

                if (this.IsLastAlEventRemoved)
                {
                    ProxyPackageInfoV2 lastAlEvent = this.GetPackageWithLastEventOfType<AlV2, UeV2>(testPackages);

                    foreach (AbstractSdkEventV2 abstractSdkEvent in lastAlEvent.AllEvents())
                    {
                        eventCodes.Add($"(last){abstractSdkEvent.Code}");
                    }
                }
            }

            return eventCodes;
        }

        private bool IsPreviousTestContainsCleaning(
            Dictionary<string, List<(string, bool)>> platformCompletedTestSequenceList,
            string testName,
            string platformName
        )
        {
            return platformCompletedTestSequenceList.Any(
                 e => e.Key.Equals(platformName, StringComparison.OrdinalIgnoreCase)
                 && e.Value.Any(
                     x => x.Item1.Equals(testName, StringComparison.OrdinalIgnoreCase)
                 )
             )
             && platformCompletedTestSequenceList[platformName].Find(
                 e => e.Item1.Equals(testName, StringComparison.OrdinalIgnoreCase)
             ).Item2;
        }

        private IEnumerable<ProxyPackageInfoV2> GetBadCodePackages(List<ProxyPackageInfoV2> testPackages)
        {
            this.ContainsZeroCodePackage = false;
            ContainsNotDeliveredPackage = false;

            foreach (ProxyPackageInfoV2 testPackage in testPackages.Where(e => e.ResponseCode == 0 || e.ResponseCode == 404).ToList())
            {
                this.ContainsZeroCodePackage = this.ContainsZeroCodePackage || testPackage.ResponseCode == 0;
                ContainsNotDeliveredPackage = ContainsNotDeliveredPackage || testPackage.ResponseCode == 404;

                yield return testPackage;
            }
        }

        private bool CheckEventsTimestampOrder(List<ProxyPackageInfoV2> testPackages, string platformName)
        {
            LinkedList<ProxyPackageInfoV2> linkedPackages = this._linkedPackages[platformName];

            if (!linkedPackages.Any())
            {
                return true;
            }

            foreach (ProxyPackageInfoV2 currentPackage in testPackages)
            {
                if (currentPackage.RequestJson == null)
                {
                    continue;
                }

                IEnumerable<IHasTimestamp> currentPackageTimestampEvents =
                    currentPackage.AllEvents().OfType<IHasTimestamp>().OrderBy(e => e.Timestamp);

                if (!currentPackageTimestampEvents.Any())
                {
                    continue;
                }

                ulong minimalCurrentPackageTimestamp = (ulong)currentPackageTimestampEvents.First().Timestamp;
                ulong maximalCurrentPackageTimestamp = (ulong)currentPackageTimestampEvents.Last().Timestamp;

                if (linkedPackages.Find(currentPackage) == null)
                {
                    continue;
                }

                ProxyPackageInfoV2 nextPackage = currentPackage != linkedPackages.Last()
                    ? linkedPackages.Find(currentPackage).Next.Value
                    : currentPackage;

                IEnumerable<IHasTimestamp> nextPackageTimestampEvents =
                    nextPackage.AllEvents().OfType<IHasTimestamp>().OrderBy(e => e.Timestamp);

                if (nextPackageTimestampEvents.Any() && nextPackage != currentPackage)
                {
                    ulong minimalNextPackageTimestamp = (ulong)nextPackageTimestampEvents.First().Timestamp;

                    if (maximalCurrentPackageTimestamp >= minimalNextPackageTimestamp)
                    {
                        return false;
                    }
                }

                ProxyPackageInfoV2 previousPackage = currentPackage != linkedPackages.First()
                    ? linkedPackages.Find(currentPackage).Previous.Value
                    : currentPackage;

                IEnumerable<IHasTimestamp> previousPackageTimestampEvents =
                    previousPackage.AllEvents().OfType<IHasTimestamp>().OrderBy(e => e.Timestamp);

                if (previousPackageTimestampEvents.Any() && previousPackage != currentPackage)
                {
                    ulong maximalPreviousPackageTimestamp = (ulong)previousPackageTimestampEvents.Last().Timestamp;

                    if (maximalPreviousPackageTimestamp >= minimalCurrentPackageTimestamp)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private IEnumerable<ProxyPackageInfoV2> FindAllPackagesOfEvent<T>(IEnumerable<ProxyPackageInfoV2> testPackages)
        {
            List<ProxyPackageInfoV2> packagesWithDesiredEventType = new();

            IEnumerable<T> desiredTypeEvents = testPackages.AllEventsOfType<T>();

            foreach (T desiredTypeEvent in desiredTypeEvents)
            {
                if (desiredTypeEvent is AbstractSdkEventV2 abstractSdkEvent)
                {
                    packagesWithDesiredEventType.Add(abstractSdkEvent.FindPackage(testPackages));
                }
            }

            return packagesWithDesiredEventType.Distinct();
        }

        private ProxyPackageInfoV2 GetPackageWithLastEventOfType<T1, T2>(IEnumerable<ProxyPackageInfoV2> testPackages)
        {
            IEnumerable<T1> desiredTypeEvents = testPackages.AllEventsOfType<T1>();
            IEnumerable<IHasTimestamp> timestampEvents = testPackages.AllEventsOfType<IHasTimestamp>().OrderBy(e => e.Timestamp);

            if (desiredTypeEvents.Count() != 0 && timestampEvents.Count() != 0)
            {
                IHasTimestamp lastEventByTimestamp = timestampEvents.Last();

                foreach (T1 desiredTypeEvent in desiredTypeEvents)
                {
                    if (desiredTypeEvent is IHasTimestamp timestampEvent
                        && timestampEvent == lastEventByTimestamp
                    )
                    {
                        lastEventByTimestamp = timestampEvent;
                    }
                }

                if (lastEventByTimestamp is T1 &&
                    lastEventByTimestamp is AbstractSdkEventV2 firstTypeEvent
                )
                {
                    ProxyPackageInfoV2 packageWithDesiredEvent = firstTypeEvent.FindPackage(testPackages);
                    return packageWithDesiredEvent.AllEvents().Count() == 1 ? packageWithDesiredEvent : null;
                }

                if (lastEventByTimestamp is T2)
                {
                    List<IHasTimestamp> timestampEventsWithoutLast = new(timestampEvents);
                    timestampEventsWithoutLast.Remove(lastEventByTimestamp);

                    IHasTimestamp secondLastEventByTimestamp = timestampEventsWithoutLast.Last();

                    if (secondLastEventByTimestamp is T1 &&
                        secondLastEventByTimestamp is AbstractSdkEventV2 secondTypeEvent
                    )
                    {
                        ProxyPackageInfoV2 packageWithDesiredEvent = secondTypeEvent.FindPackage(testPackages);
                        return packageWithDesiredEvent.AllEvents().Count() == 1 ? packageWithDesiredEvent : null;
                    }
                }
            }

            return null;
        }

        private ProxyPackageInfoV2 GetJumpedEventFromPreviousTestOfType<T>(
            IEnumerable<ProxyPackageInfoV2> testPackages,
            string platformName
        )
        {
            if (!this._linkedPackages.ContainsKey(platformName))
            {
                return null;
            }

            IEnumerable<IHasTimestamp> timestampEvents =
                testPackages.AllEventsOfType<IHasTimestamp>().OrderBy(e => e.Timestamp);

            if (!timestampEvents.Any())
            {
                return null;
            }

            if (timestampEvents.First() is UeV2 firstUeEvent && firstUeEvent.FindPackage(testPackages).AllEvents().Count() == 1)
            {
                ProxyPackageInfoV2 firstUePackage = firstUeEvent.FindPackage(testPackages);

                if (this._linkedPackages[platformName].Find(firstUePackage) == null)
                {
                    return null;
                }

                ProxyPackageInfoV2 previousTestPackage =
                    this._linkedPackages[platformName].Find(firstUePackage).Previous.Value;

                string previousTestPackageTestName = previousTestPackage.TestName;

                if (firstUePackage.TestName.Equals(previousTestPackageTestName, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                IEnumerable<ProxyPackageInfoV2> previousTestPackages =
                    this._linkedPackages[platformName].Where(e => e.TestName.Equals(previousTestPackageTestName, StringComparison.OrdinalIgnoreCase));

                SsV2 previousTestSsEvent = previousTestPackages.AllEventsOfType<SsV2>().FirstOrDefault();

                if (previousTestSsEvent == null)
                {
                    IEnumerable<IHasSessionID> sessionIdEvents = previousTestPackages.AllEventsOfType<IHasSessionID>();

                    if (sessionIdEvents.Any(e => e.SessionId == firstUeEvent.SessionId))
                    {
                        return firstUePackage;
                    }

                    return null;
                }

                //TODO: If userCounting in prev and change user, than return null, i.e. valid

                ulong? previousTestSessionID = previousTestSsEvent.Timestamp;

                if (firstUeEvent.SessionId == previousTestSessionID)
                {
                    return firstUePackage;
                }
            }

            return null;
        }
    }
}
