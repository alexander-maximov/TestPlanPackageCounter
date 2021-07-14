using Newtonsoft.Json;
using System.ComponentModel;
using TestplanPackageCounter.General;

namespace TestplanPackageCounter.Packages.Content.V2.Analytics.Events
{
    public class DiV2 : AbstractSdkEventV2
    {
        [JsonProperty("osVersion")]
        [DefaultValue(Constants.NotExistsString)]
        public string OsVersion { get; private set; }

        [JsonProperty("os")]
        [DefaultValue(Constants.NotExistsString)]
        public string Os { get; private set; }

        [JsonProperty("displayPpi")]
        [DefaultValue(null)]
        public uint? DisplayPpi { get; private set; }

        [JsonProperty("displayResolution")]
        [DefaultValue(Constants.NotExistsString)]
        public string DisplayResolution { get; private set; }

        [JsonProperty("displayDiagonal")]
        [DefaultValue(null)]
        public double? DisplayDiagonal { get; private set; }

        [JsonProperty("manufacturer")]
        [DefaultValue(Constants.NotExistsString)]
        public string Manufacturer { get; private set; }

        [JsonProperty("model")]
        [DefaultValue(Constants.NotExistsString)]
        public string Model { get; private set; }

        [JsonProperty("timeZoneOffset")]
        [DefaultValue(null)]
        public int? TimeZoneOffset { get; private set; }

        [JsonProperty("rooted")]
        [DefaultValue(null)]
        public int? Rooted { get; private set; }

        [JsonProperty("isLimitAdTrackingEnabled")]
        [DefaultValue(null)]
        public bool? IsLimitAdTrackingEnabled { get; private set; }

        [JsonProperty("userAgent")]
        [DefaultValue(Constants.NotExistsString)]
        public string UserAgent { get; private set; }

        [JsonProperty("idfv")]
        [DefaultValue(Constants.NotExistsString)]
        public string Idfv { get; private set; }

        [JsonProperty("idfa")]
        [DefaultValue(Constants.NotExistsString)]
        public string Idfa { get; private set; }

        [JsonProperty("androidId")]
        [DefaultValue(Constants.NotExistsString)]
        public string AndroidId { get; private set; }

        [JsonProperty("advertisingId")]
        [DefaultValue(Constants.NotExistsString)]
        public string AdvertisingId { get; private set; }

        [JsonProperty("uuid")]
        [DefaultValue(Constants.NotExistsString)]
        public string Uuid { get; private set; }

        [JsonProperty("instanceId")]
        [DefaultValue(Constants.NotExistsString)]
        public string InstanceId { get; private set; }

        [JsonProperty("installationSource")]
        [DefaultValue(Constants.NotExistsString)]
        public string InstallationSource { get; private set; }

        [JsonProperty("timestamp")]
        [DefaultValue(null)]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("isSandbox")]
        [DefaultValue(Constants.NotExistsString)]
        public string IsSandbox { get; private set; }
    }
}
