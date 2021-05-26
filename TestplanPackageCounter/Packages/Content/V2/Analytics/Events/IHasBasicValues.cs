using TestplanPackageCounter.Packages.Content.V2.Analytics.Events.Entries;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public interface IHasBasicValues
    {
        IeEntry[] InExperiments { get; }
    }
}
