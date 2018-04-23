using System;

namespace nHibernatePermissionFilter.Filters
{
	public class Disposable : IDisposable
	{
		private readonly Action _actionOnDispose;

		public Disposable(Action actionOnDispose)
		{
			_actionOnDispose = actionOnDispose;
		}

		public void Dispose()
		{
			_actionOnDispose?.Invoke();
		}
	}
}