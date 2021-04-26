namespace TestplanPackageCounter.General
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class EscapedStringResolver : DefaultContractResolver
    {
        private static readonly Type StringType = typeof(string);

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            if (type == null)
            {
                return null;
            }

            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            // Attach an HtmlEncodingValueProvider instance to all string properties
            foreach (JsonProperty property in properties.Where(item => item.PropertyType == StringType))
            {
                PropertyInfo propertyInfo = type.GetProperty(property.UnderlyingName);

                if (propertyInfo != null)
                {
                    propertyInfo = propertyInfo.DeclaringType == type
                        ? propertyInfo
                        : propertyInfo.DeclaringType.GetProperty(propertyInfo.Name);

                    property.ValueProvider = new HtmlEncodingValueProvider(propertyInfo);
                }
            }

            return properties;
        }

        protected class HtmlEncodingValueProvider : IValueProvider
        {
            private readonly PropertyInfo _targetProperty;

            public HtmlEncodingValueProvider(PropertyInfo targetProperty) =>
                this._targetProperty = targetProperty;

            /// <summary>
            /// SetValue gets called by Json.Net during deserialization.
            /// The value parameter has the original value read from the JSON;
            /// target is the object on which to set the value.
            /// </summary>
            /// <param name="target"></param>
            /// <param name="value"></param>
            public void SetValue(object target, object value) =>
                this._targetProperty.SetValue(target, value == null ? null : Uri.UnescapeDataString((string)value));

            /// <summary>
            /// GetValue is called by Json.Net during serialization.
            /// The target parameter has the object from which to read the string;
            /// the return value is the string that gets written to the JSON
            /// </summary>
            /// <param name="target"></param>
            /// <returns></returns>
            public object GetValue(object target) =>
                throw new NotImplementedException();
        }
    }
}
