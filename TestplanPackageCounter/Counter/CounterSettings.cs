namespace TestplanPackageCounter.Counter
{
    using TestplanPackageCounter.General;

    internal static class CounterSettings
    {
        internal static string PathToTestplan { get; set; }

        internal static string OutcomingPath { get; set; }

        internal static string PathToResults { get; set; }

        internal static bool RewriteTestplan { get; set; } = true;

        internal static bool IgnoreBadUe { get; set; } = false;

        internal static bool IgnoreLastAl { get; set; } = false;

        internal static bool IgnoreUserIdentificationPackages { get; set; } = true;

        internal static bool IgnoreBadCodePackages { get; set; } = true;

        internal static bool CalculatePackagesWithMaxUe { get; set; } = true;

        internal static bool CalculatePackagesWithMaxCa { get; set; } = false;

        internal static bool FillWithDefaultParams { get; set; } = false;

        internal static bool FillMissingTestPackagesCount { get; set; } = false;

        internal static bool WriteToCsv { get; set; } = true;

        internal static SdkVersions SdkVersion { get; set; } = SdkVersions.V2;

        internal static bool SortOnly { get; set; } = false;
    }
}
