using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TestplanPackageCounter.Packages.Content.V2;
using TestplanPackageCounter.Packages.Content.V2.Analytics;
using TestplanPackageCounter.Packages.Content.V2.Anticheat;
using TestplanPackageCounter.Packages.Content.V2.SdkVersion;
using TestplanPackageCounter.Packages.Content.V2.UserIdentification;
using TestplanPackageCounter.Packages.Converters.General;

namespace TestplanPackageCounter.Packages.Converters.V2
{
    public class RequestJsonConverterV2 : EntryConverter
    {
        private readonly Type _requestJsonType = typeof(RequestJsonContainerV2);

        private Dictionary<string, Type> _typeDictionary =
            new Dictionary<string, Type>()
            {
                { "attempt", typeof(UserIdentificationRequest) },
                { "platform", typeof(AnticheatRequest) },
                { "reports", typeof(AnalyticsRequest) },
                { "sdkVersion", typeof(SdkVersionRequest) }
            };

        public override bool CanConvert(Type objectType) =>
            objectType == this._requestJsonType;

        private protected override Type GetRuntimeType(JObject jsonObject, Type _)
        {
            List<string> propertyNames =
                jsonObject.Properties().Select(property => property.Name).ToList();

            Type definedType = this._typeDictionary.First(
                typeName => propertyNames.Contains(typeName.Key)
            ).Value;

            return definedType;
        }
    }
}
