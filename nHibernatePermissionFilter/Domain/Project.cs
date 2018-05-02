using System.Collections.Generic;

namespace nHibernatePermissionFilter.Domain
{
	class Project
	{
		public virtual string Id { get; set; }
		public virtual int Number { get; set; }
		public virtual string Dummy { get; set; }
		public virtual bool IsPublic { get; set; }
		public virtual ISet<string> AllowACLs { get; set; }
	}
}
