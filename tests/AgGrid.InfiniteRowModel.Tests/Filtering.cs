using AgGrid.InfiniteRowModel.Sample.Database;
using AgGrid.InfiniteRowModel.Sample.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AgGrid.InfiniteRowModel.Tests
{
    public class Filtering
    {
        private readonly AppDbContext _dbContext;

        public Filtering()
        {
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(dbContextOptions);
        }

        [Theory]
        [InlineData("Kowal", FilterModelType.Contains, 1, 2)]
        [InlineData("Kowal", FilterModelType.NotContains, 3)]
        [InlineData("Ala Kowalska", FilterModelType.Equals, 1)]
        [InlineData("Ala Kowalska", FilterModelType.NotEqual, 2, 3)]
        [InlineData("Ala", FilterModelType.StartsWith, 1, 3)]
        [InlineData("ska", FilterModelType.EndsWith, 1)]
        public void FilterByText(string filter, string type, params int[] expectedIds)
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
                    { "fullName", new FilterModel { Filter = filter, Type = type, FilterType = FilterModelFilterType.Text } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.NotEmpty(result.RowsThisBlock);
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }

        [Theory]
        [InlineData(18, FilterModelType.Equals, 3)]
        [InlineData(18, FilterModelType.NotEqual, 1, 2)]
        [InlineData(22, FilterModelType.LessThan, 3)]
        [InlineData(22, FilterModelType.LessThanOrEqual, 1, 3)]
        [InlineData(22, FilterModelType.GreaterThan, 2)]
        [InlineData(22, FilterModelType.GreaterThanOrEqual, 1, 2)]
        [InlineData(22.5, FilterModelType.LessThan, 3, 1)]
        public void FilterByNumber(object filter, string type, params int[] expectedIds)
        {
            var users = new[]
            {
                new User { Id = 1, Age = 22 },
                new User { Id = 2, Age = 55 },
                new User { Id = 3, Age = 18 }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "age", new FilterModel { Filter = filter, Type = type, FilterType = FilterModelFilterType.Number } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.NotEmpty(result.RowsThisBlock);
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }

        [Fact]
        public void FilterByNumberRange()
        {
            var users = new[]
            {
                new User { Id = 1, Age = 22 },
                new User { Id = 2, Age = 55 },
                new User { Id = 3, Age = 18 }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "age", new FilterModel { Filter = 16, FilterTo = 22, Type = FilterModelType.InRange, FilterType = FilterModelFilterType.Number } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Contains(result.RowsThisBlock, u => u.Id == 1);
            Assert.Contains(result.RowsThisBlock, u => u.Id == 3);
            Assert.DoesNotContain(result.RowsThisBlock, u => u.Id == 2);
        }

        [Theory]
        [InlineData(true, FilterModelType.Equals, 1, 2)]
        [InlineData(false, FilterModelType.Equals, 3)]
        public void FilterByBoolean(bool filter, string type, params int[] expectedIds)
        {
            var users = new[]
            {
                new User { Id = 1, IsVerified = true },
                new User { Id = 2, IsVerified = true},
                new User { Id = 3, IsVerified = false }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "isVerified", new FilterModel { Filter = filter, Type = type, FilterType = FilterModelFilterType.Boolean } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.NotEmpty(result.RowsThisBlock);
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }


        [Theory]
        [InlineData("2020-05-11 00:00:00", FilterModelType.Equals, 1)]
        [InlineData("2020-05-11 00:00:00", FilterModelType.NotEqual, 2, 3)]
        [InlineData("2019-07-04 00:00:00", FilterModelType.LessThan, 3)]
        [InlineData("2019-07-04 00:00:00", FilterModelType.LessThanOrEqual, 2, 3)]
        [InlineData("2019-07-04 00:00:00", FilterModelType.GreaterThan, 1)]
        [InlineData("2019-07-04 00:00:00", FilterModelType.GreaterThanOrEqual, 1, 2)]
        public void FilterByDate(string filter, string type, params int[] expectedIds)
        {
            var users = new[]
            {
                new User { Id = 1, RegisteredOn = new DateTime(2020, 5, 11) },
                new User { Id = 2, RegisteredOn = new DateTime(2019, 7, 4) },
                new User { Id = 3, RegisteredOn = new DateTime(2010, 5, 13) }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "registeredOn", new FilterModel { DateFrom = filter, Type = type, FilterType = FilterModelFilterType.Date } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.NotEmpty(result.RowsThisBlock);
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }

        [Fact]
        public void FilterByDateRange()
        {
            var users = new[]
            {
                new User { Id = 1, RegisteredOn = new DateTime(2020, 5, 11) },
                new User { Id = 2, RegisteredOn = new DateTime(2019, 7, 4) },
                new User { Id = 3, RegisteredOn = new DateTime(2010, 5, 13) }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    {
                        "registeredOn",
                        new FilterModel
                        {
                            DateFrom = "2018-07-04 00:00:00",
                            DateTo = "2023-07-04 00:00:00",
                            Type = FilterModelType.InRange,
                            FilterType = FilterModelFilterType.Date
                        }
                    }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Contains(result.RowsThisBlock, u => u.Id == 1);
            Assert.Contains(result.RowsThisBlock, u => u.Id == 2);
            Assert.DoesNotContain(result.RowsThisBlock, u => u.Id == 3);
        }

        [Theory]
        [InlineData("Ada", 22, 1)]
        [InlineData("Nowak", 18, 3)]
        public void FilterByMultipleProperties(string nameFilter, int ageFilter, params int[] expectedIds)
        {
            var users = new[]
            {
                new User { Id = 1, FullName = "Ada Kowalska", Age = 22 },
                new User { Id = 2, FullName = "Janek Nowak", Age = 55 },
                new User { Id = 3, FullName = "Ada Nowak", Age = 18 }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "fullName", new FilterModel { Filter = nameFilter, Type = FilterModelType.Contains, FilterType = FilterModelFilterType.Text } },
                    { "age", new FilterModel { Filter = ageFilter, Type = FilterModelType.Equals, FilterType = FilterModelFilterType.Number } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.NotEmpty(result.RowsThisBlock);
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }

        [Theory]
        [InlineData("Ada", "Kowalska", FilterModelOperator.And, 1)]
        [InlineData("Nowak", "Kowalczyk", FilterModelOperator.Or, 2, 3)]
        public void FilterByMultipleConditions(string filter1, string filter2, string filterOperator, params int[] expectedIds)
        {
            var users = new[]
            {
                new User { Id = 1, FullName = "Ada Kowalska" },
                new User { Id = 2, FullName = "Janek Nowak" },
                new User { Id = 3, FullName = "Ada Kowalczyk" }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    {
                        "fullName",
                        new FilterModel
                        {
                            FilterType = FilterModelFilterType.Text,
                            Operator = filterOperator,
                            Condition1 = new FilterModel
                            {
                                FilterType = FilterModelFilterType.Text,
                                Type = FilterModelType.Contains,
                                Filter = filter1
                            },
                            Condition2 = new FilterModel
                            {
                                FilterType = FilterModelFilterType.Text,
                                Type = FilterModelType.Contains,
                                Filter = filter2
                            }
                        }
                    },
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.NotEmpty(result.RowsThisBlock);
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }
    }
}
