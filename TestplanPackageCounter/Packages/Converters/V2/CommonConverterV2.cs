using System;
using System.Collections.Generic;
using TestplanPackageCounter.Packages.Content.V2;
using TestplanPackageCounter.Packages.Content.V2.Analytics;
using TestplanPackageCounter.Packages.Content.V2.SdkVersion;
using TestplanPackageCounter.Packages.Converters.General;

namespace TestplanPackageCounter.Packages.Converters.V2
{
    public class CommonConverterV2 : EntryConverter
    {
        public override bool CanConvert(Type objectType) =>
            this._availableTypes.Contains(objectType);

        private readonly List<Type> _availableTypes = new List<Type>
        {
            typeof(ReportsData),
            typeof(PackagesData),
            typeof(SdkVersionRequest),
            typeof(ProxyPackageInfoV2),
            typeof(ExcludeEntry)
        };
    }
}
