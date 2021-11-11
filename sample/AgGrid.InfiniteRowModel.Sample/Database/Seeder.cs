using AgGrid.InfiniteRowModel.Sample.Entities;
using Bogus;
using Bogus.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AgGrid.InfiniteRowModel.Sample.Database
{
    public class Seeder
    {
        private readonly AppDbContext _dbContext;

        public Seeder(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Seed()
        {
            _dbContext.Database.Migrate();

            if (_dbContext.Users.Any())
            {
                return;
            }

            var faker = new Faker<User>()
                .RuleFor(u => u.FullName, f => f.Name.FullName().OrNull(f, 0.2f))
                .RuleFor(u => u.RegisteredOn, f => f.Date.Past(10))
                .RuleFor(u => u.Age, f => f.Random.Number(10, 90))
                .RuleFor(u => u.IsVerified, f => f.Random.Bool());

            var users = faker.Generate(1000);

            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();
        }
    }
}
