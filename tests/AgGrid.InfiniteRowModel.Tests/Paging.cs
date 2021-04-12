using AgGrid.InfiniteRowModel.Sample.Database;
using AgGrid.InfiniteRowModel.Sample.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace AgGrid.InfiniteRowModel.Tests
{
    public class Paging
    {
        private readonly AppDbContext _dbContext;

        public Paging()
        {
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(dbContextOptions);
        }

        [Fact]
        public void Page()
        {
            var users = new[]
            {
                new User { FullName = "1" },
                new User { FullName = "2" },
                new User { FullName = "3" },
                new User { FullName = "4" },
                new User { FullName = "5" }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 3,
                SortModel = new[] { new SortModel { ColId = "fullName", Sort = SortModelSortDirection.Ascending } }
            };

            var page1 = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Null(page1.LastRow);
            Assert.Equal(3, page1.RowsThisBlock.Count());
            Assert.Contains(page1.RowsThisBlock, u => u.FullName == "1");
            Assert.Contains(page1.RowsThisBlock, u => u.FullName == "2");
            Assert.Contains(page1.RowsThisBlock, u => u.FullName == "3");

            query.StartRow = 3;
            query.EndRow = 6;

            var page2 = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Equal(5, page2.LastRow);
            Assert.Equal(2, page2.RowsThisBlock.Count());
            Assert.Contains(page2.RowsThisBlock, u => u.FullName == "4");
            Assert.Contains(page2.RowsThisBlock, u => u.FullName == "5");
        }
    }
}
