using System.Collections.Generic;
using System.Linq;

namespace TestplanPackageCounter.UglyCode.PackagesEnumerator
{
    internal class TestPackagesData
    {
        internal int OriginalPackagesCount { get; set; }

        internal int PackagesCount { get; set; }

        internal int PackagesCountWithoutUeAndAl { get; set; }

        internal int UePackagesCount { get; set; }

        internal int AlPackagesCount { get; set; }

        internal bool LastUeEventRemoved { get; set; }

        internal bool LastAlEventRemoved { get; set; }

        internal int IgnoredPackagesCount { get; set; }

        internal bool DoublesRemoved { get; set; }

        internal List<string> DoublesSignatures { get; set; }

        internal int UePackagesCountWithoutIgnored { get; set; }

        internal int AlPackagesCountWithoutIgnored { get; set; }

        internal int AttemptPackagesCount { get; set; }

        internal TestPackagesData(
            int originalPackagesCount,
            int packagesCount,
            int alPackagesCount,
            int uePackagesCount,
            int attemptPackagesCount,
            bool lastAlRemoved,
            bool lastUeRemoved,
            List<string> doublesSignatures
        )
        {
            this.OriginalPackagesCount = originalPackagesCount;
            this.PackagesCount = packagesCount;
            this.AlPackagesCount = alPackagesCount;
            this.UePackagesCount = uePackagesCount;
            this.LastAlEventRemoved = lastAlRemoved;
            this.LastUeEventRemoved = lastUeRemoved;
            this.DoublesSignatures = doublesSignatures;
            this.DoublesRemoved = this.DoublesSignatures.Any();
            this.IgnoredPackagesCount =
                (this.LastAlEventRemoved ? 1 : 0)
                + (this.LastUeEventRemoved ? 1 : 0)
                + this.DoublesSignatures.Count();
            this.AlPackagesCountWithoutIgnored =
                this.LastAlEventRemoved ? this.AlPackagesCount - 1 : this.AlPackagesCount;
            this.UePackagesCountWithoutIgnored =
                this.LastUeEventRemoved ? this.UePackagesCount - 1 : this.UePackagesCount;
            this.PackagesCountWithoutUeAndAl =
                this.PackagesCount - (this.AlPackagesCount + this.UePackagesCount);
            this.AttemptPackagesCount = attemptPackagesCount;
        }
    }
}
