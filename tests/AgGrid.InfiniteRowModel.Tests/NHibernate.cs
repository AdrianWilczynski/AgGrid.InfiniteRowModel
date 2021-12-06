using AgGrid.InfiniteRowModel.Sample.Entities;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.SqlCommand;
using NHibernate.Tool.hbm2ddl;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AgGrid.InfiniteRowModel.Tests
{
    public class NHibernate
    {
		private const string _dbFile = "nhibernate_test.db";

		private readonly ITestOutputHelper _output;

		public NHibernate(ITestOutputHelper output)
		{
			_output = output;
		}

		private static ISessionFactory CreateSessionFactory(ITestOutputHelper output)
			=> Fluently.Configure()
				.Database(SQLiteConfiguration.Standard.UsingFile(_dbFile))
				.Mappings(m => m.FluentMappings.Add<UserMap>())
				.ExposeConfiguration(config =>
                {
					BuildSchema(config);
					config.SetInterceptor(new NHibernateLoggerInterceptor(output));
                })
				.BuildSessionFactory();

		private static void BuildSchema(Configuration config)
		{
			if (File.Exists(_dbFile))
            {
				File.Delete(_dbFile);
			}

			new SchemaExport(config)
			  .Create(false, true);
		}

		[Fact]
		public void BasicFilteringAndSorting()
        {
			var sessionFactory = CreateSessionFactory(_output);
			using var session = sessionFactory.OpenSession();

			using (var transaction = session.BeginTransaction())
            {
				session.Save(new User { Id = 1, FullName = "Jan Kowalski" });
				session.Save(new User { Id = 2, FullName = "Ala Nowak" });
				session.Save(new User { Id = 3, FullName = "Bartosz Kowalski" });

				transaction.Commit();
			}

			var query = new GetRowsParams
			{
				StartRow = 0,
				EndRow = 10,
				FilterModel = new Dictionary<string, FilterModel>
				{
					{ "fullName", new FilterModel { Filter = "Kowalski", Type = FilterModelType.Contains, FilterType = FilterModelFilterType.Text } }
				},
				SortModel = new[]
				{
					new SortModel
					{
						ColId = "fullName",
						Sort = SortModelSortDirection.Ascending
					}
				}
			};

			var result = session.Query<User>()
				.GetInfiniteRowModelBlock(query);

			Assert.Equal(2, result.RowsThisBlock.Count());
			Assert.Equal(3, result.RowsThisBlock.ElementAt(0).Id);
			Assert.Equal(1, result.RowsThisBlock.ElementAt(1).Id);
		}
	}

	public class UserMap : ClassMap<User>
	{
		public UserMap()
		{
			Id(u => u.Id);
			Map(u => u.FullName);
			Map(u => u.RegisteredOn);
			Map(u => u.Age);
			Map(u => u.IsVerified);
		}
	}

	public class NHibernateLoggerInterceptor : EmptyInterceptor
	{
		private readonly ITestOutputHelper _output;

        public NHibernateLoggerInterceptor(ITestOutputHelper output)
        {
            _output = output;
        }

		public override SqlString OnPrepareStatement(SqlString sql)
		{
			_output.WriteLine(sql.ToString());

			return base.OnPrepareStatement(sql);
		}
	}
}
