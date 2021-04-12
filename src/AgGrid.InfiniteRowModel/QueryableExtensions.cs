using System;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace AgGrid.InfiniteRowModel
{
    public static class QueryableExtensions
    {
        public static InfiniteRowModelResult<T> GetInfiniteRowModelBlock<T>(this IQueryable<T> queryable, string getRowsParamsJson)
        {
            var getRowsParams = JsonSerializer.Deserialize<GetRowsParams>(getRowsParamsJson, new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return queryable.GetInfiniteRowModelBlock(getRowsParams);
        }

        public static InfiniteRowModelResult<T> GetInfiniteRowModelBlock<T>(this IQueryable<T> queryable, GetRowsParams getRowsParams)
        {
            var takeCount = getRowsParams.EndRow - getRowsParams.StartRow;

            var rows = queryable
                .Filter(getRowsParams)
                .Sort(getRowsParams)
                .Skip(getRowsParams.StartRow)
                .Take(takeCount + 1)
                .ToList();

            var reachedEnd = rows.Count <= takeCount;

            return new InfiniteRowModelResult<T>
            {
                RowsThisBlock = rows.Take(takeCount).ToList(),
                LastRow = reachedEnd ? getRowsParams.StartRow + rows.Count : null
            };
        }

        private static IQueryable<T> Filter<T>(this IQueryable<T> queryable, GetRowsParams getRowsParams)
        {
            foreach (var kvp in getRowsParams.FilterModel)
            {
                var colId = kvp.Key;
                var filterModel = kvp.Value;

                if (string.IsNullOrEmpty(filterModel.Operator))
                {
                    var predicate = GetPredicate(colId, filterModel, 0);
                    var args = GetWhereArgs(filterModel);

                    queryable = queryable.Where(predicate, args: args);
                }
                else
                {
                    var predicateLeftSide = GetPredicate(colId, filterModel.Condition1, 0);
                    var argsLeftSide = GetWhereArgs(filterModel.Condition1);

                    var rightSideArgsIndex = filterModel.Condition1.Type == FilterModelType.InRange ? 2 : 1;

                    var predicateRightSide = GetPredicate(colId, filterModel.Condition2, rightSideArgsIndex);
                    var argsRightSide = GetWhereArgs(filterModel.Condition2);

                    var predicate = $"{predicateLeftSide} {filterModel.Operator} {predicateRightSide}";
                    var args = argsLeftSide.Concat(argsRightSide).ToArray();

                    queryable = queryable.Where(predicate, args: args);
                }
            }

            return queryable;
        }

        private static object[] GetWhereArgs(FilterModel filterModel)
        {
            return filterModel switch
            {
                { FilterType: FilterModelFilterType.Text } => new object[] { GetString(filterModel.Filter) },

                { FilterType: FilterModelFilterType.Number, Type: FilterModelType.InRange } => new object[] { GetNumber(filterModel.Filter), filterModel.FilterTo },
                { FilterType: FilterModelFilterType.Number } => new object[] { GetNumber(filterModel.Filter) },

                { FilterType: FilterModelFilterType.Date, Type: FilterModelType.InRange } => new object[] { GetDate(filterModel.DateFrom), GetDate(filterModel.DateTo) },
                { FilterType: FilterModelFilterType.Date } => new object[] { GetDate(filterModel.DateFrom) },

                { FilterType: FilterModelFilterType.Boolean } => new object[] { GetBoolean(filterModel.Filter) },

                _ => throw new ArgumentException($"Unsupported {nameof(FilterModel.FilterType)} value ({filterModel.FilterType}). Supported values: {string.Join(", ", FilterModelFilterType.All)}.")
            };
        }

        private static string GetString(object element)
            => (element as JsonElement?)?.GetString() ?? (string)element;

        private static double? GetNumber(object element)
            => (element as JsonElement?)?.GetDouble() ?? (double?)element;

        private static DateTime GetDate(string dateString)
            => DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        private static bool GetBoolean(object element)
            => (element as JsonElement?)?.GetBoolean() ?? (bool)element;

        private static string GetPredicate(string colId, FilterModel filterModel, int index)
        {
            var propertyName = colId.ToPascalCase();

            return filterModel.Type switch
            {
                FilterModelType.Equals => $"{propertyName} == @{index}",
                FilterModelType.NotEqual => $"{propertyName} != @{index}",

                FilterModelType.Contains => $"{propertyName}.Contains(@{index})",
                FilterModelType.NotContains => $"!{propertyName}.Contains(@{index})",

                FilterModelType.StartsWith => $"{propertyName}.StartsWith(@{index})",
                FilterModelType.EndsWith => $"{propertyName}.EndsWith(@{index})",

                FilterModelType.LessThan => $"{propertyName} < @{index}",
                FilterModelType.LessThanOrEqual => $"{propertyName} <= @{index}",

                FilterModelType.GreaterThan => $"{propertyName} > @{index}",
                FilterModelType.GreaterThanOrEqual => $"{propertyName} >= @{index}",

                FilterModelType.InRange => $"{propertyName} >= @{index} AND {propertyName} <= @{index + 1}",

                _ => throw new ArgumentException($"Unsupported {nameof(FilterModel.Type)} value ({filterModel.Type}). Supported values: {string.Join(", ", FilterModelType.All)}.")
            };
        }

        private static IQueryable<T> Sort<T>(this IQueryable<T> queryable, GetRowsParams getRowsParams)
        {
            var orderingParts = getRowsParams.SortModel.Select(s => $"{s.ColId.ToPascalCase()} {s.Sort}");
            var ordering = string.Join(", ", orderingParts);

            if (ordering.Length == 0)
            {
                return queryable;
            }

            return queryable.OrderBy(ordering);
        }
    }
}
