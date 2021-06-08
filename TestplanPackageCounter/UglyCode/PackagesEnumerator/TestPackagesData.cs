using System.Collections.Generic;
using System.Linq;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal class TestPackagesData
    {
        internal int OriginalPackagesCount { get; }

        internal int PackagesCount { get; }

        internal int PackagesCountWithoutUeAndAl { get; }

        internal int UePackagesCount { get; }

        internal int AlPackagesCount { get; }

        internal bool IsLastUeEventRemoved { get; }

        internal bool IsLastAlEventRemoved { get; }

        internal int IgnoredPackagesCount { get; }

        internal bool IsDoublesRemoved { get; }

        internal IEnumerable<string> DoublesSignatures { get; }

        internal IEnumerable<string> Events { get; }

        internal int UePackagesCountWithoutIgnored { get; }

        internal int AlPackagesCountWithoutIgnored { get; }

        internal int PackagesCountWithoutIgnored { get; }

        internal int AttemptPackagesCount { get; }

        internal bool IsAllEventsOrdered { get; }

        internal TestPackagesData(
            int originalPackagesCount,
            int packagesCount,
            int alPackagesCount,
            int uePackagesCount,
            int attemptPackagesCount,
            bool isLastAlRemoved,
            bool isLastUeRemoved,
            bool isAllEventsOrdered,
            List<string> events,
            IEnumerable<string> doublesSignatures
        )
        {
            this.OriginalPackagesCount = originalPackagesCount;
            this.PackagesCount = packagesCount;
            this.AlPackagesCount = alPackagesCount;
            this.UePackagesCount = uePackagesCount;
            this.IsLastAlEventRemoved = isLastAlRemoved;
            this.IsLastUeEventRemoved = isLastUeRemoved;
            this.IsAllEventsOrdered = isAllEventsOrdered;
            this.DoublesSignatures = doublesSignatures;
            this.IsDoublesRemoved = this.DoublesSignatures.Any();            
            this.AlPackagesCountWithoutIgnored =
                this.IsLastAlEventRemoved ? this.AlPackagesCount - 1 : this.AlPackagesCount;
            this.UePackagesCountWithoutIgnored =
                this.IsLastUeEventRemoved ? this.UePackagesCount - 1 : this.UePackagesCount;
            this.PackagesCountWithoutUeAndAl =
                this.PackagesCount - (this.AlPackagesCount + this.UePackagesCount);
            this.AttemptPackagesCount = attemptPackagesCount;
            this.IgnoredPackagesCount =
                (this.IsLastAlEventRemoved ? 1 : 0)
                + (this.IsLastUeEventRemoved ? 1 : 0)
                + this.DoublesSignatures.Count()
                + this.AttemptPackagesCount;

            if (events != null)
            {
                if (this.IsLastAlEventRemoved)
                {
                    events.Remove("al");
                }
                if (this.IsLastUeEventRemoved)
                {
                    events.Remove("ue");
                }
                this.Events = events;
            }

            this.PackagesCountWithoutIgnored = this.PackagesCount - this.IgnoredPackagesCount;
        }
    }
}
