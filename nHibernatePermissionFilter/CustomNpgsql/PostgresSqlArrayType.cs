// From https://github.com/daanl/Fluent-NHibernate--PostgreSQL-column-array/tree/master/ConsoleApplication
// From http://daanleduc.nl/2014/02/08/fluent-nhibernate-postgresql-column-array/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using NpgsqlTypes;

namespace ConsoleApplication.CustomNpgsql
{
	/// <summary>
	/// Handles Postgres array columns mapped to <see cref="IList{T}" />
	/// </summary>
	public abstract class PostgresSqlArrayType<T> : IUserType
	{
		private readonly NpgsqlDbType _columnType;

		protected PostgresSqlArrayType(NpgsqlDbType columnType)
		{
			_columnType = columnType;
		}

		bool IUserType.Equals(object x, object y)
		{
			return AreEqual(x, y);
		}

		protected virtual bool AreEqual(object x, object y)
		{
			if (x == null && y != null) return false;
			if (x != null && y == null) return false;
			if (x == null) return true;

			var xArray = (IEnumerable<T>) ModelToParameter(x);
			var yArray = (IEnumerable<T>) ModelToParameter(y);
			var equal = xArray.SequenceEqual(yArray);
			return equal;
		}

		public int GetHashCode(object x)
		{
			return (x == null) ? 0 : x.GetHashCode();
		}

		protected virtual object ReaderToModel(IList<T> value)
		{
			return value.ToList();
		}

		protected virtual object ModelToParameter(object value)
		{
			var list = (IList<T>)value;
			return list.ToArray();
		}

		public virtual object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
		{
			var index = rs.GetOrdinal(names[0]);

			if (rs.IsDBNull(index))
			{
				return null;
			}

			var res = rs.GetValue(index) as IList<T>;
			if (res != null)
			{
				return ReaderToModel(res);
			}

			throw new NotImplementedException();
		}

		public virtual void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
		{
			var parameter = ((IDbDataParameter)cmd.Parameters[index]);
			if (value == null)
			{
				parameter.Value = DBNull.Value;
			}
			else
			{
				parameter.Value = ModelToParameter(value);
			}
		}

		public virtual object DeepCopy(object value)
		{
			return new List<T>((IList<T>) value);
		}

		public object Replace(object original, object target, object owner)
		{
			return DeepCopy(original);
		}

		public object Assemble(object cached, object owner)
		{
			return cached;
		}

		public object Disassemble(object value)
		{
			return value;
		}
        
		public SqlType[] SqlTypes
		{
			get
			{
				var sqlTypes = new SqlType[]
				{
					new NpgsqlExtendedSqlType(
						DbType.Object, 
						NpgsqlDbType.Array | _columnType
					)
				};

				return sqlTypes;
			}
		}

		public virtual Type ReturnedType
		{
			get { return typeof(IList<T>); }
		}

		public bool IsMutable { get; private set; } = true;
	}
}