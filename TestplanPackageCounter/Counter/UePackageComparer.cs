﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TestplanPackageCounter.Packages.Content.V1.Events;
using TestplanPackageCounter.Packages.Content.V1.Events.UpperLevelEvents;

namespace TestplanPackageCounter.Counter
{
    internal class UePackageComparer : IEqualityComparer<ProxyPackageInfoV1>
    {
        public bool Equals(ProxyPackageInfoV1 x, ProxyPackageInfoV1 y)
        {
            try
            {
                if (x.RequestJson is LuData firstLuData && y.RequestJson is LuData secondLuData)
                {
                    if (firstLuData.LuEvents.Count == secondLuData.LuEvents.Count
                        && firstLuData.LuEvents.Count == 1
                    )
                    {
                        AbstractSdkEvent firstEvent = firstLuData.LuEvents.First().Value.Events.First().Value.First();
                        AbstractSdkEvent secondEvent = secondLuData.LuEvents.First().Value.Events.First().Value.First();

                        if (firstEvent is UeEvent firstUe && secondEvent is UeEvent secondUe)
                        {
                            NameValueCollection firstParamsUrl =
                                HttpUtility.ParseQueryString(new UriBuilder(x.RequestUrl).Query);
                            NameValueCollection secondParamsUrl =
                                HttpUtility.ParseQueryString(new UriBuilder(y.RequestUrl).Query);

                            bool paramsEquals = firstParamsUrl["s"] == secondParamsUrl["s"];

                            bool lengthEquals = firstUe.Length == secondUe.Length;
                            bool timestampEquals = firstUe.Timestamp == secondUe.Timestamp;
                            bool sessionIdEquals = firstUe.SessionId == secondUe.SessionId;

                            if (lengthEquals && timestampEquals && sessionIdEquals)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public int GetHashCode(ProxyPackageInfoV1 obj)
        {
            return 0;
        }
    }
}