using System;
using costs.net.scheduler.core;
using FluentScheduler;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Serilog;

namespace costs.net.scheduler.host
{
    using System.Threading;
    using System.Threading.Tasks;

    public class ConsoleAppRunner : IServer
    {
        private static readonly ILogger Logger = Log.ForContext<ConsoleAppRunner>();

        public ConsoleAppRunner()
        {
            Features = new FeatureCollection();            
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            StopScheduler();
        }

        public IFeatureCollection Features { get; private set; }

        public void Dispose()
        {
        }

        /// <summary>Start the server with an application.</summary>
        /// <param name="application">An instance of <see cref="T:Microsoft.AspNetCore.Hosting.Server.IHttpApplication`1" />.</param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TContext">The context associated with the application.</typeparam>
        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                //Actual program code starts here...
                try
                {
                    JobManager.Initialize(new CostJobRegistry());

                    Console.WriteLine("Ready...");
                    Logger.Information("costs.net.scheduler.host is ready...");

                    // <-- Keeps the program running - The Done property is a ManualResetEventSlim instance which gets set if someone terminates the program.
                    cancellationToken.WaitHandle.WaitOne();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "FluentScheduler.JobManager error!");
                    Console.Error.WriteLine(ex.ToString());
                    StopScheduler();
                }
            }, cancellationToken);
        }

        private static void StopScheduler()
        {
            // and last shut down the scheduler when you are ready to close your program
            Console.WriteLine("Exiting...");
            Logger.Information("Exiting costs.net.scheduler.host...");
            JobManager.Stop();
        }
    }
}
