
namespace dnt.core.Services
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class HttpService : IHttpService
    {
        private readonly HttpClient _client = new HttpClient();

        public async Task<HttpResponseMessage> GetAsync(Uri url)
        {
            return await _client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(Uri url, HttpContent content)
        {
            return await _client.PostAsync(url, content);
        }
    }
}
