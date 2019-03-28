namespace dnt.dataAccess
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Entity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Npgsql.NameTranslation;
    using Npgsql;
    using Extensions;

    public class EFContext : DbContext
    {
        private static readonly INpgsqlNameTranslator NameTranslator = new NpgsqlSnakeCaseNameTranslator();
        private readonly EFContextOptions _options;
        private readonly ILoggerFactory _loggerFactory;

        public EFContext()
        {
        }

        public EFContext(EFContextOptions options, ILoggerFactory loggerFactory) : base(options.DbContextOptions)
        {
            _options = options;
            _loggerFactory = loggerFactory;
        }

        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<UserRole> UserRole { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            FixSnakeCaseNames(modelBuilder);
            EFMappings.DefineMappings(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (_options != null && _options.IsLoggingEnabled)
            {
                optionsBuilder.UseLoggerFactory(_loggerFactory);
            }
        }

        public override int SaveChanges()
        {
            OnSavingChanges();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnSavingChanges();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            OnSavingChanges();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            OnSavingChanges();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public static void FixSnakeCaseNames(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // modify column names
                foreach (var property in entity.GetProperties())
                {
                    property.Relational().ColumnName = NameTranslator.TranslateMemberName(property.Relational().ColumnName);
                }

                // modify table name
                entity.Relational().TableName = NameTranslator.TranslateMemberName(entity.Relational().TableName);
            }
        }

        private void OnSavingChanges()
        {
            if (!ChangeTracker.HasChanges())
            {
                return;
            }

            var modifiable = ChangeTracker.Entries<IModifiable>();
            foreach (var entry in modifiable)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.Entity.SetModifiedNow();
                        break;
                    case EntityState.Added:
                        entry.Entity.SetCreatedNow(_options.SystemUserId);
                        break;
                }
            }
        }

        private static string ToLower(string input)
        {
            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }
        /*
         * Further Reading:
         * https://docs.microsoft.com/en-us/ef/core/querying/async
         * https://docs.microsoft.com/en-us/ef/core/querying/related-data
         * https://docs.microsoft.com/en-us/ef/core/modeling/relationships
         * http://stackoverflow.com/questions/5597760/what-effects-can-the-virtual-keyword-have-in-entity-framework-4-1-poco-code-fi
         * 
         */
    }
}
