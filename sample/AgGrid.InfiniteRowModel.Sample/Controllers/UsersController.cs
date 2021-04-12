using AgGrid.InfiniteRowModel.Sample.Database;
using AgGrid.InfiniteRowModel.Sample.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AgGrid.InfiniteRowModel.Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public UsersController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public InfiniteRowModelResult<User> Get(string query)
        {
            return _dbContext.Users.GetInfiniteRowModelBlock(query);
        }
    }
}
