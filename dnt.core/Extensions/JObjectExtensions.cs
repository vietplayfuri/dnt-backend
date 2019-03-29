namespace dnt.core.Extensions
{
    using System;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    public static class JObjectExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="propertyName"></param>
        /// <param name="startsWith">Meaning the item to search for <see cref="string.StartsWith{string}"/><see cref="propertyName"/> parameter. 
        ///     Otherwise, an exact literal case-insensitive match comparison is done.</param>
        /// <param name="isArray"></param>
        /// <returns></returns>
        public static string GetPropertyValue(this JObject item, string propertyName, bool startsWith = true, bool isArray = false)
        {
            var properties = item.Properties();
            JProperty property;

            if (startsWith)
            {
                propertyName = propertyName.ToLowerInvariant();
                property = properties.FirstOrDefault(p => p.Name.ToLowerInvariant().StartsWith(propertyName));
            }
            else
            {
                property = properties.FirstOrDefault(p => string.Compare(p.Name, propertyName, StringComparison.CurrentCultureIgnoreCase) == 0);
            }
            var result = isArray ? property?.Value?.FirstOrDefault()?.ToString() : property?.Value?.ToString();

            return result ?? string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="propertyName"></param>
        /// <param name="startsWith">Meaning the item to search for <see cref="string.StartsWith{string}"/><see cref="propertyName"/> parameter. 
        ///     Otherwise, an exact literal case-insensitive match comparison is done.</param>
        /// <param name="isArray"></param>
        /// <returns></returns>
        public static T GetPropertyObject<T>(this JObject item, string propertyName, bool startsWith = true)
        {
            var properties = item.Properties();
            JProperty property;

            if (startsWith)
            {
                propertyName = propertyName.ToLowerInvariant();
                property = properties.FirstOrDefault(p => p.Name.ToLowerInvariant().StartsWith(propertyName));
            }
            else
            {
                property = properties.FirstOrDefault(p => string.Compare(p.Name, propertyName, StringComparison.CurrentCultureIgnoreCase) == 0);
            }
            if (property == null)
            {
                return default(T);
            }

            return property.Value.ToObject<T>();
        }
    }
}
