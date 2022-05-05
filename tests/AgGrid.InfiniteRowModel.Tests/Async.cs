using AgGrid.InfiniteRowModel.EntityFrameworkCore;
using AgGrid.InfiniteRowModel.Sample.Database;
using AgGrid.InfiniteRowModel.Sample.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AgGrid.InfiniteRowModel.Tests
{
    public class Async : IDisposable
    {
        private readonly AppDbContext _dbContext = InMemory.GetDbContext();

        [Fact]
        public async Task Filter()
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
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "fullName", new FilterModel { Filter = "Kowal", Type = FilterModelType.Contains, FilterType = FilterModelFilterType.Text } }
                }
            };

            var result = await _dbContext.Users.GetInfiniteRowModelBlockAsync(query);

            Assert.Contains(result.RowsThisBlock, r => r.Id == 1);
            Assert.Contains(result.RowsThisBlock, r => r.Id == 2);
            Assert.DoesNotContain(result.RowsThisBlock, r => r.Id == 3);
        }

        [Fact]
        public async Task ParseFromJson()
        {
            var users = new[]
            {
                new User { Id = 1, FullName = "Ala Nowak", Age = 28, IsVerified = true, RegisteredOn = new DateTime(2020, 5, 11) },
                new User { Id = 2, FullName = "Ada Kowalska", Age = 22, IsVerified = false, RegisteredOn = new DateTime(2019, 5, 11) },
                new User { Id = 3, FullName = "Jan Kowalczyk", Age = 33, IsVerified = false, RegisteredOn = new DateTime(2018, 5, 11) },
                new User { Id = 4, FullName = "Andrzej Nowak", Age = 15, IsVerified = true, RegisteredOn = new DateTime(2017, 5, 11) },
                new User { Id = 5, FullName = "Jan Nowak", Age = 34, IsVerified = true, RegisteredOn = new DateTime(2015, 5, 11) }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 3,
                SortModel = new[] { new SortModel { ColId = "fullName", Sort = SortModelSortDirection.Ascending } },
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "fullName", new FilterModel { Filter = "Ala", Type = FilterModelType.Contains, FilterType = FilterModelFilterType.Text } },
                    { "age", new FilterModel { Filter = 28, Type = FilterModelType.Equals, FilterType = FilterModelFilterType.Number } },
                    { "isVerified", new FilterModel { Filter = true, Type = FilterModelType.Equals, FilterType = FilterModelFilterType.Boolean } },
                    { "registeredOn", new FilterModel { DateFrom = "2020-05-08 00:00:00", Type = FilterModelType.GreaterThan, FilterType = FilterModelFilterType.Date } },
                }
            };

            var queryJson = JsonSerializer.Serialize(query, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            var result = await _dbContext.Users.GetInfiniteRowModelBlockAsync(queryJson);

            Assert.Equal(1, result.RowsThisBlock.Single().Id);
        }

        public virtual void Dispose() => _dbContext.Dispose();
    }
}
