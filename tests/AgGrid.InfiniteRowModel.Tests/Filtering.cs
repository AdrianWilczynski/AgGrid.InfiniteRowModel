using AgGrid.InfiniteRowModel.Sample.Database;
using AgGrid.InfiniteRowModel.Sample.Entities;
using AgGrid.InfiniteRowModel.Tests.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AgGrid.InfiniteRowModel.Tests
{
    public abstract class Filtering : IDisposable
    {
        protected readonly AppDbContext _dbContext;

        protected Filtering(AppDbContext dbContext)
        {
            _dbContext = dbContext;
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

            Assert.Equal(expectedIds.Length, result.RowsThisBlock.Count());
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

            Assert.Equal(expectedIds.Length, result.RowsThisBlock.Count());
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
        [InlineData(true, FilterModelType.NotEqual, 3)]
        [InlineData(false, FilterModelType.NotEqual, 1, 2)]
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

            Assert.Equal(expectedIds.Length, result.RowsThisBlock.Count());
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

            Assert.Equal(expectedIds.Length, result.RowsThisBlock.Count());
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

            Assert.Equal(expectedIds.Length, result.RowsThisBlock.Count());
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

            Assert.Equal(expectedIds.Length, result.RowsThisBlock.Count());
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }

        [Fact]
        public void FilterByNull()
        {
            var users = new[]
            {
                new User { Id = 1, FullName = "Ala Kowalska" },
                new User { Id = 2, FullName = null },
                new User { Id = 3, FullName = null },
                new User { Id = 4, FullName = "Jan Kowalski" },
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "fullName", new FilterModel { Type = FilterModelType.Null, FilterType = FilterModelFilterType.Text } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Contains(result.RowsThisBlock, r => r.Id == 2);
            Assert.Contains(result.RowsThisBlock, r => r.Id == 3);
            Assert.DoesNotContain(result.RowsThisBlock, r => r.Id == 4 || r.Id == 1);
        }

        [Theory]
        [InlineData(FilterModelOperator.And)]
        [InlineData(FilterModelOperator.Or, 1, 2, 3)]
        public void CombineFilteringByNull(string filterOperator, params int[] expectedIds)
        {
            var users = new[]
            {
                new User { Id = 1, FullName = "Ada Kowalska" },
                new User { Id = 2, FullName = null },
                new User { Id = 3, FullName = "Ada Kowalczyk" },
                new User { Id = 4, FullName = "Jan Kowalski" }
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
                                Type = FilterModelType.Null
                            },
                            Condition2 = new FilterModel
                            {
                                FilterType = FilterModelFilterType.Text,
                                Type = FilterModelType.StartsWith,
                                Filter = "A"
                            }
                        }
                    },
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Equal(expectedIds.Length, result.RowsThisBlock.Count());
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }

        [Theory]
        [InlineData(FilterModelOperator.And, 2)]
        [InlineData(FilterModelOperator.Or, 1, 2, 3)]
        public void CombineFilteringByRange(string filterOperator, params int[] expectedIds)
        {
            var users = new[]
            {
                new User { Id = 1, Age = 32 },
                new User { Id = 2, Age = 55 },
                new User { Id = 3, Age = 66 },
                new User { Id = 4, Age = 77 }
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
                        "age",
                        new FilterModel
                        {
                            FilterType = FilterModelFilterType.Number,
                            Operator = filterOperator,
                            Condition1 = new FilterModel
                            {
                                FilterType = FilterModelFilterType.Number,
                                Type = FilterModelType.InRange,
                                Filter = 50,
                                FilterTo = 70
                            },
                            Condition2 = new FilterModel
                            {
                                FilterType = FilterModelFilterType.Number,
                                Type = FilterModelType.LessThan,
                                Filter = 60
                            }
                        }
                    },
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Equal(expectedIds.Length, result.RowsThisBlock.Count());
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }

        [Fact]
        public void FilterByNotNull()
        {
            var users = new[]
            {
                new User { Id = 1, FullName = "Ala Kowalska" },
                new User { Id = 2, FullName = null },
                new User { Id = 3, FullName = null },
                new User { Id = 4, FullName = "Jan Kowalski" },
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "fullName", new FilterModel { Type = FilterModelType.NotNull, FilterType = FilterModelFilterType.Text } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Contains(result.RowsThisBlock, r => r.Id == 1);
            Assert.Contains(result.RowsThisBlock, r => r.Id == 4);
            Assert.DoesNotContain(result.RowsThisBlock, r => r.Id == 2 || r.Id == 3);
        }

        [Theory]
        [InlineData("kOwAl", FilterModelType.Contains, 1, 2)]
        [InlineData("kOwAl", FilterModelType.NotContains, 3)]
        [InlineData("aLa", FilterModelType.StartsWith, 1, 3)]
        [InlineData("owaLSKA", FilterModelType.EndsWith, 1)]
        [InlineData("ala KOWALSKA", FilterModelType.Equals, 1)]
        [InlineData("ala KOWALSKA", FilterModelType.NotEqual, 2, 3)]
        public void BeCaseInsensitiveIfConfiguredToBe(string filter, string type, params int[] expectedIds)
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

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query, new() { CaseInsensitive = true });

            Assert.Equal(expectedIds.Length, result.RowsThisBlock.Count());
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }

        [Theory]
        [InlineData(22, 34, 2)]
        [InlineData(21, 33, 1)]
        [InlineData(22, 55, 2, 3)]
        public void HaveExclusiveInRangeNumberFilteringIfConfiguredToThisWay(int from, int to, params int[] expectedIds)
        {
            var users = new[]
            {
                new User { Id = 1, Age = 22 },
                new User { Id = 2, Age = 33  },
                new User { Id = 3, Age = 44 },
                new User { Id = 4, Age = 55 },
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "age", new FilterModel { Filter = from,  FilterTo = to, Type = FilterModelType.InRange, FilterType = FilterModelFilterType.Number } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query, new() { InRangeExclusive = true });

            Assert.Equal(expectedIds.Length, result.RowsThisBlock.Count());
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }

        [Fact]
        public void HaveInclusiveInRangeNumberFilteringByDefault()
        {
            var users = new[]
            {
                new User { Id = 1, Age = 22 },
                new User { Id = 2, Age = 33  },
                new User { Id = 3, Age = 44 },
                new User { Id = 4, Age = 55 },
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "age", new FilterModel { Filter = 22,  FilterTo = 44, Type = FilterModelType.InRange, FilterType = FilterModelFilterType.Number } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Equal(3, result.RowsThisBlock.Count());
            Assert.Collection(result.RowsThisBlock, r => Assert.Equal(1, r.Id), r => Assert.Equal(2, r.Id), r => Assert.Equal(3, r.Id));
        }

        [Theory]
        [InlineData("2019-10-10 00:00:00", "2020-01-01 00:00:00", 2)]
        [InlineData("2018-01-01 00:00:00", "2021-11-15 00:00:00", 1, 2)]
        [InlineData("2019-10-10 00:00:00", "2021-11-15 04:11:44", 2, 3)]
        public void HaveExclusiveInRangeDateFilteringIfConfiguredToThisWay(string from, string to, params int[] expectedIds)
        {
            var users = new[]
            {
                new User { Id = 1, RegisteredOn = new DateTime(2019, 10, 10, 0, 0, 0) },
                new User { Id = 2, RegisteredOn = new DateTime(2019, 10, 10, 9, 22, 33)  },
                new User { Id = 3, RegisteredOn = new DateTime(2021, 11, 15, 0, 0, 0) },
                new User { Id = 4, RegisteredOn = new DateTime(2021, 11, 15, 4, 11, 44) },
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "registeredOn", new FilterModel { DateFrom = from, DateTo = to, Type = FilterModelType.InRange, FilterType = FilterModelFilterType.Date } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query, new() { InRangeExclusive = true });

            Assert.Equal(expectedIds.Length, result.RowsThisBlock.Count());
            Assert.True(result.RowsThisBlock.All(r => expectedIds.Contains(r.Id)));
        }

        [Fact]
        public void HaveInclusiveInRangeDateFilteringByDefault()
        {
            var users = new[]
            {
                new User { Id = 1, RegisteredOn = new DateTime(2019, 10, 10, 0, 0, 0) },
                new User { Id = 2, RegisteredOn = new DateTime(2019, 10, 10, 9, 22, 33)  },
                new User { Id = 3, RegisteredOn = new DateTime(2021, 11, 15, 0, 0, 0) },
                new User { Id = 4, RegisteredOn = new DateTime(2021, 11, 15, 4, 11, 44) },
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "registeredOn", new FilterModel { DateFrom = "2018-01-01 00:00:00", DateTo = "2021-11-15 00:00:00", Type = FilterModelType.InRange, FilterType = FilterModelFilterType.Date } }
                }
            };

            var result = _dbContext.Users.GetInfiniteRowModelBlock(query);

            Assert.Equal(3, result.RowsThisBlock.Count());
            Assert.Collection(result.RowsThisBlock, r => Assert.Equal(1, r.Id), r => Assert.Equal(2, r.Id), r => Assert.Equal(3, r.Id));
        }

        [Fact]
        public void FilterOverProjection()
        {
            var users = new[]
            {
                new User { Id = 1, FullName = "Ala Kowalska", Age = 15 },
                new User { Id = 2, FullName = "Jan Kowalski", Age = 20 },
                new User { Id = 3, FullName = "Ala Nowak", Age = 30 }
            };

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                const int currentYear = 2020;

                cfg.CreateMap<User, UserDto>()
                    .ForMember(d => d.Name, o => o.MapFrom(s => s.FullName))
                    .ForMember(d => d.BirthYear, o => o.MapFrom(s => currentYear - s.Age));
            });

            mapperConfiguration.AssertConfigurationIsValid();

            var mapper = mapperConfiguration.CreateMapper();

            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    { "name", new FilterModel { Filter = "Ala", Type = FilterModelType.Contains, FilterType = FilterModelFilterType.Text } },
                    { "birthYear", new FilterModel { Filter = 1990, Type = FilterModelType.Equals, FilterType = FilterModelFilterType.Number } }
                }
            };

            var result = mapper.ProjectTo<UserDto>(_dbContext.Users)
                .GetInfiniteRowModelBlock(query);

            Assert.Single(result.RowsThisBlock);
            Assert.Equal(3, result.RowsThisBlock.Single().Id);
        }

        public virtual void Dispose() => _dbContext.Dispose();
    }
}
