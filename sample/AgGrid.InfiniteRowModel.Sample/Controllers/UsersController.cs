using AgGrid.InfiniteRowModel.EntityFrameworkCore;
using AgGrid.InfiniteRowModel.Sample.Database;
using AgGrid.InfiniteRowModel.Sample.Entities;
using Bogus;
using Bogus.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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
        public async Task<InfiniteRowModelResult<User>> Get(string query)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .GetInfiniteRowModelBlockAsync(query);
        }

        [HttpPost]
        public async Task Add()
        {
            var user = new Faker<User>()
                .RuleFor(u => u.RegisteredOn, _ => DateTime.Now)
                .RuleFor(u => u.FullName, f => f.Name.FullName().OrNull(f, 0.2f))
                .RuleFor(u => u.Age, f => f.Random.Number(10, 90))
                .RuleFor(u => u.IsVerified, f => f.Random.Bool())
                .Generate();

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
