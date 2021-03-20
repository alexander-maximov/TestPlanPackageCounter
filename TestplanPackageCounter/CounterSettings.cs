namespace TestplanPackageCounter
{
    internal class CounterSettings
    {
        internal string PathToTestplan { get; }

        internal string PathToResults { get; }

        internal bool RewriteTestplan { get; }

        internal bool IgnoreUePackages { get; }

        internal bool IgnoreAlPackages { get; }

        internal bool CalculatePackagesWithMaxUe { get; }

        public CounterSettings(
            string pathToTestplan,
            string pathToResults,
            bool rewriteTestplan,
            bool ignoreUePackages,
            bool ignoreAlPackages,
            bool calculateWithMaxUe
        )
        {
            this.PathToTestplan = pathToTestplan;
            this.PathToResults = pathToResults;
            this.RewriteTestplan = rewriteTestplan;
            this.IgnoreAlPackages = ignoreAlPackages;
            this.IgnoreUePackages = ignoreUePackages;
            this.CalculatePackagesWithMaxUe = this.IgnoreUePackages ? false : calculateWithMaxUe;
        }
    }
}
