namespace nHibernatePermissionFilter.Filters
{
	public class PublicOrMatchesIdFilter : FluentNHibernate.Mapping.FilterDefinition
	{
		public PublicOrMatchesIdFilter()
		{
			this.WithName("publicOrMatchesId").AddParameter("justId", NHibernate.NHibernateUtil.String);
		}
	}
}