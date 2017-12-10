
using DNet.Cache;
using DNet.DataAccess.Dialect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace DNet.DataAccess
{
    /// <summary>
    /// Lambda转where SQL
    /// Author Jack Liu
    /// </summary>
    public class WhereVisitor : ExpressionVisitor, IVisitor
    {
        private DataBaseType DbType;

        private List<DbParameter> parameters;

        /// <summary>
        /// 条件参数
        /// </summary>
        public List<DbParameter> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        protected Dictionary<string, EntityInfo> Es { get; set; }


        public string ParameterPrefix
        {
            get
            {
                return DbParameterFactory.GetParameterPrefix(DbType);
            }
        }

        public StringBuilder SqlBuilder
        {
            get;
            set;
        }

        public int CallIndex { get; set; }

        public int ParameterIndex { get; set; }

        /// <summary>
        /// 是否预处理二元运算 列和value都在左边的 譬如Compare
        /// </summary>
        public bool IsPreTreatBinary { get; set; }

        public Type MemberType { get; set; }

        public ISqlDialect SqlDialect { get; set; }

        public WhereVisitor(DataBaseType dbType)
        {
            this.parameters = new List<DbParameter>();
            Es = new Dictionary<string, EntityInfo>();
            this.DbType = dbType;
            CallIndex = 0;
            ParameterIndex = 0;
            SqlDialect = SqlDialectFactory.CreateSqlDialect(dbType);
        }

        public WhereVisitor(DataBaseType dbType, int callIndex) : this(dbType)
        {
            CallIndex = callIndex;
        }

        public string Translate(Expression exp)
        {
            int position = SqlBuilder.Length;
            this.Visit(exp);
            string clause = SqlBuilder.ToString().Substring(position);
            SqlBuilder.Remove(position, clause.Length);
            return clause;
        }

        public string Translate<T1>(Expression<Func<T1, bool>> exp) where T1 : class
        {
            Es[typeof(T1).Name] = Caches.EntityInfoCache.Get(typeof(T1));
            this.SqlBuilder = new StringBuilder();
            //m=>true
            if (exp.Body.NodeType == ExpressionType.Constant)
            {
                if (Convert.ToBoolean((exp.Body as ConstantExpression).Value) == true)
                {
                    return " 1=1 ";
                }
            }
            this.Visit(exp.Body);
            return this.SqlBuilder.ToString();
        }

        public string Translate<T1, T2>(Expression<Func<T1, T2, bool>> exp) where T1 : class where T2 : class
        {
            Es[typeof(T1).Name] = Caches.EntityInfoCache.Get(typeof(T1));
            Es[typeof(T2).Name] = Caches.EntityInfoCache.Get(typeof(T2));
            this.SqlBuilder = new StringBuilder();
            if (exp.Body.NodeType == ExpressionType.Constant)
            {
                if (Convert.ToBoolean((exp.Body as ConstantExpression).Value) == true)
                {
                    return " 1=1 ";
                }
            }
            this.Visit(exp.Body);
            return this.SqlBuilder.ToString();
        }

        public string Translate<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> exp) where T1 : class where T2 : class where T3 : class
        {
            Es[typeof(T1).Name] = Caches.EntityInfoCache.Get(typeof(T1));
            Es[typeof(T2).Name] = Caches.EntityInfoCache.Get(typeof(T2));
            Es[typeof(T3).Name] = Caches.EntityInfoCache.Get(typeof(T3));
            this.SqlBuilder = new StringBuilder();
            if (exp.Body.NodeType == ExpressionType.Constant)
            {
                if (Convert.ToBoolean((exp.Body as ConstantExpression).Value) == true)
                {
                    return " 1=1 ";
                }
            }
            this.Visit(exp.Body);
            return this.SqlBuilder.ToString();
        }

        public string Translate<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> exp) where T1 : class where T2 : class where T3 : class where T4 : class
        {
            Es[typeof(T1).Name] = Caches.EntityInfoCache.Get(typeof(T1));
            Es[typeof(T2).Name] = Caches.EntityInfoCache.Get(typeof(T2));
            Es[typeof(T3).Name] = Caches.EntityInfoCache.Get(typeof(T3));
            Es[typeof(T4).Name] = Caches.EntityInfoCache.Get(typeof(T4));
            this.SqlBuilder = new StringBuilder();
            if (exp.Body.NodeType == ExpressionType.Constant)
            {
                if (Convert.ToBoolean((exp.Body as ConstantExpression).Value) == true)
                {
                    return " 1=1 ";
                }
            }
            this.Visit(exp.Body);
            return this.SqlBuilder.ToString();
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodExp)
        {
            switch (methodExp.Method.Name)
            {
                case "Contains":
                    if (methodExp.Method.DeclaringType == typeof(string))
                    {
                        this.Visit(methodExp.Object);
                        SqlBuilder.AppendFormat(SqlDialect.Contains(), Translate(methodExp.Arguments[0]));
                        return methodExp;
                    }
                    else
                    {
                        Expression mex;
                        if (methodExp.Object != null)
                        {
                            this.Visit(methodExp.Arguments[0]);
                            SqlBuilder.Append(" IN (");
                            mex = methodExp.Object;
                        }
                        else
                        {
                            this.Visit(methodExp.Arguments[1]);
                            SqlBuilder.Append(" IN (");
                            mex = methodExp.Arguments[0];
                        }
                        IEnumerable result = Expression.Lambda(mex).Compile().DynamicInvoke() as IEnumerable;
                        int length = 0;
                        foreach (var v in result)
                        {
                            length++;
                            this.Visit(Expression.Constant(v));
                            SqlBuilder.Append(",");
                        }
                        if (length == 0)
                        {
                            SqlBuilder.Append("NULL,");
                        }
                        SqlBuilder.Remove(SqlBuilder.Length - 1, 1);
                        SqlBuilder.Append(")");
                        return methodExp;
                    }
                case "ToUpper":
                    SqlBuilder.Append(" UPPER(");
                    this.Visit(methodExp.Object);
                    SqlBuilder.Append(")");
                    return methodExp;
                case "ToLower":
                    SqlBuilder.Append(" LOWER(");
                    this.Visit(methodExp.Object);
                    SqlBuilder.Append(")");
                    return methodExp;
                case "CompareTo":
                    IsPreTreatBinary = true;
                    this.Visit(methodExp.Object);
                    SqlBuilder.Append("{0}");
                    this.Visit(methodExp.Arguments[0]);
                    return methodExp;
                case "Equals":
                    this.Visit(methodExp.Object);
                    SqlBuilder.Append("=");
                    this.Visit(methodExp.Arguments[0]);
                    return methodExp;
                case "IndexOf":
                    SqlBuilder.AppendFormat(SqlDialect.IndexOf(), Translate(methodExp.Arguments[0]), Translate(methodExp.Object));
                    return methodExp;
                case "IsNullOrEmpty":
                    string template = " ({0} IS NULL OR {0}='') ";
                    string[] sections = template.Split(new string[] { "{0}" }, StringSplitOptions.None);
                    for (int i = 0; i < sections.Length - 1; i++)
                    {
                        if (i == 0)
                        {
                            SqlBuilder.Append(sections[i]);
                        }
                        this.Visit(methodExp.Arguments[0]);
                        SqlBuilder.Append(sections[i + 1]);
                    }
                    return methodExp;
                case "ToString":
                    //Convert.ToString()
                    if (methodExp.Method.DeclaringType == typeof(System.Convert))
                    {
                        SqlBuilder.AppendFormat(SqlDialect.ToChar(), Translate(methodExp.Arguments[0]));
                        return methodExp;
                    }
                    else if (methodExp.Method.DeclaringType == typeof(System.Object))
                    {
                        SqlBuilder.AppendFormat(SqlDialect.ToChar(), Translate(methodExp.Object));
                        return methodExp;
                    }
                    else if (methodExp.Method.DeclaringType == typeof(Nullable<DateTime>) || methodExp.Method.DeclaringType == typeof(DateTime))
                    {
                        if (methodExp.Arguments[0] is ConstantExpression)
                        {
                            string timeFormat = ((ConstantExpression)methodExp.Arguments[0]).Value as string;
                            if (DbType == DataBaseType.SqlServer)
                            {
                                string clause = Translate(methodExp.Object);
                                //非列就转向了
                                if (string.IsNullOrEmpty(clause))
                                {
                                    goto default;
                                }
                                SqlBuilder.AppendFormat("(" + SqlDialect.ParseTimeFormat(timeFormat) + ")", clause);
                            }
                            else
                            {
                                SqlBuilder.AppendFormat(SqlDialect.DateTimeToChar(), Translate(methodExp.Object), SqlDialect.ParseTimeFormat(timeFormat));
                            }
                            return methodExp;
                        }
                    }
                    goto default;
                case "ToInt32":
                    //Convert.ToInt32()
                    if (methodExp.Method.DeclaringType == typeof(System.Convert))
                    {
                        SqlBuilder.AppendFormat(SqlDialect.ToNumber(), Translate(methodExp.Arguments[0]));
                        return methodExp;
                    }
                    goto default;
                case "ToDateTime":
                    if (methodExp.Method.DeclaringType == typeof(System.Convert))
                    {
                        MemberTypeVisitor mtVisitor = new MemberTypeVisitor();
                        MemberType = mtVisitor.Translate(methodExp.Arguments[0]);
                        if (MemberType == typeof(Nullable<DateTime>) || MemberType == typeof(DateTime))
                        {
                            this.Visit(methodExp.Arguments[0]);
                            return methodExp;
                        }
                        else if (MemberType == typeof(string))
                        {
                            SqlBuilder.AppendFormat(SqlDialect.ToDateTime(), Translate(methodExp.Arguments[0]));
                            return methodExp;
                        }
                    }
                    goto default;
                case "StartsWith":
                    this.Visit(methodExp.Object);
                    SqlBuilder.AppendFormat(SqlDialect.StartsWith(), Translate(methodExp.Arguments[0]));
                    return methodExp;
                case "EndsWith":
                    this.Visit(methodExp.Object);
                    SqlBuilder.AppendFormat(SqlDialect.EndsWith(), Translate(methodExp.Arguments[0]));
                    return methodExp;
                case "TrimStart":
                    SqlBuilder.Append(" LTRIM(");
                    this.Visit(methodExp.Object);
                    SqlBuilder.Append(")");
                    return methodExp;
                case "TrimEnd":
                    SqlBuilder.Append(" RTRIM(");
                    this.Visit(methodExp.Object);
                    SqlBuilder.Append(")");
                    return methodExp;
                case "Trim":
                    SqlBuilder.Append(" RTRIM(LTRIM(");
                    this.Visit(methodExp.Object);
                    SqlBuilder.Append("))");
                    return methodExp;
                default:
                    try
                    {
                        object result = Expression.Lambda(methodExp).Compile().DynamicInvoke();
                        var r = Expression.Constant(result, methodExp.Method.ReturnType);
                        this.Visit(r);
                        return r;
                    }
                    catch
                    {
                        if (methodExp.Object != null)
                        {
                            Expression obj = this.Visit(methodExp.Object);
                            return obj;
                        }
                        else
                        {
                            Expression obj = this.Visit(methodExp.Arguments[0]);
                            return obj;
                        }
                    }
                    throw new NotSupportedException(string.Format("lambda表达式不支持使用此'{0}'方法，请自行扩展", methodExp.Method.Name));

            }
        }

        protected override Expression VisitUnary(UnaryExpression unaryExp)
        {
            try
            {
                if (unaryExp.NodeType == ExpressionType.Convert)
                {
                    this.Visit(unaryExp.Operand);
                    return unaryExp;
                }
                else if (unaryExp.NodeType == ExpressionType.Not)
                {
                    SqlBuilder.Append(" NOT ");
                    this.Visit(unaryExp.Operand);
                    return unaryExp;
                }
                else
                {
                    object result = Expression.Lambda(unaryExp).Compile().DynamicInvoke();
                    var r = Expression.Constant(result, result.GetType());
                    this.Visit(r);
                    return r;
                }
            }
            catch
            {
                throw new NotSupportedException(string.Format("一元运算符 '{0}’不支持", unaryExp.NodeType));
            }
        }

        protected override Expression VisitBinary(BinaryExpression binaryExp)
        {
            SqlBuilder.Append("(");
            switch (binaryExp.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    this.Visit(binaryExp.Left);
                    SqlBuilder.Append(" AND ");
                    this.Visit(binaryExp.Right);
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    this.Visit(binaryExp.Left);
                    SqlBuilder.Append(" OR ");
                    this.Visit(binaryExp.Right);
                    break;
                case ExpressionType.Equal:
                    this.Visit(binaryExp.Left);
                    if (IsNullValue(binaryExp.Right))
                    {
                        SqlBuilder.Append(" IS ");
                    }
                    else
                    {
                        SqlBuilder.Append(" = ");
                    }
                    this.Visit(binaryExp.Right);
                    break;
                case ExpressionType.NotEqual:
                    this.Visit(binaryExp.Left);
                    if (IsNullValue(binaryExp.Right))
                    {
                        SqlBuilder.Append(" IS NOT ");
                    }
                    else
                    {
                        SqlBuilder.Append(" <> ");
                    }
                    this.Visit(binaryExp.Right);
                    break;
                case ExpressionType.LessThan:
                    this.Visit(binaryExp.Left);
                    if (IsPreTreatBinary)
                    {
                        SqlBuilder.Replace("{0}", "<");
                        IsPreTreatBinary = false;
                    }
                    else
                    {
                        SqlBuilder.Append(" < ");
                        this.Visit(binaryExp.Right);
                    }
                    break;
                case ExpressionType.LessThanOrEqual:
                    this.Visit(binaryExp.Left);
                    if (IsPreTreatBinary)
                    {
                        SqlBuilder.Replace("{0}", "<=");
                        IsPreTreatBinary = false;
                    }
                    else
                    {
                        SqlBuilder.Append(" <= ");
                        this.Visit(binaryExp.Right);
                    }
                    break;
                case ExpressionType.GreaterThan:
                    this.Visit(binaryExp.Left);
                    if (IsPreTreatBinary)
                    {
                        SqlBuilder.Replace("{0}", ">");
                        IsPreTreatBinary = false;
                    }
                    else
                    {
                        SqlBuilder.Append(" > ");
                        this.Visit(binaryExp.Right);
                    }
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    this.Visit(binaryExp.Left);
                    if (IsPreTreatBinary)
                    {
                        SqlBuilder.Replace("{0}", ">=");
                        IsPreTreatBinary = false;
                    }
                    else
                    {
                        SqlBuilder.Append(" >= ");
                        this.Visit(binaryExp.Right);
                    }
                    break;
                case ExpressionType.Add:
                    if (binaryExp.Type == typeof(string) && DbType != DataBaseType.SqlServer)
                    {
                        SqlBuilder.Append("CONCAT(");
                        this.Visit(binaryExp.Left);
                        SqlBuilder.Append(",");
                        this.Visit(binaryExp.Right);
                        SqlBuilder.Append(")");
                    }
                    else
                    {
                        this.Visit(binaryExp.Left);
                        SqlBuilder.Append(" + ");
                        this.Visit(binaryExp.Right);
                    }
                    break;
                case ExpressionType.Subtract:
                    this.Visit(binaryExp.Left);
                    SqlBuilder.Append(" - ");
                    this.Visit(binaryExp.Right);
                    break;
                case ExpressionType.Multiply:
                    this.Visit(binaryExp.Left);
                    SqlBuilder.Append(" * ");
                    this.Visit(binaryExp.Right);
                    break;
                case ExpressionType.Divide:
                    this.Visit(binaryExp.Left);
                    SqlBuilder.Append(" / ");
                    this.Visit(binaryExp.Right);
                    break;
                default:
                    throw new NotSupportedException(string.Format("二元运算符 '{0}'不支持", binaryExp.NodeType));

            }
            SqlBuilder.Append(")");
            return binaryExp;
        }

        protected bool IsNullValue(Expression exp)
        {
            try
            {
                if (exp.NodeType == ExpressionType.MemberAccess)
                {
                    var tName = ((MemberExpression)exp).Expression.Type.Name;
                    if (Es.ContainsKey(tName))
                    {
                        return false;
                    }
                }
                object result = Expression.Lambda(exp).Compile().DynamicInvoke();
                return result == null;
            }
            catch
            {
                return false;
            }
        }

        protected override Expression VisitConstant(ConstantExpression constantExp)
        {
            if (constantExp.Value == null)
            {
                SqlBuilder.Append("NULL");
            }
            else
            {
                string paramName = string.Format("{0}_{1}", CallIndex, ParameterIndex++);
                this.parameters.Add(GetDbParameter(paramName, constantExp.Value));
                SqlBuilder.Append(ParameterPrefix + paramName);
            }
            return constantExp;
        }

        protected override Expression VisitMember(MemberExpression memberExp)
        {
            if (memberExp.Expression != null && memberExp.Expression.NodeType == ExpressionType.Parameter)
            {
                var typeName = memberExp.Expression.Type.Name;
                string fieldName = this.Es[typeName].TableName + "." + GetFieldName(typeName, memberExp.Member.Name);
                if (!string.IsNullOrEmpty(fieldName))
                {
                    SqlBuilder.Append(fieldName);
                    MemberType = memberExp.Type;
                }
                return memberExp;
            }
            else if (memberExp.NodeType == ExpressionType.MemberAccess)
            {
                object result = Expression.Lambda(memberExp).Compile().DynamicInvoke();
                if (result != null)
                {
                    var r = Expression.Constant(result, result.GetType());
                    this.Visit(r);
                    return r;
                }
                else
                {
                    var r = Expression.Constant(result);
                    this.Visit(r);
                    return r;
                }
            }
            throw new NotSupportedException(string.Format("成员 '{0}’不支持", memberExp.Member.Name));
        }

        public string GetFieldName(string typeName, string memberName)
        {
            if (this.Es[typeName].Columns.Keys.Contains(memberName))
            {
                return this.Es[typeName].Columns[memberName];
            }
            else
            {
                return memberName;
            }
        }

        public DbParameter GetDbParameter(string parameterName, object value)
        {
            return DbParameterFactory.CreateDbParameter(parameterName, value, DbType);
        }
    }
}
