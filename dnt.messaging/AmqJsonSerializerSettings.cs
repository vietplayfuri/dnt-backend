namespace costs.net.messaging
{
    using Newtonsoft.Json;

    public static class AmqJsonSerializerSettings
    {
        public static JsonSerializerSettings Settings => new JsonSerializerSettings
        {
            // Allows deserializing to the actual runtime type
            TypeNameHandling = TypeNameHandling.All,
            // In a version resilient way
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        };
    }
}