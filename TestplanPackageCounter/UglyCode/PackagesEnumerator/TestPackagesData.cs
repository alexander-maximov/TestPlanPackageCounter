using System.Collections.Generic;
using System.Linq;
using TestplanPackageCounter.Packages.Content.General;

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

        internal IEnumerable<ProxyPackageInfo> BadCodesPackages { get; } = new List<ProxyPackageInfo>();

        internal IEnumerable<string> Events { get; } = new List<string>();

        internal int UePackagesCountWithoutIgnored { get; }

        internal int AlPackagesCountWithoutIgnored { get; }

        internal int PackagesCountWithoutIgnored { get; }

        internal int AttemptPackagesCount { get; }

        internal bool IsAllEventsOrdered { get; }

        internal bool ContainsZeroCodePackage { get; }

        internal TestPackagesData(
            int originalPackagesCount,
            int packagesCount,
            int alPackagesCount,
            int uePackagesCount,
            int attemptPackagesCount,
            List<string> events,
            IEnumerable<string> doublesSignatures,
            IEnumerable<ProxyPackageInfo> badCodesPackages,
            bool isLastAlRemoved,
            bool isLastUeRemoved,
            bool isAllEventsOrdered,
            bool containsZeroCodePackage = false
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
            this.BadCodesPackages = badCodesPackages;
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
                + this.AttemptPackagesCount
                + this.BadCodesPackages.Count();

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
            this.ContainsZeroCodePackage = containsZeroCodePackage;
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
            this.PackagesCountWithoutUeAndAl =
                this.PackagesCount - (this.AlPackagesCount + this.UePackagesCount);
            this.AttemptPackagesCount = 0;
            this.IgnoredPackagesCount = 0;

            this.PackagesCountWithoutIgnored = this.PackagesCount - this.IgnoredPackagesCount;
        }
    }
}
