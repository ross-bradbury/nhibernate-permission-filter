using System;
using System.Collections.Generic;
using System.Linq;
using NpgsqlTypes;

namespace ConsoleApplication.CustomNpgsql
{
	/// <summary>
	/// Handles Postgres text[] columns mapped to <see cref="System.Collections.Generic.ISet{T}">ISet&lt;String&gt;</see>
	/// </summary>
	/// <remarks>
	/// When written to the database the text values will be sorted.
	/// </remarks>
	public class PostgresSqlTextSetType : PostgresSqlArrayType<string>
	{
		public PostgresSqlTextSetType() : base(NpgsqlDbType.Text)
		{
		}

		public override Type ReturnedType { get; } = typeof(ISet<string>);

		protected override object ReaderToModel(IList<string> value)
		{
			return new HashSet<string>(value);
		}

		protected override object ModelToParameter(object value)
		{
			var set = (ISet<string>) value;
			var sorted = set.OrderBy(t => t).ToArray();
			return sorted;
		}

		public override object DeepCopy(object value)
		{
			return new HashSet<string>((ISet<string>)value);
		}
	}
}