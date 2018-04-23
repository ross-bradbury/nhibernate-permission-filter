// From https://github.com/daanl/Fluent-NHibernate--PostgreSQL-column-array/tree/master/ConsoleApplication
// From http://daanleduc.nl/2014/02/08/fluent-nhibernate-postgresql-column-array/
using System.Data;
using NHibernate.Dialect;

namespace ConsoleApplication.CustomNpgsql
{
    public class CustomPostgreSQL82Dialect : PostgreSQL82Dialect
    {
        public CustomPostgreSQL82Dialect()
        {
            RegisterColumnType(DbType.Object, "text[]");
        }
    }
}