using AgGrid.InfiniteRowModel.Sample.Database;
using AgGrid.InfiniteRowModel.Sample.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AgGrid.InfiniteRowModel.Tests
{
    public static class InMemory
    {
        public static AppDbContext GetDbContext()
        {
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return  new AppDbContext(dbContextOptions);
        }
    }

    public class InMemoryFiltering : Filtering
    {
        public InMemoryFiltering() : base(InMemory.GetDbContext()) { }

        [Theory]
        [InlineData("Kowalska", FilterModelType.Contains, 1)]
        [InlineData("Kowalska", FilterModelType.NotContains, 2)]
        [InlineData("Ala", FilterModelType.StartsWith, 1)]
        [InlineData("Kowalska", FilterModelType.EndsWith, 1)]
        [InlineData("Ala Kowalska", FilterModelType.Equals, 1)]
        [InlineData("Ala Kowalska", FilterModelType.NotEqual, 2)]
        public void BeCaseSensitiveByDefaultButThisDependsOnDatabaseBehavior(string filter, string type, params int[] expectedIds)
        {
            var users = new[]
            {
                new User { Id = 1, FullName = "Ala Kowalska" },
                new User { Id = 2, FullName = "ala kowalska" },
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "fullName", new FilterModel { Filter = filter, Type = type, FilterType = FilterModelFilterType.Text } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Equal(expectedIds.Length, result.RowsThisBlock.Count());
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }
    }

    public class InMemoryOrdering : Ordering
    {
        public InMemoryOrdering() : base(InMemory.GetDbContext()) { }
    }

    public class InMemoryPaging : Paging
    {
        public InMemoryPaging() : base(InMemory.GetDbContext()) { }
    }
}
