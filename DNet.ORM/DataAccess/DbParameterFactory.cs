using MySql.Data.MySqlClient;
//using MySql.Data.MySqlClient;
//using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNet.DataAccess
{
    public class DbParameterFactory
    {

        /// <summary>
        /// 创建DbParameter
        /// </summary>
        /// <returns></returns>
        public static DbParameter CreateDbParameter(string parameterName, object value, DataBaseType dbType)
        {
            try
            {
                DbParameter dbParameter;
                //return new SqlParameter(parameterName, value);
                if (dbType == DataBaseType.SqlServer)
                {
                    return new SqlParameter("@" + parameterName, value);
                }
                else if (dbType == DataBaseType.MySql)
                {
                    return new MySqlParameter("?" + parameterName, value);
                }
                else
                {
                    return new MySqlParameter(":" + parameterName, value);
                }
            }
            catch
            {
                throw new Exception("DbParameter错误！");
            }
        }

        public static string GetParameterPrefix(DataBaseType dbType)
        {
            if (dbType == DataBaseType.SqlServer)
            {
                return "@";
            }
            else if (dbType == DataBaseType.MySql)
            {
                return "?";
            }
            else
            {
                return ":";
            }
        }
    }
}

