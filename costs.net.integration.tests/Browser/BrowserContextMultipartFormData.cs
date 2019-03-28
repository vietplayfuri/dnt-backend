namespace costs.net.integration.tests.Browser
{
    using System;

    public class BrowserContextMultipartFormData
    {
        private readonly Action<BrowserContextMultipartFormDataConfigurator> _configuration;

        public BrowserContextMultipartFormData(Action<BrowserContextMultipartFormDataConfigurator> configuration)
        {
            _configuration = configuration;
        }

        public BrowserContextMultipartFormDataConfigurator GetConfig()
        {
            var browserContextMultipartFormDataConfigurator = new BrowserContextMultipartFormDataConfigurator();
            _configuration.Invoke(browserContextMultipartFormDataConfigurator);
            return browserContextMultipartFormDataConfigurator;
        }
    }
}