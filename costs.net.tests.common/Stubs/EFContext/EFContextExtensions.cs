namespace costs.net.tests.common.Stubs.EFContext
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Storage;
    using Moq;

    public static class DbContextExtensions
    {
        public static Mock<DbSet<TEntity>> MockAsyncQueryable<TDbContext, TEntity>(this Mock<TDbContext> context, IQueryable<TEntity> list, Expression<Func<TDbContext, DbSet<TEntity>>> setupFunc)
            where TDbContext : DbContext
            where TEntity : class
        {
            var mockSet = new Mock<DbSet<TEntity>>();

            mockSet.As<IAsyncEnumerable<TEntity>>()
                .Setup(m => m.GetEnumerator())
                .Returns(new AsyncEnumeratorStub<TEntity>(list.GetEnumerator()));

            mockSet.As<IQueryable<TEntity>>()
                .Setup(m => m.Provider)
                .Returns(new AsyncQueryProviderStub<TEntity>(list.Provider));

            mockSet.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(list.Expression);
            mockSet.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(list.ElementType);
            mockSet.As<IQueryable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(list.GetEnumerator);

            context.Setup(setupFunc).Returns(mockSet.Object);

            return mockSet;
        }

        public static Mock<IDbContextTransaction> MockCurrentTransaction<TDbContext>(this Mock<TDbContext> context)
            where TDbContext : DbContext
        {
            var databaseMock = new Mock<DatabaseFacade>(context.Object);
            var transactionMock = new Mock<IDbContextTransaction>();
            databaseMock.Setup(d => d.CurrentTransaction).Returns(transactionMock.Object);
            context.Setup(c => c.Database).Returns(databaseMock.Object);

            return transactionMock;
        }
    }
}