using ConsoleApplication.CustomNpgsql;
using FluentNHibernate.Mapping;
using nHibernatePermissionFilter.Domain;

namespace nHibernatePermissionFilter.Mappings
{
	class ProjectMap : ClassMap<Project>
	{
		public ProjectMap()
		{
			this.Table("project");

			Id(x => x.Id);

			Map(x => x.Number).Column("n");

			Map(x => x.Dummy);

			Map(x => x.IsPublic).Column("is_public");

			Map(x => x.AllowACLs)
				.Column("allow_acls")
				.CustomType<PostgresSqlArrayType>()
				.CustomSqlType("text[]");

			//this.ApplyFilter("aclFilter", "(is_public OR allow_acls && :groupIds)");
			this.ApplyFilter("publicOrMatchesId", "(is_public OR :justId = Id)");
		}
	}
}
