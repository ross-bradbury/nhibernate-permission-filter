using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ConsoleApplication.CustomNpgsql;
using FluentNHibernate.Cfg;
using nHibernatePermissionFilter.Domain;
using nHibernatePermissionFilter.PonyApp.FluentFilters;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Type;
using Configuration = NHibernate.Cfg.Configuration;

namespace nHibernatePermissionFilter
{
	class Program
	{
		static void Main(string[] args)
		{
			//var sessionFactory = SessionFactory();
			var sessionFactory = _sessionFactory.Value;

			using (var ses = sessionFactory.OpenSession())
			{
				var unfilteredCount = ses.Query<Project>().Take(100).ToList();
				Console.WriteLine($"Unfiltered count {unfilteredCount.Count}");

				//ses.EnableFilter("aclFilter").SetParameterList("groupIds", new[] { "256a2ceb-2b96-4034-a557-3e20697c7bed", "85f3f476-e98a-45f7-911a-3221a667c769" });
				//ses.EnableFilter("aclFilter").SetParameter("groupIds", "'{\"256a2ceb-2b96-4034-a557-3e20697c7bed\",\"85f3f476-e98a-45f7-911a-3221a667c769\"}'");
				//ses.EnableFilter("aclFilter").SetParameter("groupIds", new[] { "256a2ceb-2b96-4034-a557-3e20697c7bed", "85f3f476-e98a-45f7-911a-3221a667c769" });
				ses.EnableFilter("publicOrMatchesId").SetParameter("justId", "c4d745c2-9150-4c47-9d9c-58773dde0441");

				var filteredCount = ses.Query<Project>().Take(100).ToList().Count;
				Console.WriteLine($"Filtered count {filteredCount}");
			}
		}


		private static ISessionFactory SessionFactory()
		{
			var Config = new Configuration();
			Config.DataBaseIntegration(setings =>
			{
				setings.ConnectionStringName = "PostgresDbContext";
				// becareful its ConnectionStringName I am giving , not the full ConnectionString
				setings.Driver<NpgsqlDriver>();
				setings.Dialect<PostgreSQL82Dialect>(); // mine is PostgreSQL 9.6.1/9.5.5
#if DEBUG
				setings.LogSqlInConsole = true;
				setings.LogFormattedSql = true;
#endif
			});
			Config.AddAssembly(Assembly.GetExecutingAssembly());


			var sessionFactory = Config.BuildSessionFactory();
			return sessionFactory;
		}

		private static Lazy<ISessionFactory> _sessionFactory = new Lazy<ISessionFactory>(() =>
		{
			var configuration = _configuration.Value;

			var sessionFactory = Fluently.Configure(configuration)
				//.Mappings(m => m.FluentMappings.Add(typeof(AclFilter)))
				.Mappings(m =>
					m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
				//.Mappings(m =>
				//	m.HbmMappings
				//		.AddFromAssemblyOf<Project>())
				.BuildSessionFactory();

			return sessionFactory;
		});

		private static Lazy<Configuration> _configuration = new Lazy<Configuration>(() =>
		{
			var configuration = new Configuration();

			configuration.SetProperty(
				NHibernate.Cfg.Environment.ConnectionString,
				ConfigurationManager.ConnectionStrings["PostgresDbContext"].ConnectionString
			);

			configuration.Configure();

			return configuration;
		});

	}

	namespace PonyApp.FluentFilters
	{
		public class PublicOrMatchesIdFilter : FluentNHibernate.Mapping.FilterDefinition
		{
			public PublicOrMatchesIdFilter()
			{
				this.WithName("publicOrMatchesId").AddParameter("justId", NHibernate.NHibernateUtil.String);
			}
		}
	}
}

// UGH -- I either cannot escape the array constructor or it wants to prefix ARRAY with the table alias or the ?| operator it thinks the ? is a parameter placeholder...!!!

/*
 * 
 * 
create table project (n int not null, id text not null primary key, dummy text);

insert into project (n, id, dummy)
select
    n,
    gen_random_uuid(),
    (select array_agg(gen_random_uuid())::text from generate_series(1,20) as Y(n)) as incompressible_text
from generate_series(1,1000000) as X(n);

alter table project add acls jsonb;

update project set acls = 
	json_build_object(
		'isPublic',
		case when n % 256000 = 0 then true else false end,
		'allowacls',
		json_build_array(
			gen_random_uuid(),
			case when n % 123473 = 0 THEN '256a2ceb-2b96-4034-a557-3e20697c7bed' else gen_random_uuid() end,
			case when n % 12347 = 0 THEN '18d363c1-ebb6-4469-b032-c39f93ea8de6' else gen_random_uuid() end,
			case when n % 1234 = 0 THEN 'cbef893e-0729-4fee-bf4f-d51762fccc64' else gen_random_uuid() end,
			gen_random_uuid()
			)
		);
	from project
	limit 3;
	

create table project (n int not null, id text not null primary key, dummy text);
alter table project add is_public boolean;
alter table project add allow_acls text[];

insert into project (n, id, dummy, is_public, allow_acls)
select
    n,
    gen_random_uuid(),
    (select array_agg(gen_random_uuid())::text from generate_series(1,20) as Y(n)) as incompressible_text,
	case when n % 256000 = 0 then true else false end as is_public,
	ARRAY[
		gen_random_uuid(),
		case when n % 123473 = 0 THEN '256a2ceb-2b96-4034-a557-3e20697c7bed' else gen_random_uuid() end,
		case when n % 12347 = 0 THEN '18d363c1-ebb6-4469-b032-c39f93ea8de6' else gen_random_uuid() end,
		case when n % 1234 = 0 THEN 'cbef893e-0729-4fee-bf4f-d51762fccc64' else gen_random_uuid() end,
		gen_random_uuid()
	] as allow_acls
from generate_series(1,1000000) as X(n);

create index on project (is_public);
create index on project using GIN (allow_acls);


EXPLAIN (ANALYZE, COSTS, VERBOSE, BUFFERS) 
select n, id, is_public from project where (is_public OR allow_acls && ARRAY['256a2ceb-2b96-4034-a557-3e20697c7bed','85f3f476-e98a-45f7-911a-3221a667c769']) AND n between 740500 and 741000;


*/