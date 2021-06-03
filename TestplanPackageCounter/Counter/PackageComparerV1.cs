using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents;

namespace TestplanPackageCounter.Counter
{
    internal class PackageComparerV1 : IEqualityComparer<ProxyPackageInfoV1>
    {
        public bool Equals(ProxyPackageInfoV1 x, ProxyPackageInfoV1 y)
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

        public int GetHashCode(ProxyPackageInfoV1 obj)
        {
            return 0;
        }
    }
}
