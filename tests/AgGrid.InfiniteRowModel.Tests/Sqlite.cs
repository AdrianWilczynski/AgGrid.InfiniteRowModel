using AgGrid.InfiniteRowModel.Sample.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AgGrid.InfiniteRowModel.Tests
{
    public static class Sqlite
    {
        public static AppDbContext GetDbContext()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new AppDbContext(dbContextOptions);

            dbContext.Database.EnsureCreated();

            return dbContext;
        }

        public static void Cleanup(AppDbContext dbContext) => dbContext.Database.GetDbConnection().Close();
    }

    public class SqliteFiltering : Filtering
    {
        public SqliteFiltering() : base(Sqlite.GetDbContext()) { }

        public override void Dispose()
        {
            Sqlite.Cleanup(_dbContext);
            base.Dispose();
        }
    }

    public class SqliteOrdering : Ordering
    {
        public SqliteOrdering() : base(Sqlite.GetDbContext()) { }

        public override void Dispose()
        {
            Sqlite.Cleanup(_dbContext);
            base.Dispose();
        }
    }

    public class SqlitePaging : Paging
    {
        public SqlitePaging() : base(Sqlite.GetDbContext()) { }

        public override void Dispose()
        {
            Sqlite.Cleanup(_dbContext);
            base.Dispose();
        }
    }
}
