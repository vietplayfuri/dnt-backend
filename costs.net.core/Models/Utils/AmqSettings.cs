namespace dnt.core.Models.Utils
{
    public class AmqSettings
    {
        public string AmqHost { get; set; }
        public string AmqA5Queue { get; set; }
        public string PurchaseOrderQueue { get; set; }
        public string AmqHostExternal { get; set; }
        public string BatchUpdateRequest { get; set; }
        public string XmgErrorQueue { get; set; }
        public string A5UserLoginQueue { get; set; }

        /// <summary>
        ///     Number of retries to reconnect to AMQ before giving up. 0 means no limit
        /// </summary>
        public int MaxConnectRetries { get; set; }

        /// <summary>
        ///     Interval between attempts to reconnect in milliseconds
        /// </summary>
        public int ReconnectInterval { get; set; }

        /// <summary>
        ///     Overrides broker's idle timeout if specified value is greater than 0
        /// </summary>
        public uint RemoteIdleTimeout { get; set; }
    }
}