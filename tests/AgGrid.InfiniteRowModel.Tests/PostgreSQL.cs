using AgGrid.InfiniteRowModel.Sample.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using System;
using Xunit.Abstractions;

namespace AgGrid.InfiniteRowModel.Tests
{
    public static class PostgreSQL
    {
        public static AppDbContext GetDbContext(ITestOutputHelper output)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(PostgreSQL).Assembly)
                .Build();

            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql($"Host=localhost;Username={configuration["PostgreSQL:Username"]};Password={configuration["PostgreSQL:Password"]};Database={Guid.NewGuid()}")
                .LogTo(output.WriteLine, (eventId, _) => eventId == RelationalEventId.CommandExecuted)
                .Options;

            var dbContext = new AppDbContext(dbContextOptions);

            dbContext.Database.EnsureCreated();

            return dbContext;
        }

        public static void Cleanup(AppDbContext dbContext) => dbContext.Database.EnsureDeleted();
    }

    public class PostgreSQLFiltering : Filtering
    {
        public PostgreSQLFiltering(ITestOutputHelper output) : base(PostgreSQL.GetDbContext(output)) { }

        public override void Dispose()
        {
            PostgreSQL.Cleanup(_dbContext);
            base.Dispose();
        }
    }

    public class PostgreSQLOrdering : Ordering
    {
        public PostgreSQLOrdering(ITestOutputHelper output) : base(PostgreSQL.GetDbContext(output)) { }

        public override void Dispose()
        {
            PostgreSQL.Cleanup(_dbContext);
            base.Dispose();
        }
    }

    public class PostgreSQLPaging : Paging
    {
        public PostgreSQLPaging(ITestOutputHelper output) : base(PostgreSQL.GetDbContext(output)) { }

        public override void Dispose()
        {
            PostgreSQL.Cleanup(_dbContext);
            base.Dispose();
        }
    }
}