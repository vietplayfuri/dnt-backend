namespace costs.net.messaging
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Apache.NMS;
    using Apache.NMS.ActiveMQ;
    using core.Messaging;
    using core.Models.Utils;
    using Serilog;

    public abstract class BaseBus : IBus
    {
        protected IConnection Connection { get; private set; }

        private bool _isDisposed;

        private IConnectionFactory _factory;

        private readonly ILogger _logger;

        private readonly int _reconnectInterval = 5000;

        protected AmqSettings AmqSettings { get; }

        public string HostName { get; }

        protected ISession Session { get; private set; }

        private IList<IDisposable> _disposableReources;

        protected BaseBus(string hostName, AmqSettings amqSettings, ILogger logger)
        {
            HostName = hostName;
            AmqSettings = amqSettings;
            _logger = logger;

            _factory = new ConnectionFactory(hostName, $"Adcosts-{Guid.NewGuid()}");
            _reconnectInterval = amqSettings.ReconnectInterval != 0 ? amqSettings.ReconnectInterval : _reconnectInterval;
            _disposableReources = new List<IDisposable>();
        }

        public async Task ActivateAsync()
        {
            await TryConnect();
            OnConnected();
        }

        private async Task<IBus> TryConnect()
        {
            var counter = 0;
            var connected = false;

            while (!connected && (AmqSettings.MaxConnectRetries == 0 || counter < AmqSettings.MaxConnectRetries))
            {
                try
                {
                    _logger.Information($"Connecting to AMQ {HostName}");

                    if (Connection == null || !Connection.IsStarted)
                    {
                        Connection = _factory.CreateConnection();
                        Connection.Start();
                        Connection.ExceptionListener += async ex => await ConnectionOnExceptionListener(ex);
                        Connection.ConnectionInterruptedListener += async () => await ConnectionOnConnectionInterruptedListener();
                    }

                    connected = true;
                    return this;
                }
                catch (Exception ex)
                {
                    if (ex is InvalidClientIDException && ex.Message.Contains("already connected"))
                    {
                        _logger.Error(ex, $"Attempting to re-connect to AMQ {HostName} when we have already connected!");
                    }
                    else
                    {
                        ++counter;
                        _logger.Error(
                            $"Failed to connect to AMQ {HostName}. Retries {counter} exception '{ex.ToString()}'");
                    }

                    DisposeResources();
                    await Task.Delay(_reconnectInterval);
                }
            }

            if (!connected)
            {
                throw new Exception($"Couldn't connect to AMQ {HostName}");
            }

            return null;
        }

        public void CreateSession()
        {
            // 3rd part: https://github.com/apache/activemq-nms-openwire/blob/master/src/main/csharp/Session.cs
            // Causing problems when disconnected multiple times, that's why method DisposeResources() was introduced
            if (Connection != null && Connection.IsStarted &&
                (Session == null || (Session)Session == null || !((Session)Session).Started))
            {
                Session = Connection.CreateSession();
            }
        }

        protected void AddDisposableResource(IDisposable resource)
        {
            _disposableReources.Add(resource);
        }

        private void DisposeResources()
        {
            try
            {
                foreach (var resource in _disposableReources)
                {
                    resource.Dispose();
                }

                _disposableReources = new List<IDisposable>();

                if (Session != null)
                {
                    Session.Close();
                    Session.Dispose();
                    Session = null;
                }

                if (Connection != null)
                {
                    if (Connection.IsStarted)
                    {
                        Connection.Close();
                    }

                    Connection.Dispose();
                    Connection = null;
                }

                _factory = new ConnectionFactory(HostName, $"Adcosts-{Guid.NewGuid()}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error when disposing resources.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // TODO: Are we really sure about this? Can you confirm and fix if needed/remove the comment?
            GC.SuppressFinalize(this);
        }

        protected virtual void OnConnected()
        {
            _logger.Information($"Connected to AMQ {HostName}");
        }

        protected static string GetClosedCallbackString(Exception error)
        {
            return $"Error {error}";
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Free other managed objects.
                DisposeResources();
            }

            // Free any own unmanaged objects.
            // Set large fields to null.
            Session = null;
            Connection = null;
            _factory = null;
            _isDisposed = true;
        }

        private async Task ReconnectAsync(Exception exception)
        {
            _logger.Error($"Session closed. {GetClosedCallbackString(exception)}");
            await Reconnect();
        }

        private async Task Reconnect()
        {
            DisposeResources();
            await TryConnect();
            OnConnected();
        }

        private async Task ReconnectAsync()
        {
            _logger.Error("Session closed. Connection Interrupted!");
            await Reconnect();
        }

        private Task ConnectionOnConnectionInterruptedListener()
        {
            return ReconnectAsync();
        }

        private Task ConnectionOnExceptionListener(Exception exception)
        {
            return ReconnectAsync(exception);
        }

        ~BaseBus()
        {
            Dispose(false);
        }
    }
}
