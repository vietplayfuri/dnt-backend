namespace costs.net.tests.common.Extensions
{
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class JsonTestReader
    {
        public async Task<T> GetObject<T>(string filepath, bool isArray = false) where T : class, new()
        {
            var stringJson = await ReadAllLinesAsync(filepath);
            var result = JsonConvert.DeserializeObject<dynamic>(stringJson);
            JObject jsonObject;

            if (isArray)
            {
                jsonObject = ((JArray) result.list).First as JObject;
            }
            else
            {
                jsonObject = result as JObject;
            }
            return jsonObject?.ToObject<T>();
        }

        private static async Task<string> ReadAllLinesAsync(string filepath)
        {
            using (var reader = File.OpenText(filepath))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}