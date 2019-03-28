namespace dnt.core.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class DictionaryExtensions
    {
        public static TVal GetOrDefault<TKey, TVal>(this Dictionary<TKey, TVal> dictionary, TKey key, Func<TKey, TVal> defaultValue)
        {
            TVal value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }
            return defaultValue(key);
        }

        public static IEnumerable<TVal> GetOrEmpty<TKey, TVal>(this Dictionary<TKey, List<TVal>> dictionary, TKey key)
        {
            List<TVal> value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }
            return Enumerable.Empty<TVal>();
        }

        public static IEnumerable<TVal> GetOrEmpty<TKey, TVal>(this Dictionary<TKey, IEnumerable<TVal>> dictionary, TKey key)
        {
            IEnumerable<TVal> value;
            if (dictionary.TryGetValue(key, out value))
            {
                return value;
            }
            return Enumerable.Empty<TVal>();
        }

        public static Dictionary<string, dynamic> RemoveJArray(this IDictionary<string, dynamic> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            var modifiedProductionDetails = new Dictionary<string, dynamic>();
            foreach (var item in dictionary)
            {
                modifiedProductionDetails[item.Key] = item.Value;
                if (dictionary[item.Key] != null && dictionary[item.Key].GetType() == typeof(JArray))
                {
                    var newThing = (JArray)modifiedProductionDetails[item.Key];
                    modifiedProductionDetails[item.Key] = newThing.ToObject<List<dynamic>>();
                }
            }

            return modifiedProductionDetails;
        }

        public static T ToModel<T>(this Dictionary<string, dynamic> data)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None
            }));
        }
    }
}
