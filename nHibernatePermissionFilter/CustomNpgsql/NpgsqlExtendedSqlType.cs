// From https://github.com/daanl/Fluent-NHibernate--PostgreSQL-column-array/tree/master/ConsoleApplication
// From http://daanleduc.nl/2014/02/08/fluent-nhibernate-postgresql-column-array/
using System.Data;
using NHibernate.SqlTypes;
using NpgsqlTypes;

namespace ConsoleApplication.CustomNpgsql
{
    public class NpgsqlExtendedSqlType : SqlType
    {
        private readonly NpgsqlDbType _npgDbType;
        
        public NpgsqlExtendedSqlType(DbType dbType, NpgsqlDbType npgDbType) : base(dbType)
        {
            _npgDbType = npgDbType;
        }

        public NpgsqlExtendedSqlType(DbType dbType, NpgsqlDbType npgDbType, int length) : base(dbType, length)
        {
            _npgDbType = npgDbType;
        }

        public NpgsqlExtendedSqlType(DbType dbType, NpgsqlDbType npgDbType, byte precision, byte scale) : base(dbType, precision, scale)
        {
            _npgDbType = npgDbType;
        }

        public NpgsqlDbType NpgDbType
        {
            get { return _npgDbType; }
        }
    }
}