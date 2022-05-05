using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AgGrid.InfiniteRowModel.EntityFrameworkCore
{
    public static class QueryableExtensions
    {
        public static async Task<InfiniteRowModelResult<T>> GetInfiniteRowModelBlockAsync<T>(this IQueryable<T> queryable, string getRowsParamsJson, InfiniteRowModelOptions options = null)
            => await GetInfiniteRowModelBlockAsync(queryable, InfiniteScroll.DeserializeGetRowsParams(getRowsParamsJson), options);

        public static async Task<InfiniteRowModelResult<T>> GetInfiniteRowModelBlockAsync<T>(this IQueryable<T> queryable, GetRowsParams getRowsParams, InfiniteRowModelOptions options = null)
        {
            var rows = await InfiniteScroll.ToQueryableRows(queryable, getRowsParams, options).ToListAsync();
            return InfiniteScroll.ToRowModelResult(getRowsParams, rows);
        }
    }
}
