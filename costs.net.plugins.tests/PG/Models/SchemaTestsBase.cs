namespace costs.net.plugins.tests.PG.Models
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using NUnit.Framework;

    public abstract class SchemaTestsBase
    {
        protected string BasePath = AppContext.BaseDirectory;

        protected readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        [OneTimeSetUp]
        public void Initialize()
        {
            SerializerSettings.Converters.Add(new IsoDateTimeConverter
            {
                DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff'Z'"
            });
        }

        protected string GetSchemaValidationAssertionReason(IEnumerable<string> errors) => 
            $"because there shouldn't be any errors in schema but found:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}{Environment.NewLine}";

        protected string GetPath(string path) => $"{BasePath}{Path.DirectorySeparatorChar}{path}";
    }
}