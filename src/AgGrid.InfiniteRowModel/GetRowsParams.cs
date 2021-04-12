using System.Collections.Generic;

namespace AgGrid.InfiniteRowModel
{
    public class GetRowsParams
    {
        public int StartRow { get; set; }
        public int EndRow { get; set; }
        public IEnumerable<SortModel> SortModel { get; set; }
        public IDictionary<string, FilterModel> FilterModel { get; set; }
    }

    public class SortModel
    {
        public string Sort { get; set; }
        public string ColId { get; set; }
    }

    public static class SortModelSortDirection
    {
        public const string Ascending = "asc";
        public const string Descending = "desc";
    }

    public class FilterModel
    {
        public string FilterType { get; set; }
        public string Type { get; set; }
        public object Filter { get; set; }
        public double FilterTo { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }

        public string Operator { get; set; }
        public FilterModel Condition1 { get; set; }
        public FilterModel Condition2 { get; set; }
    }

    public static class FilterModelFilterType
    {
        public static IEnumerable<string> All => new[] { Text, Number, Date };

        public const string Text = "text";
        public const string Number = "number";
        public const string Date = "date";
    }

    public static class FilterModelType
    {
        public static IEnumerable<string> All => new[]
        {
            Equals, NotEqual, Contains, NotContains,
            StartsWith, EndsWith, LessThan, LessThanOrEqual,
            GreaterThan, GreaterThanOrEqual, InRange
        };

        new public const string Equals = "equals";
        public const string NotEqual = "notEqual";

        public const string Contains = "contains";
        public const string NotContains = "notContains";

        public const string StartsWith = "startsWith";
        public const string EndsWith = "endsWith";

        public const string LessThan = "lessThan";
        public const string LessThanOrEqual = "lessThanOrEqual";

        public const string GreaterThan = "greaterThan";
        public const string GreaterThanOrEqual = "greaterThanOrEqual";

        public const string InRange = "inRange";
    }

    public static class FilterModelOperator
    {
        public const string And = "AND";
        public const string Or = "OR";
    }
}
