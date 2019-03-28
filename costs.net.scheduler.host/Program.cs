namespace costs.net.scheduler.host
{
    using System;
    using System.Threading;
    using Microsoft.AspNetCore.Hosting;

    internal class Program
    {
        // TODO: Add logger here
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var host = new WebHostBuilder().UseStartup(typeof(Startup)).Build();

            //This is unbelievably complex because .NET Core Console.ReadLine() does not block in a docker container...!
            using (var cts = new CancellationTokenSource())
            {
                Action shutdown = () =>
                {
                    if (!cts.IsCancellationRequested)
                    {
                        Console.WriteLine("Application is shutting down...");
                        cts.Cancel();
                    }
                };

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    shutdown();
                    // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                    eventArgs.Cancel = true;
                };

                host.RunAsync(cts.Token).Wait();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("UnhandledException! Application is shutting down...", e);
        }
    }
}