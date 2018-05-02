using NpgsqlTypes;

namespace ConsoleApplication.CustomNpgsql
{
	/// <summary>
	/// Handles Postgres text[] columns mapped to <see cref="System.Collections.Generic.IList{T}">IList&lt;String&gt;</see>
	/// </summary>
	public class PostgresSqlTextArrayType : PostgresSqlArrayType<string>
	{
		public PostgresSqlTextArrayType() : base(NpgsqlDbType.Text)
		{
		}
	}
}