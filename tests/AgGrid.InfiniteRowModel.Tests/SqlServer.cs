using AgGrid.InfiniteRowModel.Sample.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using Xunit.Abstractions;

namespace AgGrid.InfiniteRowModel.Tests
{
    public static class SqlServer
    {
        public static AppDbContext GetDbContext(ITestOutputHelper output)
        {
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(@$"Server=(localdb)\mssqllocaldb;Database={Guid.NewGuid()};Trusted_Connection=True;")
                .LogTo(output.WriteLine, (eventId, _) => eventId == RelationalEventId.CommandExecuted)
                .Options;

            var dbContext = new AppDbContext(dbContextOptions);

            dbContext.Database.EnsureCreated();

            dbContext.Database.OpenConnection();
            dbContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Users ON");

            return dbContext;
        }

        public static void Cleanup(AppDbContext dbContext)
        {
            dbContext.Database.CloseConnection();
            dbContext.Database.EnsureDeleted();
        }
    }

    public class SqlServerFiltering : Filtering
    {
        public SqlServerFiltering(ITestOutputHelper output) : base(SqlServer.GetDbContext(output)) { }

        public override void Dispose()
        {
            SqlServer.Cleanup(_dbContext);
            base.Dispose();
        }
    }

    public class SqlServerOrdering : Ordering
    {
        public SqlServerOrdering(ITestOutputHelper output) : base(SqlServer.GetDbContext(output)) { }

        public override void Dispose()
        {
            SqlServer.Cleanup(_dbContext);
            base.Dispose();
        }
    }

    public class SqlServerPaging : Paging
    {
        public SqlServerPaging(ITestOutputHelper output) : base(SqlServer.GetDbContext(output)) { }

        public override void Dispose()
        {
            SqlServer.Cleanup(_dbContext);
            base.Dispose();
        }
    }
}
