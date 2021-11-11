using AgGrid.InfiniteRowModel.Sample.Database;
using AgGrid.InfiniteRowModel.Sample.Entities;
using System;
using System.Linq;
using Xunit;

namespace AgGrid.InfiniteRowModel.Tests
{
    public abstract class Ordering : IDisposable
    {
        protected readonly AppDbContext _dbContext;

        protected Ordering(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Fact]
        public void OrderByDescending()
        {
            var users = new[]
            {
                new User { Id = 1, FullName = "Ala Kowalska" },
                new User { Id = 2, FullName = "Jan Kowalski" },
                new User { Id = 3, FullName = "Ala Nowak" }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                SortModel = new[]
                {
                    new SortModel
                    {
                        ColId = "fullName",
                        Sort = SortModelSortDirection.Descending
                    }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Equal("Jan Kowalski", result.RowsThisBlock.ElementAt(0).FullName);
            Assert.Equal("Ala Nowak", result.RowsThisBlock.ElementAt(1).FullName);
            Assert.Equal("Ala Kowalska", result.RowsThisBlock.ElementAt(2).FullName);
        }

        [Fact]
        public void OrderByAscending()
        {
            var users = new[]
            {
                new User { Id = 1, FullName = "Ala Nowak" },
                new User { Id = 2, FullName = "Ala Kowalska" },
                new User { Id = 3, FullName = "Jan Kowalski" }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                SortModel = new[]
                {
                    new SortModel
                    {
                        ColId = "fullName",
                        Sort = SortModelSortDirection.Ascending
                    }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Equal("Ala Kowalska", result.RowsThisBlock.ElementAt(0).FullName);
            Assert.Equal("Ala Nowak", result.RowsThisBlock.ElementAt(1).FullName);
            Assert.Equal("Jan Kowalski", result.RowsThisBlock.ElementAt(2).FullName);
        }

        [Fact]
        public void OrderByMultipleProperties()
        {
            var users = new[]
            {
                new User { Id = 1, FullName = "Jan Kowalski", Age = 18 },
                new User { Id = 2, FullName = "Ala Nowak", Age = 21 },
                new User { Id = 3, FullName = "Ala Nowak", Age = 22 }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                SortModel = new[]
                {
                    new SortModel
                    {
                        ColId = "fullName",
                        Sort = SortModelSortDirection.Ascending
                    },
                    new SortModel
                    {
                        ColId = "age",
                        Sort = SortModelSortDirection.Ascending
                    }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Equal(21, result.RowsThisBlock.ElementAt(0).Age);
            Assert.Equal(22, result.RowsThisBlock.ElementAt(1).Age);
            Assert.Equal(18, result.RowsThisBlock.ElementAt(2).Age);
        }

        public virtual void Dispose() => _dbContext.Dispose();
    }
}
