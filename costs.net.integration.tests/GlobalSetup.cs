namespace costs.net.integration.tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using api.Extensions;
    using Autofac;
    using dataAccess;
    using dataAccess.Entity;
    using Microsoft.Extensions.Configuration;
    using Npgsql;
    using NUnit.Framework;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Serilog;
    using Serilog.Core;

    [SetUpFixture]
    public class GlobalSetup
    {
        private string _adminConnectionString;
        private IConfigurationRoot _configuration;
        private string _dbRestoreFilePath;
        private string _dbRestoreProcess;
        private string _dbRestoreArguments;
        private string _dbName;
        private string _dbHostName;
        private int _dbPort;
        private string DisconnectClientsSql => $"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{_dbName}'";
        private EFContext _efContext;
        private Logger _logger;
        private string _connectionString;
        private Guid _costAdminUserId;


        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddEnvironmentVariables();
            _configuration = builder.Build();

            _logger = new LoggerConfiguration().ReadFrom.Configuration(_configuration).CreateLogger();

            _logger.Information("Running GlobalSetup (this should only happen once!)");

            _connectionString = _configuration.GetValue<string>("Data:DatabaseConnection:ConnectionString");
            _adminConnectionString = _configuration.GetValue<string>("Data:DatabaseConnection:ConnectionStringAdmin");

            _dbRestoreProcess = _configuration.GetValue<string>("AppSettings:DbRestoreProcess");
            _dbRestoreArguments = _configuration.GetValue<string>("AppSettings:DbRestoreArguments");
            _dbRestoreFilePath = _configuration.GetValue<string>("AppSettings:DbRestoreFilePath");

            _costAdminUserId = Guid.Parse(_configuration.GetValue<string>("AppSettings:CostsAdminUserId"));

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance<IConfiguration>(_configuration);
            containerBuilder.RegisterInstance(new Mock<ILoggerFactory>().Object);

            // Add db stuff
            RegisterDataAccess(containerBuilder);

            var container = containerBuilder.Build();

            _efContext = container.Resolve<EFContext>();
            var dbConnection = (NpgsqlConnection)_efContext.Database.GetDbConnection();
            _dbName = dbConnection.Database;
            _dbHostName = dbConnection.Host;
            _dbPort = dbConnection.Port;

            try
            {
                CreateDb().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Error while creating db. Dir: {TestContext.CurrentContext.WorkDirectory} Error: {e.Message} {e.StackTrace}");
                throw;
            }

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCaseExceptDictionaryResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
        }

        private void RegisterDataAccess(ContainerBuilder builder)
        {
            var dbContextBuilder = new DbContextOptionsBuilder<EFContext>();
            dbContextBuilder.UseNpgsql(_connectionString);
            var efContextOptions = new EFContextOptions
            {
                DbContextOptions = dbContextBuilder.Options,
                SystemUserId = _costAdminUserId
            };
            builder.RegisterInstance(efContextOptions);
            builder.RegisterModule<DataAccessModule>();
        }

        private async Task CreateDb()
        {
            _logger.Information($"Creating DB '{_dbName}' on {_adminConnectionString}");

            using (var connection = new NpgsqlConnection(_adminConnectionString))
            {
                await connection.OpenAsync();

                //Disconnect all active connections from test DB
                using (var command = new NpgsqlCommand(DisconnectClientsSql, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                //Drop and create test DB
                using (var command = new NpgsqlCommand($"drop database if exists {_dbName};", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                using (var command = new NpgsqlCommand($"create database {_dbName};", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }

            RestoreDb();
            await CreateProject();
        }

        private void RestoreDb()
        {
            var workDir = TestContext.CurrentContext.WorkDirectory;

            _dbRestoreFilePath = _dbRestoreFilePath
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace("{workDir}", $"{workDir}{Path.DirectorySeparatorChar}");

            _dbRestoreArguments = _dbRestoreArguments
                .Replace("{filePath}", _dbRestoreFilePath)
                .Replace("{dbName}", _dbName)
                .Replace("{hostName}", _dbHostName)
                .Replace("{dbPort}", _dbPort.ToString());

            _logger.Information($"Restoring Postgres DB {_dbName}");

            var processResult = StartProcess(_dbRestoreProcess, _dbRestoreFilePath, _dbRestoreArguments);

            _logger.Information($"Flyway executed without error, exit code {processResult.Item1} stdout {processResult.Item2}.");
        }

        private static Tuple<int, string> StartProcess(string processName, string filePath, string arguments)
        {
            var directoryInfo = new FileInfo(filePath).Directory;
            if (directoryInfo == null)
            {
                throw new IOException($"Couldn't find directory of file {filePath}");
            }
            var directoryName = directoryInfo.FullName;

            var processInfo = new ProcessStartInfo
            {
                FileName = processName,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = directoryName
            };

            Process process;
            if ((process = Process.Start(processInfo)) == null)
            {
                throw new InvalidOperationException("Broken");
            }

            string stdOut;
            using (var stream = process.StandardOutput)
            {
                stdOut = stream.ReadToEnd();
            }

            process.WaitForExit();
            var exitCode = process.ExitCode;

            if (exitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Process failed: {process.StartInfo.FileName} {process.StartInfo.Arguments}. Stdout: {stdOut} Stderror: {process.StandardError.ReadToEnd()}");
            }

            return new Tuple<int, string>(exitCode, stdOut);
        }

        [OneTimeTearDown]
        public void BaseTearDown()
        {
            DropDb().Wait();
        }

        private async Task DropDb()
        {
            using (var connection = new NpgsqlConnection(_adminConnectionString))
            {
                await connection.OpenAsync();

                //Disconnect all active connections from test DB
                using (var command = new NpgsqlCommand(DisconnectClientsSql, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                //Drop and create test DB
                using (var command = new NpgsqlCommand($"drop database if exists {_dbName};", connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task CreateProject()
        {
            var user = await _efContext.CostUser.FirstOrDefaultAsync(u => u.Email == Constants.AdstreamUserEmail);
            var campaignDictionary = await _efContext.Dictionary
                .Include(d => d.DictionaryEntries)
                .Where(d => d.Name == "Campaign")
                .FirstAsync();

            var project = new Project
            {
                Advertiser = "Advertiser",
                CampaignId = campaignDictionary.DictionaryEntries.First().Id,
                CreatedById = user.Id,
                Created = DateTime.Now,
                GdamProjectId = "123456789",
                Modified = DateTime.Now,
                Name = "ProjectTest",
                SubBrand = "SubBrand",
                Product = "Product",
                ShortId = "r",
                AdCostNumber = "AdCostNumber",
                AgencyId = user.AgencyId
            };
            _efContext.Project.Add(project);
            await _efContext.SaveChangesAsync();
        }
    }
}