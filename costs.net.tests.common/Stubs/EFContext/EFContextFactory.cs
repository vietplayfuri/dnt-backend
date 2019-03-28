namespace costs.net.tests.common.Stubs.EFContext
{
    using System;
    using dataAccess;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.EntityFrameworkCore.Storage.Internal;
    using Microsoft.Extensions.Logging;
    using Moq;

    public static class EFContextFactory
    {
        public static EFContext CreateInMemoryEFContext(Guid? systemUserId = null)
        {
            return CreateInMemoryEFContext(options => new EFContext(options, new Mock<ILoggerFactory>().Object), systemUserId);
        }

        public static EFContextTest CreateInMemoryEFContextTest(Guid? systemUserId = null)
        {
            return CreateInMemoryEFContext(options => new EFContextTest(options, new Mock<ILoggerFactory>().Object), systemUserId);
        }

        public static T CreateInMemoryEFContext<T>(Func<EFContextOptions, T> efContectFactory, Guid? systemUserId = null)
            where T : EFContext
        {
            var modelBuilder = new ModelBuilder(new Microsoft.EntityFrameworkCore.Metadata.Conventions.ConventionSet());

            EFContext.FixSnakeCaseNames(modelBuilder);
            EFMappings.DefineMappings(modelBuilder);

            var dbContextOptions = new DbContextOptionsBuilder<T>()
                .UseInMemoryDatabase("costs")
                .UseModel(modelBuilder.Model)
                .ConfigureWarnings(wcb => wcb.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var efContextOptions = new EFContextOptions
            {
                DbContextOptions = dbContextOptions,
                SystemUserId = systemUserId ?? Guid.Empty
            };

            return efContectFactory(efContextOptions);
        }
    }
}
