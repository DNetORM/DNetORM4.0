using DNet.Cache;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;


namespace DNet.DataAccess
{
    /// <summary>
    /// 连接查询辅助类
    /// Author Jack Liu
    /// </summary>
    public class JoinQuery
    {

        private int callIndex = 0;
        /// <summary>
        /// SQL
        /// </summary>
        protected StringBuilder SqlBuilder { get; set; }

        /// <summary>
        /// 连接条件
        /// </summary>
        protected List<JoinRelation> JoinRelations { get; set; }

        /// <summary>
        /// where条件
        /// 最后拼接SQL的时候要Trim掉'AND'
        /// </summary>
        protected StringBuilder WhereClause { get; set; }

        /// <summary>
        /// 条件参数
        /// </summary>
        protected List<DbParameter> Parameters { get; set; }

        /// <summary>
        /// 排序
        /// 最后拼接SQL的时候要Trim掉','
        /// </summary>
        protected StringBuilder OrderBy { get; set; }

        /// <summary>
        /// 分组
        /// 最后拼接SQL的时候要Trim掉','
        /// </summary>
        protected StringBuilder GroupByFields { get; set; }

        /// <summary>
        /// select筛选
        /// </summary>
        protected List<TableSelect> TableSelects { get; set; }

        protected DNetContext DbContext { get; set; }

        public JoinQuery()
        {
            SqlBuilder = new StringBuilder();
            JoinRelations = new List<JoinRelation>();
            TableSelects = new List<TableSelect>();
            WhereClause = new StringBuilder();
            Parameters = new List<DbParameter>();
            OrderBy = new StringBuilder();
            GroupByFields = new StringBuilder();
        }

        private void Clear()
        {
            SqlBuilder.Clear();
            JoinRelations.Clear();
            TableSelects.Clear();
            WhereClause.Clear();
            Parameters.Clear();
            OrderBy.Clear();
            GroupByFields.Clear();
        }

        public JoinQuery(DNetContext db) : this()
        {
            DbContext = db;
        }

        /// <summary>
        /// 左外连接 参数决定顺序
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="on"></param>
        /// <returns></returns>
        public JoinQuery LeftJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> on) where TLeft : class, new() where TRight : class, new()
        {
            JoinRelation link = new JoinRelation { JoinType = JoinType.Outer };
            WhereVisitor visitor = new WhereVisitor(DbContext.DataBase.DBType, callIndex++);
            link.LeftTable = Caches.EntityInfoCache.Get(typeof(TLeft)).TableName;
            link.RightTable = Caches.EntityInfoCache.Get(typeof(TRight)).TableName;
            link.OnSql = visitor.Translate<TLeft, TRight>(on);
            Parameters.AddRange(visitor.Parameters);
            JoinRelations.Add(link);
            return this;
        }

        /// <summary>
        /// 内连接 参数决定顺序
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="on"></param>
        /// <returns></returns>
        public JoinQuery InnerJoin<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> on) where TLeft : class, new() where TRight : class, new()
        {
            JoinRelation link = new JoinRelation { JoinType = JoinType.Inner };
            WhereVisitor visitor = new WhereVisitor(DbContext.DataBase.DBType, callIndex++);
            link.LeftTable = Caches.EntityInfoCache.Get(typeof(TLeft)).TableName;
            link.RightTable = Caches.EntityInfoCache.Get(typeof(TRight)).TableName;
            link.OnSql = visitor.Translate<TLeft, TRight>(on);
            Parameters.AddRange(visitor.Parameters);
            JoinRelations.Add(link);
            return this;
        }

        /// <summary>
        /// 添加where查询条件
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public JoinQuery Where<TEntity>(Expression<Func<TEntity, bool>> where) where TEntity : class, new()
        {
            WhereVisitor visitor = new WhereVisitor(DbContext.DataBase.DBType, callIndex++);
            visitor.Translate<TEntity>(where);
            WhereClause.Append(visitor.SqlBuilder.ToString() + " AND ");
            Parameters.AddRange(visitor.Parameters);
            return this;
        }

        public JoinQuery Where<T1, T2>(Expression<Func<T1, T2, bool>> where) where T1 : class, new() where T2 : class, new()
        {
            WhereVisitor visitor = new WhereVisitor(DbContext.DataBase.DBType, callIndex++);
            visitor.Translate<T1, T2>(where);
            WhereClause.Append(visitor.SqlBuilder.ToString() + " AND ");
            Parameters.AddRange(visitor.Parameters);
            return this;
        }

        public JoinQuery Where<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> where) where T1 : class, new() where T2 : class, new() where T3 : class, new()
        {
            WhereVisitor visitor = new WhereVisitor(DbContext.DataBase.DBType, callIndex++);
            visitor.Translate<T1, T2, T3>(where);
            WhereClause.Append(visitor.SqlBuilder.ToString() + " AND ");
            Parameters.AddRange(visitor.Parameters);
            return this;
        }

        /// <summary>
        /// 添加升序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public JoinQuery OrderByAsc<TEntity>(Expression<Func<TEntity, dynamic>> orderBy)
        {
            DynamicVisitor visitor = new DynamicVisitor();
            visitor.Translate<TEntity, dynamic>(orderBy);
            foreach (DynamicMember c in visitor.DynamicMembers)
            {
                OrderBy.Append(c.Field + " ASC,");
            }
            return this;
        }

        /// <summary>
        /// 添加降序
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public JoinQuery OrderByDesc<TEntity>(Expression<Func<TEntity, dynamic>> orderBy)
        {
            DynamicVisitor visitor = new DynamicVisitor();
            visitor.Translate<TEntity, dynamic>(orderBy);
            foreach (DynamicMember c in visitor.DynamicMembers)
            {
                OrderBy.Append(c.Field + " DESC,");
            }
            return this;
        }

        /// <summary>
        /// 添加查询
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="select"></param>
        /// <returns></returns>
        public JoinQuery Fields<TEntity>(Expression<Func<TEntity, dynamic>> select = null)
        {
            EntityInfo entityInfo = Caches.EntityInfoCache.Get(typeof(TEntity));
            TableSelect tableSelect = new TableSelect { Table = entityInfo.TableName };
            if (select == null)
            {
                tableSelect.Fields.Add(new DynamicMember { Field = entityInfo.TableName + ".*" });
            }
            else
            {
                DynamicVisitor visitor = new DynamicVisitor();
                visitor.Translate<TEntity, dynamic>(select);
                tableSelect.Fields.AddRange(visitor.DynamicMembers.ToArray());
            }
            TableSelects.Add(tableSelect);
            return this;
        }

        public JoinQuery Fields<T1, T2>(Expression<Func<T1, T2, dynamic>> select = null)
        {
            EntityInfo e1 = Caches.EntityInfoCache.Get(typeof(T1));
            EntityInfo e2 = Caches.EntityInfoCache.Get(typeof(T2));
            TableSelect tableSelect = new TableSelect { Table = e1.TableName };
            if (select == null)
            {
                tableSelect.Fields.Add(new DynamicMember { Field = e1.TableName + ".*," + e2.TableName + ".*" });
            }
            else
            {
                DynamicVisitor visitor = new DynamicVisitor();
                visitor.Translate<T1, T2, dynamic>(select);
                tableSelect.Fields.AddRange(visitor.DynamicMembers.ToArray());
            }
            TableSelects.Add(tableSelect);
            return this;
        }

        public JoinQuery Fields<T1, T2, T3>(Expression<Func<T1, T2, T3, dynamic>> select = null)
        {
            EntityInfo e1 = Caches.EntityInfoCache.Get(typeof(T1));
            EntityInfo e2 = Caches.EntityInfoCache.Get(typeof(T2));
            EntityInfo e3 = Caches.EntityInfoCache.Get(typeof(T3));
            TableSelect tableSelect = new TableSelect { Table = e1.TableName };
            if (select == null)
            {
                tableSelect.Fields.Add(new DynamicMember { Field = e1.TableName + ".*," + e2.TableName + ".*," + e3.TableName + ".*" });
            }
            else
            {
                DynamicVisitor visitor = new DynamicVisitor();
                visitor.Translate<T1, T2, T3, dynamic>(select);
                tableSelect.Fields.AddRange(visitor.DynamicMembers.ToArray());
            }
            TableSelects.Add(tableSelect);
            return this;
        }

        public JoinQuery GroupBy<TEntity>(Expression<Func<TEntity, dynamic>> select = null)
        {
            DynamicVisitor visitor = new DynamicVisitor();
            visitor.Translate<TEntity, dynamic>(select);
            foreach (DynamicMember c in visitor.DynamicMembers)
            {
                GroupByFields.Append(c.Field + ",");
            }
            return this;
        }

        public JoinQuery GroupBy<T1, T2>(Expression<Func<T1, T2, dynamic>> select = null)
        {
            DynamicVisitor visitor = new DynamicVisitor();
            visitor.Translate<T1, T2, dynamic>(select);
            foreach (DynamicMember c in visitor.DynamicMembers)
            {
                GroupByFields.Append(c.Field + ",");
            }
            return this;
        }

        public JoinQuery GroupBy<T1, T2, T3>(Expression<Func<T1, T2, T3, dynamic>> select = null)
        {
            DynamicVisitor visitor = new DynamicVisitor();
            visitor.Translate<T1, T2, T3, dynamic>(select);
            foreach (DynamicMember c in visitor.DynamicMembers)
            {
                GroupByFields.Append(c.Field + ",");
            }
            return this;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        public List<TModel> GetList<TModel>() 
        {
            List<string> tables = new List<string>();
            SqlBuilder.Append(" SELECT ");
            if (TableSelects.Count == 0)
            {
                this.GetList<TModel>();
            }
            foreach (TableSelect s in TableSelects)
            {
                SqlBuilder.Append(s.GetSelect());
            }
            SqlBuilder.Remove(SqlBuilder.Length - 1, 1);//去掉','
            SqlBuilder.Append(" FROM ");
            SqlBuilder.Append(JoinRelations[0].LeftTable);
            foreach (JoinRelation j in JoinRelations)
            {
                if (j.JoinType == JoinType.Outer)
                {
                    SqlBuilder.Append(" LEFT OUTER JOIN ");
                }
                else
                {
                    SqlBuilder.Append(" INNER JOIN ");
                }
                if (tables.Count(m => m == j.RightTable) == 0)
                {
                    SqlBuilder.Append(j.RightTable);
                    tables.Add(j.RightTable);
                }
                else
                {
                    SqlBuilder.Append(j.LeftTable);
                    tables.Add(j.LeftTable);
                }
                SqlBuilder.Append(" ON ");
                SqlBuilder.Append(j.OnSql.TrimEnd("AND".ToCharArray()));
            }
            if (WhereClause.Length > 0)
            {
                SqlBuilder.Append(" WHERE ");
                SqlBuilder.Append(WhereClause.ToString());
                SqlBuilder.Remove(SqlBuilder.Length - 4, 4);//去掉'AND '
            }
            if (GroupByFields.Length > 0)
            {
                SqlBuilder.Append(" GROUP BY ");
                SqlBuilder.Append(GroupByFields.ToString());
                SqlBuilder.Remove(SqlBuilder.Length - 1, 1);//去掉','
            }
            if (OrderBy.Length > 0)
            {
                SqlBuilder.Append(" ORDER BY ");
                SqlBuilder.Append(OrderBy.ToString());
                SqlBuilder.Remove(SqlBuilder.Length - 1, 1);//去掉','
            }
            //开始组装sql
            return DbContext.GetList<TModel>(SqlBuilder.ToString(), Parameters.ToArray());

        }

        /// <summary>
        /// 返回数量
        /// </summary>
        /// <returns></returns>
        public int GetCount()
        {
            List<string> tables = new List<string>();
            SqlBuilder.Append(" SELECT COUNT(1) AS CT FROM ");
            SqlBuilder.Append(JoinRelations[0].LeftTable);
            foreach (JoinRelation j in JoinRelations)
            {
                if (j.JoinType == JoinType.Outer)
                {
                    SqlBuilder.Append(" LEFT OUTER JOIN ");
                }
                else
                {
                    SqlBuilder.Append(" INNER JOIN ");
                }
                if (tables.Count(m => m == j.RightTable) == 0)
                {
                    SqlBuilder.Append(j.RightTable);
                    tables.Add(j.RightTable);
                }
                else
                {
                    SqlBuilder.Append(j.LeftTable);
                    tables.Add(j.LeftTable);
                }
                SqlBuilder.Append(" ON ");
                SqlBuilder.Append(j.OnSql.TrimEnd("AND".ToCharArray()));
            }
            if (WhereClause.Length > 0)
            {
                SqlBuilder.Append(" WHERE ");
                SqlBuilder.Append(WhereClause.ToString());
                SqlBuilder.Remove(SqlBuilder.Length - 4, 4);//去掉'AND '
            }
            if (GroupByFields.Length > 0)
            {
                SqlBuilder.Append(" GROUP BY ");
                SqlBuilder.Append(GroupByFields.ToString());
                SqlBuilder.Remove(SqlBuilder.Length - 1, 1);//去掉','
            }
            if (OrderBy.Length > 0)
            {
                SqlBuilder.Append(" ORDER BY ");
                SqlBuilder.Append(OrderBy.ToString());
                SqlBuilder.Remove(SqlBuilder.Length - 1, 1);//去掉','
            }
            //开始组装sql
            return DbContext.GetSingle<int>(SqlBuilder.ToString(), Parameters.ToArray());
        }

        /// <summary>
        /// 多表连查分页
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="page"></param>
        /// <returns></returns>
        public PageDataSource<TModel> GetPage<TModel>(PageFilter page) 
        {
            List<string> tables = new List<string>();
            SqlBuilder.Append(" SELECT ");
            if (TableSelects.Count == 0)
            {
                this.GetList<TModel>();
            }
            foreach (TableSelect s in TableSelects)
            {
                SqlBuilder.Append(s.GetSelect());
            }
            SqlBuilder.Remove(SqlBuilder.Length - 1, 1);//去掉','
            SqlBuilder.Append(" FROM ");
            SqlBuilder.Append(JoinRelations[0].LeftTable);
            foreach (JoinRelation j in JoinRelations)
            {
                if (j.JoinType == JoinType.Outer)
                {
                    SqlBuilder.Append(" LEFT OUTER JOIN ");
                }
                else
                {
                    SqlBuilder.Append(" INNER JOIN ");
                }
                if (tables.Count(m => m == j.RightTable) == 0)
                {
                    SqlBuilder.Append(j.RightTable);
                    tables.Add(j.RightTable);
                }
                else
                {
                    SqlBuilder.Append(j.LeftTable);
                    tables.Add(j.LeftTable);
                }
                SqlBuilder.Append(" ON ");
                SqlBuilder.Append(j.OnSql.TrimEnd("AND".ToCharArray()));
            }
            if (WhereClause.Length > 0)
            {
                SqlBuilder.Append(" WHERE ");
                SqlBuilder.Append(WhereClause.ToString());
                SqlBuilder.Remove(SqlBuilder.Length - 3, 3);//去掉'AND'
            }
            if (GroupByFields.Length > 0)
            {
                SqlBuilder.Append(" GROUP BY ");
                SqlBuilder.Append(GroupByFields.ToString());
                SqlBuilder.Remove(SqlBuilder.Length - 1, 1);//去掉','
            }
            if (OrderBy.Length > 0)
            {
                SqlBuilder.Append(" ORDER BY ");
                SqlBuilder.Append(OrderBy.ToString());
                SqlBuilder.Remove(SqlBuilder.Length - 1, 1);//去掉','
            }
            //开始组装sql
            return DbContext.GetPage<TModel>(SqlBuilder.ToString(), page, Parameters.ToArray());
        }
    }
}
