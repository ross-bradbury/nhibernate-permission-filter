using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;

namespace nHibernatePermissionFilter.Filters
{
	public class AclNormalizedTableFilter : FluentNHibernate.Mapping.FilterDefinition
	{
		const string FilterName = "aclNormalizedTableFilter";

		public AclNormalizedTableFilter()
		{
			this.WithName(FilterName).AddParameter("groupIds", NHibernateUtil.String);
		}

		public static IDisposable Enable(ISession session, IEnumerable<string> groupIds)
		{
			session.EnableFilter(FilterName).SetParameterList("groupIds", groupIds.ToList());
			return new Disposable(() => session.DisableFilter(FilterName));
		}
	}
}