namespace TestplanPackageCounter
{
    internal class CounterSettings
    {
        internal string PathToTestplan { get; }

        internal string OutcomingPath { get; }

        internal string PathToResults { get; }

        internal bool RewriteTestplan { get; }

        internal bool IgnoreUePackages { get; }

        internal bool IgnoreAlPackages { get; }

        internal bool CalculatePackagesWithMaxUe { get; }

        internal bool FillDefaultParams { get; }

        public CounterSettings(
            string pathToTestplan,
            string outcomingPath,
            string pathToResults,
            bool rewriteTestplan,
            bool ignoreUePackages,
            bool ignoreAlPackages,
            bool calculateWithMaxUe,
            bool fillDefaultParams
        )
        {
            this.PathToTestplan = pathToTestplan;
            this.OutcomingPath = outcomingPath;
            this.PathToResults = pathToResults;
            this.RewriteTestplan = rewriteTestplan;
            this.IgnoreAlPackages = ignoreAlPackages;
            this.IgnoreUePackages = ignoreUePackages;
            this.CalculatePackagesWithMaxUe = this.IgnoreUePackages ? false : calculateWithMaxUe;
            this.FillDefaultParams = fillDefaultParams;
        }
    }
}
