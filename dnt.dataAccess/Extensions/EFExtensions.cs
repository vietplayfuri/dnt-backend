namespace dnt.dataAccess.Extensions
{
    using System;
    using System.Linq.Expressions;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Npgsql.NameTranslation;

    public static class EFExtensions
    {
        public static EntityEntry<T> AddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {
            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
            return !exists ? dbSet.Add(entity) : null;
        }

        public static void ReloadEntity<TEntity>(
            this DbContext context,
            TEntity entity)
            where TEntity : class
        {
            context.Entry(entity).Reload();
        }

        public static IQueryable<TEntity> FromView<TEntity>(this DbSet<TEntity> dbSet, string viewName)
            where TEntity : class
        {
            var mapper = new NpgsqlSnakeCaseNameTranslator();
            return dbSet.FromSql($"select * from {mapper.TranslateTypeName(viewName)}");
        }

        public static bool IsBusy<TDbContext>(this TDbContext dbContext) where TDbContext : DbContext
        {
            var connection = dbContext.Database.GetDbConnection();
            return connection.State == ConnectionState.Executing || connection.State == ConnectionState.Fetching;
        }

        public static void InTransaction<TDbContext>(this TDbContext dbContext, Action action,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified)
            where TDbContext : DbContext
        {
            if (dbContext.Database.CurrentTransaction == null)
            {
                using (var tran = dbContext.Database.BeginTransaction(isolationLevel))
                {
                    action();
                    dbContext.SaveChanges();
                    tran.Commit();
                }
            }
            else
            {
                action();
                dbContext.SaveChanges();
            }
        }

        public static async Task InTransactionAsync<TDbContext>(this TDbContext dbContext, Action action,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified)
            where TDbContext : DbContext
        {
            if (dbContext.Database.CurrentTransaction == null)
            {
                using (var tran = dbContext.Database.BeginTransaction(isolationLevel))
                {
                    action();
                    await dbContext.SaveChangesAsync();
                    tran.Commit();
                }
            }
            else
            {
                action();
                dbContext.SaveChanges();
            }
        }

        public static async Task InTransactionAsync<TDbContext>(this TDbContext dbContext, Action action,
            Func<Task> onCommited = null,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified)
            where TDbContext : DbContext
        {
            if (dbContext.Database.CurrentTransaction == null)
            {
                using (var tran = dbContext.Database.BeginTransaction(isolationLevel))
                {
                    action();
                    await dbContext.SaveChangesAsync();
                    tran.Commit();
                    if (onCommited != null)
                    {
                        await onCommited();
                    }
                }
            }
            else
            {
                action();
                dbContext.SaveChanges();
            }
        }
        
        public static async Task InTransactionAsync<TDbContext>(this TDbContext dbContext, Func<Task> action,
            Func<Task> onCommited = null,
            IsolationLevel isolationLevel = IsolationLevel.Unspecified)
            where TDbContext : DbContext
        {
            if (dbContext.Database.CurrentTransaction == null)
            {
                using (var tran = await dbContext.Database.BeginTransactionAsync(isolationLevel))
                {
                    // Execute action and commit transaction. Rollback happens automaticall in Transaction's Dispose method
                    await action();
                    await dbContext.SaveChangesAsync();
                    tran.Commit();
                    if (onCommited != null)
                    {
                        await onCommited();
                    }
                }
            }
            else
            {
                await action();
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
