using System;
using System.Threading.Tasks;
using costs.net.core.ExternalResource.Paperpusher;

namespace costs.net.tests.common.Stubs
{
    public class PaperpusherClientStub : IPaperpusherClient
    {
        public Task<bool> SendMessage<T>(Guid messageId, T payload, string activityType) where T : class
        {
            return Task.FromResult(true);
        }

        public Task<bool> SendMessage<T>(T body) where T : class
        {
            return Task.FromResult(true);
        }
    }
}
