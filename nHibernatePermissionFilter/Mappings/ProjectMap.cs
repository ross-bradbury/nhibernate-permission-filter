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


			this.ApplyFilter("aclNormalizedTableFilter", "(is_public OR Id in (SELECT pra.id FROM project_read_acls as pra WHERE pra.member_id IN ( :groupIds )))");

			this.ApplyFilter("AclMakeTextArrayFilter", "(is_public OR allow_acls && (select make_text_array(:groupIds)))");

			this.ApplyFilter("AclStringToArrayFilter", "(is_public OR allow_acls && (select string_to_array(:groupIds, ' ')))");

			this.ApplyFilter("publicOrMatchesId", "(is_public OR :justId = Id)");
		}
	}
}
