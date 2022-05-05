using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleToAttribute("AgGrid.InfiniteRowModel.EntityFrameworkCore")]
namespace AgGrid.InfiniteRowModel
{
    public static class QueryableExtensions
    {
        public static InfiniteRowModelResult<T> GetInfiniteRowModelBlock<T>(this IQueryable<T> queryable, string getRowsParamsJson, InfiniteRowModelOptions options = null)
            => GetInfiniteRowModelBlock(queryable, InfiniteScroll.DeserializeGetRowsParams(getRowsParamsJson), options);

        public static InfiniteRowModelResult<T> GetInfiniteRowModelBlock<T>(this IQueryable<T> queryable, GetRowsParams getRowsParams, InfiniteRowModelOptions options = null)
        {
            var rows = InfiniteScroll.ToQueryableRows(queryable, getRowsParams, options).ToList();
            return InfiniteScroll.ToRowModelResult(getRowsParams, rows);
        }
    }
}
