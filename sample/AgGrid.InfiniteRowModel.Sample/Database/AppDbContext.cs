using AgGrid.InfiniteRowModel.Sample.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgGrid.InfiniteRowModel.Sample.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
