using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using TestplanPackageCounter.Packages.Content.V2;

namespace TestplanPackageCounter.Counter
{
    internal class PackageComparerV2 : IEqualityComparer<ProxyPackageInfoV2>
    {
        public bool Equals(ProxyPackageInfoV2 x, ProxyPackageInfoV2 y)
        {
            if (x.RequestUrl == null || y.RequestUrl == null)
            {
                return false;
            }

            NameValueCollection firstParamsUrl =
                HttpUtility.ParseQueryString(new UriBuilder(x.RequestUrl).Query);
            NameValueCollection secondParamsUrl =
                HttpUtility.ParseQueryString(new UriBuilder(y.RequestUrl).Query);

            bool signaturesAreEquals =
                firstParamsUrl["s"] != null
                && firstParamsUrl["s"] == secondParamsUrl["s"];

            if (signaturesAreEquals)
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(ProxyPackageInfoV2 obj)
        {
            return 0;
        }
    }
}
