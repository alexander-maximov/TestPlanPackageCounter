using System.Collections.Generic;
using System.Linq;
using TestplanPackageCounter.Packages.Content.General;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal class TestPackagesData
    {
        internal int OriginalPackagesCount { get; }

        internal int PackagesCount { get; }

        internal int PackagesCountWithoutAlCaUe { get; }

        internal int SdkVersionCount { get; }

        internal int UePackagesCount { get; }

        internal int AlPackagesCount { get; }

        internal int CaPackagesCount { get; }

        internal bool IsLastUeEventRemoved { get; }

        internal bool IsLastAlEventRemoved { get; }

        internal int IgnoredPackagesCount { get; }

        internal bool IsDoublesRemoved { get; }

        internal IEnumerable<string> DoublesSignatures { get; }

        internal IEnumerable<ProxyPackageInfo> BadCodesPackages { get; } = new List<ProxyPackageInfo>();

        internal IEnumerable<string> Events { get; } = new List<string>();

        internal int EventsCount { get; }

        internal int UePackagesCountWithoutIgnored { get; }

        internal int AlPackagesCountWithoutIgnored { get; }

        internal int PackagesCountWithoutIgnored { get; }

        internal int AttemptPackagesCount { get; }

        internal bool IsAllEventsOrdered { get; }

        internal bool ContainsZeroCodePackage { get; }

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
            this.PackagesCountWithoutAlCaUe =
                this.PackagesCount - (this.AlPackagesCount + this.UePackagesCount + this.CaPackagesCount);
            this.AttemptPackagesCount = attemptPackagesCount;
            this.SdkVersionCount = sdkVersionCount;
            this.IgnoredPackagesCount =
                (this.IsLastAlEventRemoved ? 1 : 0)
                + (this.IsLastUeEventRemoved ? 1 : 0)
                + this.DoublesSignatures.Count()
                + this.AttemptPackagesCount
                + this.BadCodesPackages.Count();

            this.Events = events;

            this.EventsCount = events.Where(e => !e.Contains("err") && !e.Contains("ign")).Count();

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
    }
}
