using System;
using System.Collections.Generic;
using NHibernate;

namespace nHibernatePermissionFilter.Filters
{
	public class AclStringToArrayFilter : FluentNHibernate.Mapping.FilterDefinition
	{
		const string FilterName = "AclStringToArrayFilter";

		public AclStringToArrayFilter()
		{
			this.WithName(FilterName).AddParameter("groupIds", NHibernateUtil.String);
		}

		public static IDisposable Enable(ISession session, IEnumerable<string> groupIds)
		{
			var joinedString = string.Join(" ", groupIds);
			session.EnableFilter(FilterName).SetParameter("groupIds", joinedString);
			return new Disposable(() => session.DisableFilter(FilterName));
		}
	}
}