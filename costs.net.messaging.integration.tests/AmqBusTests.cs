namespace costs.net.messaging.integration.tests
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using core.Messaging;
    using Extensions;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Module;
    using NUnit.Framework;
    using Serilog;
    using Stubs;
    
    [TestFixture]
    public class AmqBusTests
    {
        [Test]
        public async Task Listen_Always_Should_ConsumeMessageInCorrectOrder()
        {
            // Arrange
            var builder = new ContainerBuilder();
            var amqSettings = new TestAmqSettings();
            IOptions<TestAmqSettings> amqOptions = new OptionsWrapper<TestAmqSettings>(amqSettings);

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("appsettings.Development.json", true)
                .Build();

            configuration.GetSection("AmqSettings").Bind(amqSettings);
            
            builder.RegisterInstance<IConfiguration>(configuration);
            builder.RegisterInstance(amqSettings);
            builder.RegisterInstance(amqOptions);

            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            builder.RegisterInstance<ILogger>(logger);
            builder.RegisterInstance<IMessageStorage>(new MessageStorage());
            builder.RegisterModule<TestModule>();

            var serviceProvider = new AutofacServiceProvider(builder.Build());

            var sender = serviceProvider.GetService<IMessageSender>();
            var receiver = serviceProvider.GetService<IMessageReceiver>();
            var messageStorage = serviceProvider.GetService<IMessageStorage>();

            const int count = 5;

            // Act
            await sender.ActivateAsync();

            for (var i = 0; i < count; ++i)
            {
                await sender.SendMessage(new TestMessage { Id = i }, amqSettings.QueueName);
            }
            // Now start receiver and check
            await receiver.ActivateAsync();

            while (messageStorage.Messages.Count < count)
            {
                Thread.Sleep(100);
            }

            //Assert
            messageStorage.Messages.Select(m => m.Id).ShouldAllBeEquivalentTo(Enumerable.Range(0, count), config => config.WithStrictOrdering());
        }
    }
}