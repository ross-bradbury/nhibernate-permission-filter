using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;

namespace nHibernatePermissionFilter.Filters
{
	public class AclMakeTextArrayFilter : FluentNHibernate.Mapping.FilterDefinition
	{
		const string FilterName = "AclMakeTextArrayFilter";

		public AclMakeTextArrayFilter()
		{
			this.WithName(FilterName).AddParameter("groupIds", NHibernateUtil.String);
		}

		public static IDisposable Enable(ISession session, IEnumerable<string> groupIds)
		{
			var sb = new StringBuilder();
			sb.Append("{");
			foreach (var id in groupIds)
			{
				var formatted = id.Replace("\\", "\\\\").Replace("\"", "\\\"");
				if (sb.Length > 1)
				{
					sb.Append(",");
				}
				sb.Append(formatted);
			}
			sb.Append("}");

			var value = sb.ToString();
			session.EnableFilter(FilterName).SetParameter("groupIds", value);
			return new Disposable(() => session.DisableFilter(FilterName));
		}
	}
}