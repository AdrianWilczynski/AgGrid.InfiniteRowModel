using AgGrid.InfiniteRowModel.Sample.Database;
using AgGrid.InfiniteRowModel.Sample.Entities;
using System;
using System.Linq;
using Xunit;

namespace AgGrid.InfiniteRowModel.Tests
{
    public abstract class Paging : IDisposable
    {
        protected readonly AppDbContext _dbContext;

        protected Paging(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Fact]
        public void Page()
        {
            var users = new[]
            {
                new User { Id = 1, FullName = "1" },
                new User { Id = 2, FullName = "2" },
                new User { Id = 3, FullName = "3" },
                new User { Id = 4, FullName = "4" },
                new User { Id = 5, FullName = "5" }
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

        public virtual void Dispose() => _dbContext.Dispose();
    }
}
