using AgGrid.InfiniteRowModel.Sample.Database;
using AgGrid.InfiniteRowModel.Sample.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace AgGrid.InfiniteRowModel.Tests
{
    public class Ordering
    {
        private readonly AppDbContext _dbContext;

        public Ordering()
        {
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(dbContextOptions);
        }

        [Fact]
        public void OrderByDescending()
        {
            var users = new[]
            {
                new User { FullName = "Ala Kowalska" },
                new User { FullName = "Jan Kowalski" },
                new User { FullName = "Ala Nowak" }
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
                new User { FullName = "Ala Nowak" },
                new User { FullName = "Ala Kowalska" },
                new User { FullName = "Jan Kowalski" }
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
                new User { FullName = "Jan Kowalski", Age = 18 },
                new User { FullName = "Ala Nowak", Age = 21 },
                new User { FullName = "Ala Nowak", Age = 22 }
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
    }
}
