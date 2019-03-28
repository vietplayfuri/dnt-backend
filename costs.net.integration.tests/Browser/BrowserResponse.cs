namespace costs.net.integration.tests.Browser
{
    using System;
    using System.Net;
    using System.Net.Http;

    public class BrowserResponse
    {
        public BrowserResponse(HttpResponseMessage message)
        {
            StatusCode = message.StatusCode;
            ContentType = message.Content.Headers.ContentType?.MediaType;
            Body = new ResponseBody(message.Content);
            RequestUri = message.RequestMessage.RequestUri;
            RequestMethod = message.RequestMessage.Method;
        }

        public HttpMethod RequestMethod { get; set; }

        public Uri RequestUri { get; set; }

        public HttpStatusCode StatusCode { get; private set; }

        public ResponseBody Body { get; private set; }

        public string ContentType { get; private set; }

        public class ResponseBody
        {
            public HttpContent Content { get; }

            public ResponseBody(HttpContent content)
            {
                Content = content;
            }

            public string AsString()
            {
                return Content.ReadAsStringAsync().Result;
            }
        }
    }
}