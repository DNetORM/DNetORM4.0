using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNet.DataAccess.Dialect
{
    public class SqlDialectFactory
    {
        public static ISqlDialect CreateSqlDialectr(DataBaseType dbType)
        {
            try
            {
              
                if (dbType == DataBaseType.SqlServer)
                {
                    return new SqlServerDialect();
                }
                else if (dbType == DataBaseType.MySql)
                {
                    return new MySqlDialect();
                }
                else
                {
                    return new OracleDialect();
                }
            }
            catch
            {
                throw new Exception("SqlDialect错误！");
            }
        }
    }
}
