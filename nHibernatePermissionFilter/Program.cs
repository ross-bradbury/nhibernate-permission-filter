using System;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using FluentNHibernate.Cfg;
using nHibernatePermissionFilter.Domain;
using nHibernatePermissionFilter.Filters;
using NHibernate;
using Configuration = NHibernate.Cfg.Configuration;

namespace nHibernatePermissionFilter
{
	class Program
	{
		static void Main(string[] args)
		{
			//var sessionFactory = SessionFactory();
			var sessionFactory = SessionFactory.Value;

			using (var ses = sessionFactory.OpenSession())
			{
				PrepareSchemaAndData(ses.Connection);

				//var test = ses.Query<Project>().Take(1).Single();
				//test.AllowACLs.Add(Guid.Empty.ToString());
				//ses.Flush();

				ses.EnableFilter("publicOrMatchesId").SetParameter("justId", "c4d745c2-9150-4c47-9d9c-58773dde0441");
				RunQuery(ses, "WARMUP");
				ses.DisableFilter("publicOrMatchesId");

				RunQuery(ses, "first 100 random");


				var groupIds = new[] { "256a2ceb-2b96-4034-a557-3e20697c7bed", "85f3f476-e98a-45f7-911a-3221a667c769" };

				using (AclNormalizedTableFilter.Enable(ses, groupIds))
				{
					RunQuery(ses, "Using normalized table (would be faster if not checking is_public so there would not be an OR)");
				}

				using (AclMakeTextArrayFilter.Enable(ses, groupIds))
				{
					RunQuery(ses, "Using array overlap with custom function make_text_array");
				}

				using (AclStringToArrayFilter.Enable(ses, groupIds))
				{
					RunQuery(ses, "Using array overlap with string_to_array");
				}
			}
		}

		private static void PrepareSchemaAndData(DbConnection connection)
		{
			using (var command = connection.CreateCommand())
			{
				// HACK: Figure out if we already made the tables
				command.CommandText = "SELECT count(1) from project_read_acls_fornhpermfilt";
				try
				{
					var count = Convert.ToInt64(command.ExecuteScalar());

					if (count >= 1)
						return;
				}
				catch (DbException)
				{

				}
			}

			using (var command = connection.CreateCommand())
			using (var tx = connection.BeginTransaction())
			{
				command.Transaction = tx;

				command.CommandTimeout = 10000;
				command.CommandText =
					@"create extension if not exists pgcrypto; 
create table if not exists project_fornhpermfilt (n int not null, id text not null primary key, dummy text, is_public boolean, allow_acls text[]);
create index on project_fornhpermfilt (is_public) where is_public;
create index on project_fornhpermfilt using GIN (allow_acls);";
				command.ExecuteNonQuery();

				int increment = 100000;
				for (int i = increment; i <= 1000000; i += increment)
				{
					var start = i - increment + 1;
					var end = i;
					var watch = Stopwatch.StartNew();
					var sql = $@"insert into project_fornhpermfilt (n, id, dummy, is_public, allow_acls)
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
		case when n % 123 = 0 THEN 'ffcfba8f-30d9-4a13-a4cc-a6636c7e9e79' else gen_random_uuid() end
	] as allow_acls
from generate_series({start},{end}) as X(n);";

					Console.WriteLine(sql);
					command.CommandText = sql;
					command.ExecuteNonQuery();
					Console.WriteLine(watch.ElapsedMilliseconds + " ms");
				}

				var postCommands = new string[]
				{
					//@"create index on project_fornhpermfilt (is_public) where is_public;",

					//@"create index on project_fornhpermfilt using GIN (allow_acls);",

					@"CREATE OR REPLACE FUNCTION public.make_text_array(text) RETURNS text[] LANGUAGE sql IMMUTABLE STRICT AS $function$SELECT $1::text[]$function$;",

					@"select id, unnest(allow_acls) as member_id into project_read_acls_fornhpermfilt from project_fornhpermfilt;",

					@"alter table project_read_acls_fornhpermfilt add primary key (member_id, id);",
				};

				foreach (var cmdText in postCommands)
				{
					command.CommandText = cmdText;
					Console.WriteLine(cmdText);
					command.ExecuteNonQuery();
				}

				tx.Commit();
			}

			using (var command = connection.CreateCommand())
			{
				command.CommandTimeout = 10000;
				command.CommandText = "VACUUM ANALYZE;";
				Console.WriteLine(command.CommandText);
				command.ExecuteNonQuery();
			}
		}

		private static void RunQuery(ISession ses, string comment = "")
		{
			for (int i = 1; i <= 3; i += 1)
			{
				Thread.Sleep(1000);
				var watch = Stopwatch.StartNew();
				var filtered = ses.Query<Project>().Take(100).ToList();
				var filteredCount = filtered.Count;
				watch.Stop();
				Console.WriteLine($"Filtered count {filteredCount} in {watch.ElapsedMilliseconds} ms ({watch.ElapsedTicks} ticks) for execution {i}");
				Console.WriteLine($"^^^^ {comment}\n");
			}
			Console.WriteLine();
		}

		private static readonly Lazy<ISessionFactory> SessionFactory = new Lazy<ISessionFactory>(() =>
		{
			var configuration = Configuration.Value;

			var sessionFactory = Fluently.Configure(configuration)
				.Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
				.BuildSessionFactory();

			return sessionFactory;
		});

		private static readonly Lazy<Configuration> Configuration = new Lazy<Configuration>(() =>
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
}

// UGH -- I either cannot escape the array constructor or it wants to prefix ARRAY with the table alias or the ?| operator it thinks the ? is a parameter placeholder...!!!


// EXPLAIN (ANALYZE, COSTS, VERBOSE, BUFFERS) select n, id, is_public from project_fornhpermfilt where (is_public OR allow_acls && ARRAY['256a2ceb-2b96-4034-a557-3e20697c7bed','85f3f476-e98a-45f7-911a-3221a667c769']) AND n between 740500 and 741000;
