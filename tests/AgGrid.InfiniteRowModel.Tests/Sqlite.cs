using AgGrid.InfiniteRowModel.Sample.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit.Abstractions;

namespace AgGrid.InfiniteRowModel.Tests
{
    public static class Sqlite
    {
        public static AppDbContext GetDbContext(ITestOutputHelper output)
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .LogTo(output.WriteLine, (eventId, _) => eventId == RelationalEventId.CommandExecuted)
                .Options;

            var dbContext = new AppDbContext(dbContextOptions);

            dbContext.Database.EnsureCreated();

            return dbContext;
        }

        public static void Cleanup(AppDbContext dbContext) => dbContext.Database.GetDbConnection().Close();
    }

    public class SqliteFiltering : Filtering
    {
        public SqliteFiltering(ITestOutputHelper output) : base(Sqlite.GetDbContext(output)) { }

        public override void Dispose()
        {
            Sqlite.Cleanup(_dbContext);
            base.Dispose();
        }
    }

    public class SqliteOrdering : Ordering
    {
        public SqliteOrdering(ITestOutputHelper output) : base(Sqlite.GetDbContext(output)) { }

        public override void Dispose()
        {
            Sqlite.Cleanup(_dbContext);
            base.Dispose();
        }
    }

    public class SqlitePaging : Paging
    {
        public SqlitePaging(ITestOutputHelper output) : base(Sqlite.GetDbContext(output)) { }

        public override void Dispose()
        {
            Sqlite.Cleanup(_dbContext);
            base.Dispose();
        }
    }
}
