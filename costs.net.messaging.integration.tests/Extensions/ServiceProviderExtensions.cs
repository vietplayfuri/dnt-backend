namespace costs.net.messaging.integration.tests.Extensions
{
    using System;

    public static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException();
            }

            return (T) serviceProvider.GetService(typeof(T));
        }
    }
}