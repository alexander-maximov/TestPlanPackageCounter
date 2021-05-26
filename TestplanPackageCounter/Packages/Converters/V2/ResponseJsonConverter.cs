using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TestplanPackageCounter.Packages.Content.V2;
using TestplanPackageCounter.Packages.Content.V2.Anticheat;
using TestplanPackageCounter.Packages.Content.V2.SdkVersion;
using TestplanPackageCounter.Packages.Content.V2.UserIdentification;
using TestplanPackageCounter.Packages.Converters.General;

namespace TestplanPackageCounter.Packages.Converters.V2
{
    public class ResponseJsonConverter : EntryConverter
    {
        private readonly Type _responseBaseType = typeof(ResponseJsonContainerV2);
        private readonly Type _sdkVersionResponseType = typeof(SdkVersionResponse);

        private Dictionary<string, Type> _typeDictionary =
            new Dictionary<string, Type>()
            {
                { "retryAfter", typeof(RetryAfterResponse) },
                { "devtodevId", typeof(DevtodevIdResponse) },
                { "verificationResult", typeof(AnticheatResponse) }
            };

        public override bool CanConvert(Type objectType) =>
            objectType == this._responseBaseType;

        private protected override Type GetRuntimeType(JObject jsonObject, Type _)
        {
            List<string> propertyNames =
                jsonObject.Properties().Select(property => property.Name).ToList();

            Type definedType = this._typeDictionary.FirstOrDefault(
                typeName => propertyNames.Contains(typeName.Key)
            ).Value ?? this._sdkVersionResponseType;

            return definedType;
        }
    }
}
