using AgGrid.InfiniteRowModel.Sample.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AgGrid.InfiniteRowModel.Tests
{
    public class Validation : IDisposable
    {
        private readonly AppDbContext _dbContext = InMemory.GetDbContext();

        [Fact]
        public void ValidateColIds()
        {
            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                FilterModel = new Dictionary<string, FilterModel>
                {
                    {
                        "invalidColId",
                        new FilterModel
                        {
                            Filter = "test",
                            Type = FilterModelType.Contains,
                            FilterType = FilterModelFilterType.Text
                        }
                    },
                    {
                        "anotherInvalidColId",
                        new FilterModel
                        {
                            Filter = 5,
                            Type = FilterModelType.Equals,
                            FilterType = FilterModelFilterType.Number
                        }
                    }
                }
            };

            var exception = Assert.Throws<ArgumentException>(() =>_dbContext.Users.GetInfiniteRowModelBlock(query));
            Assert.Contains("colId", exception.Message);
            Assert.Contains(query.FilterModel.First().Key, exception.Message);
            Assert.Contains(query.FilterModel.Last().Key, exception.Message);
        }

        [Fact]
        public void ValidateSortOrder()
        {
            var query = new GetRowsParams
            {
                StartRow = 0,
                EndRow = 10,
                SortModel = new[]
                {
                    new SortModel
                    {
                        ColId = "fullName",
                        Sort = "madeUpSortOrder"
                    }
                }
            };

            var exception = Assert.Throws<ArgumentException>(() => _dbContext.Users.GetInfiniteRowModelBlock(query));
            Assert.Contains(nameof(SortModel.Sort), exception.Message);
            Assert.Contains(query.SortModel.First().Sort, exception.Message);
        }

        [Fact]
        public void ValidateOperator()
        {
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
                            Operator = "madeUpOperator",
                            Condition1 = new FilterModel
                            {
                                FilterType = FilterModelFilterType.Text,
                                Type = FilterModelType.Contains,
                                Filter = "test"
                            },
                            Condition2 = new FilterModel
                            {
                                FilterType = FilterModelFilterType.Text,
                                Type = FilterModelType.Contains,
                                Filter = "test"
                            }
                        }
                    },
                }
            };

            var exception = Assert.Throws<ArgumentException>(() => _dbContext.Users.GetInfiniteRowModelBlock(query));
            Assert.Contains(nameof(FilterModel.Operator), exception.Message);
            Assert.Contains(query.FilterModel.First().Value.Operator, exception.Message);
        }

        public void Dispose() => _dbContext.Dispose();
    }
}
