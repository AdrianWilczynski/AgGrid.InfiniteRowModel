using AgGrid.InfiniteRowModel.Sample.Database;
using Microsoft.EntityFrameworkCore;
using System;

namespace AgGrid.InfiniteRowModel.Tests
{
    public static class SqlServer
    {
        public static AppDbContext GetDbContext()
        {
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(@$"Server=(localdb)\mssqllocaldb;Database={Guid.NewGuid()};Trusted_Connection=True;")
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
        public SqlServerFiltering() : base(SqlServer.GetDbContext()) { }

        public override void Dispose()
        {
            SqlServer.Cleanup(_dbContext);
            base.Dispose();
        }
    }

    public class SqlServerOrdering : Ordering
    {
        public SqlServerOrdering() : base(SqlServer.GetDbContext()) { }

        public override void Dispose()
        {
            SqlServer.Cleanup(_dbContext);
            base.Dispose();
        }
    }

    public class SqlServerPaging : Paging
    {
        public SqlServerPaging() : base(SqlServer.GetDbContext()) { }

        public override void Dispose()
        {
            SqlServer.Cleanup(_dbContext);
            base.Dispose();
        }
    }
}
