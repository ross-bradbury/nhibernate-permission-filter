using System;
using System.Data;
using System.Data.Common;
using NHibernate;
using NHibernate.Driver;
using NHibernate.SqlTypes;
using Npgsql;

namespace ConsoleApplication.CustomNpgsql
{
    public class NpgsqlDriverExtended : NpgsqlDriver
    {
	    protected override void InitializeParameter(DbParameter dbParam, string name, SqlType sqlType)
	    {
			if (sqlType is NpgsqlExtendedSqlType && dbParam is NpgsqlParameter)
			{
				InitializeParameter(dbParam as NpgsqlParameter, name, sqlType as NpgsqlExtendedSqlType);
			}
			else
			{
				base.InitializeParameter(dbParam, name, sqlType);
			}
		}

        protected virtual void InitializeParameter(NpgsqlParameter dbParam, string name, NpgsqlExtendedSqlType sqlType)
        {
            if (sqlType == null)
            {
                throw new QueryException(String.Format("No type assigned to parameter '{0}'", name));
            }

            dbParam.ParameterName = FormatNameForParameter(name);
            dbParam.DbType = sqlType.DbType;
            dbParam.NpgsqlDbType = sqlType.NpgDbType;

        }
    }
}
