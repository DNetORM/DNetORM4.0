using DNet.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNet.DataAccess
{
    public class SqlBuilder
    {
        /// <summary>
        /// 获取查询字段
        /// Author Jack Liu
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetSelectAllFields<T>()
        {
            string selectFields = Caches.EntityInfoCache.Get(typeof(T)).SelectFields;
            string tableName = Caches.EntityInfoCache.Get(typeof(T)).TableName;
            selectFields = tableName + "." + selectFields.Replace(",", "," + tableName + ".");
            return selectFields;
        }

        /// <summary>
        /// 获取查询字段带别名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableAlias"></param>
        /// <returns></returns>
        public static string GetSelectAllFields<T>(string tableAlias)
        {
            string selectFields = Caches.EntityInfoCache.Get(typeof(T)).SelectFields;
            selectFields = tableAlias + "." + selectFields.Replace(",", "," + tableAlias + ".");
            return selectFields;
        }

        /// <summary>
        /// 获取in的sql
        /// </summary>
        /// <param name="tableNameOrAlias"></param>
        /// <param name="columnName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static SqlConstructor GetInSql(string tableNameOrAlias, string columnName, IEnumerable<int> values,DataBaseType dbType)
        {
            SqlConstructor result = new SqlConstructor();
            if (values != null && values.Any())
            {
                result.Sql.AppendFormat(" {0}.{1} IN (", tableNameOrAlias, columnName);
                int i = 100;
                foreach (var v in values)
                {
                    i++;
                    result.Sql.AppendFormat("@{0}{1},", columnName, i);
                    result.Parameters.Add(DbParameterFactory.CreateDbParameter(columnName + i.ToString(), v));
                }
                result.Sql.Remove(result.Sql.Length - 1, 1);
                result.Sql.Append(") ");
                result.HasSql = true;
            }
            return result;
        }

        /// <summary>
        /// 获取in的sql
        /// </summary>
        /// <param name="tableNameOrAlias"></param>
        /// <param name="columnName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static SqlConstructor GetInSql(string tableNameOrAlias, string columnName, IEnumerable<string> values)
        {
            SqlConstructor result = new SqlConstructor();
            if (values != null && values.Any())
            {
                result.Sql.AppendFormat(" {0}.{1} IN (", tableNameOrAlias, columnName);
                int i = 100;
                foreach (var v in values)
                {
                    i++;
                    result.Sql.AppendFormat("@{0}{1},", columnName, i);
                    result.Parameters.Add(DbParameterFactory.CreateDbParameter(columnName + i.ToString(), v));
                }
                result.Sql.Remove(result.Sql.Length - 1, 1);
                result.Sql.Append(") ");
                result.HasSql = true;
            }
            return result;
        }

    }
}
