using AgGrid.InfiniteRowModel.Sample.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace AgGrid.InfiniteRowModel.Tests
{
    public static class PostgreSQL
    {
        public static AppDbContext GetDbContext()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(PostgreSQL).Assembly)
                .Build();

            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql($"Host=localhost;Username={configuration["PostgreSQL:Username"]};Password={configuration["PostgreSQL:Password"]};Database={Guid.NewGuid()}")
                .Options;

            var dbContext = new AppDbContext(dbContextOptions);

            dbContext.Database.EnsureCreated();

            return dbContext;
        }

        public static void Cleanup(AppDbContext dbContext) => dbContext.Database.EnsureDeleted();
    }

    public class PostgreSQLFiltering : Filtering
    {
        public PostgreSQLFiltering() : base(PostgreSQL.GetDbContext()) { }

        public override void Dispose()
        {
            PostgreSQL.Cleanup(_dbContext);
            base.Dispose();
        }
    }

    public class PostgreSQLOrdering : Ordering
    {
        public PostgreSQLOrdering() : base(PostgreSQL.GetDbContext()) { }

        public override void Dispose()
        {
            PostgreSQL.Cleanup(_dbContext);
            base.Dispose();
        }
    }

    public class PostgreSQLPaging : Paging
    {
        public PostgreSQLPaging() : base(PostgreSQL.GetDbContext()) { }

        public override void Dispose()
        {
            PostgreSQL.Cleanup(_dbContext);
            base.Dispose();
        }
    }
}