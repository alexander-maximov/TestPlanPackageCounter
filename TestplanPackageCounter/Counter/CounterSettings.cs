namespace TestplanPackageCounter.Counter
{
    using TestplanPackageCounter.General;

    internal class CounterSettings
    {
        internal string PathToTestplan { get; }

        internal string OutcomingPath { get; }

        internal string PathToResults { get; }

        internal bool RewriteTestplan { get; }

        internal bool IgnoreBadUe { get; }

        internal bool IgnoreLastAl { get; }

        internal bool IgnoreUserIdentificationPackages { get; }

        internal bool IgnoreBadCodePackages { get; }

        internal bool CalculatePackagesWithMaxUe { get; }

        internal bool CalculatePackagesWithMaxCa { get; }

        internal bool FillWithDefaultParams { get; }

        internal bool FillMissingTestPackagesCount { get; }

        internal bool WriteToCsv { get; }

        internal SdkVersions SdkVersion { get; }

        internal bool SortOnly { get; }

        public CounterSettings(
            string pathToTestplan,
            string outcomingPath,
            string pathToResults,
            SdkVersions sdkVersion,
            bool rewriteTestplan = true,
            bool ignoreBadUe = true,
            bool ignoreLastAl = false,
            bool ignoreUserIdentification = true,
            bool ignoreBadCodePackages = true,
            bool calculateWithMaxUe = true,
            bool calculateWithMaxCa = true,
            bool fillWithDefaultParams = false,
            bool fillMissingTestPackagesCount = false,
            bool writeToCsv = true,
            bool sortOnly = false
        )
        {
            this.PathToTestplan = pathToTestplan;
            this.OutcomingPath = outcomingPath;
            this.PathToResults = pathToResults;
            this.SdkVersion = sdkVersion;
            this.RewriteTestplan = rewriteTestplan;
            this.IgnoreBadUe = ignoreBadUe;
            this.IgnoreLastAl = ignoreLastAl;
            this.IgnoreUserIdentificationPackages = ignoreUserIdentification;
            this.IgnoreBadCodePackages = ignoreBadCodePackages;
            this.CalculatePackagesWithMaxUe = calculateWithMaxUe;
            this.CalculatePackagesWithMaxCa = calculateWithMaxCa;
            this.FillWithDefaultParams = fillWithDefaultParams;
            this.FillMissingTestPackagesCount = fillMissingTestPackagesCount;
            this.WriteToCsv = writeToCsv;
            this.SortOnly = sortOnly;
        }
    }
}
