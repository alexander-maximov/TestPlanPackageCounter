namespace TestplanPackageCounter.Counter
{
    using TestplanPackageCounter.General;

    internal class CounterSettings
    {
        internal string PathToTestplan { get; }

        internal string OutcomingPath { get; }

        internal string PathToResults { get; }

        internal bool RewriteTestplan { get; }

        internal bool IgnoreUePackages { get; }

        internal bool IgnoreLastUe { get; }

        internal bool CalculatePackagesWithMaxUe { get; }

        internal bool FillWithDefaultParams { get; }

        internal bool WriteToCsv { get; }

        internal SdkVersions SdkVersion { get; }

        public CounterSettings(
            string pathToTestplan,
            string outcomingPath,
            string pathToResults,
            SdkVersions sdkVersion,
            bool rewriteTestplan,
            bool ignoreUePackages,
            bool ignoreLastUe,
            bool calculateWithMaxUe,
            bool fillWithDefaultParams,
            bool writeToCsv
        )
        {
            this.PathToTestplan = pathToTestplan;
            this.OutcomingPath = outcomingPath;
            this.PathToResults = pathToResults;
            this.SdkVersion = sdkVersion;
            this.RewriteTestplan = rewriteTestplan;
            this.IgnoreLastUe = ignoreLastUe;
            this.IgnoreUePackages = ignoreUePackages;
            this.CalculatePackagesWithMaxUe = calculateWithMaxUe;
            this.FillWithDefaultParams = fillWithDefaultParams;
            this.WriteToCsv = writeToCsv;
        }
    }
}
