namespace costs.net.integration.tests.Browser
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using dataAccess.Entity;
    using Newtonsoft.Json;

    public class RequestModifier
    {
        private readonly List<Action<HttpRequestMessage>> _modifiers = new List<Action<HttpRequestMessage>>();

        private string _url;

        public void User(CostUser user)
        {
            _modifiers.Add(m =>
            {
                var qChar = _url.Contains("?") ? "&" : "?";
                m.RequestUri = new Uri(_url + $"{qChar}$id$={user.GdamUserId}", UriKind.Relative);
            });
        }


        public void JsonBody(object request)
        {
            _modifiers.Add(m => m.Content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
        }

        public void Apply(HttpRequestMessage request, string uri)
        {
            _url = uri;

            foreach (var modifier in _modifiers)
            {
                modifier(request);
            }
        }

        public void MultiPartFormData(BrowserContextMultipartFormData browserContext)
        {
            var config = browserContext.GetConfig();
            if (!config.IsEmpty())
            {
                var content = new MultipartFormDataContent { { new StreamContent(config.File), config.Name, config.FileName } };
                _modifiers.Add(m => m.Content = content);
            }
        }
    }
}