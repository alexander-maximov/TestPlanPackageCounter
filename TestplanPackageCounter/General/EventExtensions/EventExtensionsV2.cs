using System.Collections.Generic;
using System.Linq;
using TestplanPackageCounter.Packages.Content.V2;
using TestplanPackageCounter.Packages.Content.V2.Analytics;
using TestplanPackageCounter.Packages.Content.V2.Analytics.Events;

namespace TestplanPackageCounter.General.EventExtensions
{
    public static class EventExtensionsV2
    {
        public static IEnumerable<ReportsData> AllReportsDatas(this IEnumerable<ProxyPackageInfoV2> proxyPackages)
        {
            List<ReportsData> reportsDataList = new List<ReportsData>();

            foreach (ProxyPackageInfoV2 package in proxyPackages)
            {
                if (package.RequestJson is AnalyticsRequest analyticsRequest)
                {
                    reportsDataList.AddRange(analyticsRequest.Reports);
                }
            }

            return reportsDataList;
        }

        public static IEnumerable<PackagesData> AllPackagesDatas(this IEnumerable<ReportsData> reportsDatas)
        {
            List<PackagesData> packagesDataList = new List<PackagesData>();

            foreach (ReportsData reportsData in reportsDatas)
            {
                packagesDataList.AddRange(reportsData.Packages);
            }

            return packagesDataList;
        }

        public static IEnumerable<PackagesData> AllPackagesData(this IEnumerable<ProxyPackageInfoV2> proxyPackages) =>
            proxyPackages.AllReportsDatas().AllPackagesDatas();

        public static IEnumerable<AbstractSdkEventV2> AllEvents(this IEnumerable<PackagesData> packagesDatas)
        {
            List<AbstractSdkEventV2> abstractSdkEventList = new List<AbstractSdkEventV2>();

            foreach (PackagesData packagesData in packagesDatas)
            {
                abstractSdkEventList.AddRange(packagesData.Events);
            }

            return abstractSdkEventList;
        }

        public static IEnumerable<AbstractSdkEventV2> AllEvents(this IEnumerable<ReportsData> reportsDatas) =>
            reportsDatas.AllPackagesDatas().AllEvents();

        public static IEnumerable<AbstractSdkEventV2> AllEvents(this IEnumerable<ProxyPackageInfoV2> packages) =>
            packages.AllPackagesData().AllEvents();

        public static IEnumerable<AbstractSdkEventV2> AllEvents(this ProxyPackageInfoV2 package)
        {
            IEnumerable<ProxyPackageInfoV2> singlePackage = new List<ProxyPackageInfoV2>() { package };
            return singlePackage.AllPackagesData().AllEvents();
        }

        public static IEnumerable<T> AllEventsOfType<T>(this IEnumerable<ProxyPackageInfoV2> proxyPackages)
        {
            return proxyPackages.AllReportsDatas().AllEventsOfType<T>().ToList();
        }

        public static IEnumerable<T> AllEventsOfType<T>(this IEnumerable<ReportsData> reportsDatas)
        {
            return reportsDatas.AllPackagesDatas().AllEventsOfType<T>().ToList();
        }

        public static IEnumerable<T> AllEventsOfType<T>(this IEnumerable<PackagesData> packagesDatas)
        {
            return packagesDatas.AllEvents().OfType<T>().ToList();
        }

        public static PackagesData FindPackagesData(
            this AbstractSdkEventV2 sdkEvent, 
            IEnumerable<PackagesData> packagesDatas
        )
        {
            foreach (PackagesData packagesData in packagesDatas)
            {
                AbstractSdkEventV2[] abstractSdkEvents = packagesData.Events;

                if (abstractSdkEvents.Contains(sdkEvent))
                {
                    return packagesData;
                }
            }

            return null;
        }

        public static ReportsData FindReportsData(
            this PackagesData packagesData,
            IEnumerable<ReportsData> reportsDatas
        )
        {
            foreach (ReportsData reportsData in reportsDatas)
            {
                PackagesData[] packagesDatas = reportsData.Packages;

                if (packagesDatas.Contains(packagesData))
                {
                    return reportsData;
                }
            }

            return null;
        }

        public static ReportsData FindReportsData(
            this AbstractSdkEventV2 sdkEvent,
            IEnumerable<ReportsData> reportsDatas
        )
        {
            IEnumerable<PackagesData> packagesDatas = reportsDatas.AllPackagesDatas();

            PackagesData packagesData = sdkEvent.FindPackagesData(packagesDatas);

            return packagesData.FindReportsData(reportsDatas);
        }

        public static ProxyPackageInfoV2 FindPackage(
            this ReportsData reportsData,
            IEnumerable<ProxyPackageInfoV2> packages
        )
        {
            foreach (ProxyPackageInfoV2 package in packages)
            {
                if (package.RequestJson is AnalyticsRequest analyticsRequest)
                {
                    ReportsData[] reportsDatas = analyticsRequest.Reports;

                    if (reportsDatas.Contains(reportsData))
                    {
                        return package;
                    }
                }
            }

            return null;
        }

        public static ProxyPackageInfoV2 FindPackage(
            this PackagesData packagesData,
            IEnumerable<ProxyPackageInfoV2> packages
        )
        {
            IEnumerable<ReportsData> reportsDatas = packages.AllReportsDatas();

            ReportsData reportsData = packagesData.FindReportsData(reportsDatas);

            return reportsData.FindPackage(packages);
        }

        public static ProxyPackageInfoV2 FindPackage(
            this AbstractSdkEventV2 sdkEvent,
            IEnumerable<ProxyPackageInfoV2> packages
        )
        {
            IEnumerable<PackagesData> packagesDatas = packages.AllPackagesData();

            PackagesData packagesData = sdkEvent.FindPackagesData(packagesDatas);

            return packagesData.FindPackage(packages);
        }
    }
}
