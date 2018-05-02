// From https://github.com/daanl/Fluent-NHibernate--PostgreSQL-column-array/tree/master/ConsoleApplication
// From http://daanleduc.nl/2014/02/08/fluent-nhibernate-postgresql-column-array/
using System.Data;
using NHibernate.SqlTypes;
using NpgsqlTypes;

namespace ConsoleApplication.CustomNpgsql
{
    public class NpgsqlExtendedSqlType : SqlType
    {
		public NpgsqlExtendedSqlType(DbType dbType, NpgsqlDbType npgDbType) : base(dbType)
        {
            NpgDbType = npgDbType;
        }

        public NpgsqlExtendedSqlType(DbType dbType, NpgsqlDbType npgDbType, int length) : base(dbType, length)
        {
            NpgDbType = npgDbType;
        }

        public NpgsqlExtendedSqlType(DbType dbType, NpgsqlDbType npgDbType, byte precision, byte scale) : base(dbType, precision, scale)
        {
            NpgDbType = npgDbType;
        }

		public NpgsqlDbType NpgDbType { get; }
	}
}