namespace TestplanPackageCounter.Packages.Content.V1.Events
{
    using Newtonsoft.Json;
    using System.ComponentModel;
    using TestplanPackageCounter.General;
    using TestplanPackageCounter.Packages.Content.General;

    public class DiEvent : AbstractSdkEventV1, IHasTimestamp
    {
        [JsonProperty("manufacturer")]
        [DefaultValue(Constants.NotExistsString)]
        public string Manufacturer { get; private set; }

        [JsonProperty("model")]
        [DefaultValue(Constants.NotExistsString)]
        public string Model { get; private set; }

        [JsonProperty("deviceVersion")]
        [DefaultValue(Constants.NotExistsString)]
        public string DeviceVersion { get; private set; }

        [JsonProperty("screenResolution")]
        [JsonRequired]
        public string ScreenResolution { get; private set; }

        [JsonProperty("screenDpi")]
        [DefaultValue(null)]
        public int? ScreenDpi { get; private set; }

        [JsonProperty("deviceOS")]
        [DefaultValue(Constants.NotExistsString)]
        public string DeviceOs { get; private set; }

        [JsonProperty("odin")]
        [JsonRequired]
        public string Odin { get; private set; }

        [JsonProperty("idfa")]
        [DefaultValue(Constants.NotExistsString)]
        public string Idfa { get; private set; }

        [JsonProperty("idfv")]
        [DefaultValue(Constants.NotExistsString)]
        public string Idfv { get; private set; }

        [JsonProperty("d2dUdid")]
        [JsonRequired]
        public string D2DUdid { get; private set; }

        [JsonProperty("imei")]
        [DefaultValue(Constants.NotExistsString)]
        public string Imei { get; private set; }

        [JsonProperty("androidId")]
        [DefaultValue(Constants.NotExistsString)]
        public string AndroidId { get; private set; }

        [JsonProperty("advertisingId")]
        [DefaultValue(Constants.NotExistsString)]
        public string AdvertisingId { get; private set; }

        [JsonProperty("timeZoneOffset")]
        [JsonRequired]
        public int TimeZoneOffset { get; private set; }

        [JsonProperty("inch")]
        [DefaultValue(null)]
        public float? Inch { get; private set; }

        [JsonProperty("macWifi")]
        [DefaultValue(Constants.NotExistsString)]
        public string MacWiFi { get; private set; }

        [JsonProperty("timestamp")]
        [JsonRequired]
        public ulong? Timestamp { get; private set; }

        [JsonProperty("isLimitAdTrackingEnabled")]
        [DefaultValue(null)]
        public bool? IsLimitAdTrackingEnabled { get; private set; }

        [JsonProperty("isRooted")]
        [JsonRequired]
        public int IsRooted { get; private set; }
    }
}
