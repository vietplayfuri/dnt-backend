namespace costs.net.integration.tests.Browser
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class BrowserShim
    {
        private readonly HttpClient _client;

        public BrowserShim(HttpClient client)
        {
            _client = client;
        }

        private static RequestModifier ApplyModifier(Action<RequestModifier> action, HttpRequestMessage req, string url)
        {
            var modifier = new RequestModifier();
            action(modifier);
            modifier.Apply(req, url);
            return modifier;
        }

        public async Task<BrowserResponse> Put(string url, Action<RequestModifier> action = null)
        {
            return await HttpRequest(url, HttpMethod.Put, action);
        }

        public async Task<BrowserResponse> Get(string url, Action<RequestModifier> action = null)
        {
            return await HttpRequest(url, HttpMethod.Get, action);
        }

        private async Task<BrowserResponse> HttpRequest(string url, HttpMethod httpMethod, Action<RequestModifier> action)
        {
            var req = new HttpRequestMessage(httpMethod, url);
            if (action != null)
            {
                ApplyModifier(action, req, url);
            }

            var result = await _client.SendAsync(req);
            return new BrowserResponse(result);
        }

        public async Task<BrowserResponse> Patch(string url, Action<RequestModifier> action)
        {
            return await HttpRequest(url, new HttpMethod("PATCH"), action);
        }

        public async Task<BrowserResponse> Delete(string url, Action<RequestModifier> action)
        {
            return await HttpRequest(url, HttpMethod.Delete, action);
        }

        public async Task<BrowserResponse> Post(string url, Action<RequestModifier> action)
        {
            return await HttpRequest(url, HttpMethod.Post, action);
        }
    }
}
