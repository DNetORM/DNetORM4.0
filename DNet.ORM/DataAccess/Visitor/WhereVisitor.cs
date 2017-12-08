
using DNet.Cache;
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

        /// <summary>
        /// 身份标志
        /// </summary>
        public string ParameterID { get; set; }

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

        /// <summary>
        /// 提取字段名for模板
        /// </summary>
        public bool IsGetFieldNameForTemplate { get; set; }

        public string FieldNameForTemplate { get; set; }

        public WhereVisitor(DataBaseType dbType)
        {
            this.parameters = new List<DbParameter>();
            Es = new Dictionary<string, EntityInfo>();
            this.DbType = dbType;
            CallIndex = 0;
            ParameterIndex = 0;
        }

        public WhereVisitor(DataBaseType dbType, int callIndex) : this(dbType)
        {
            CallIndex = callIndex;
        }

        public string Translate<T1>(Expression<Func<T1, bool>> exp) where T1 : class
        {
            ParameterID = typeof(T1).Name;
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
            ParameterID = typeof(T1).Name + "_" + typeof(T2).Name;
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
            ParameterID = typeof(T1).Name + "_" + typeof(T2).Name;
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
                        if (DbType == DataBaseType.SqlServer)
                        {
                            SqlBuilder.Append(" LIKE '%'+");
                            this.Visit(methodExp.Arguments[0]);
                            SqlBuilder.Append("+'%' ");
                        }
                        else
                        {
                            SqlBuilder.Append(" LIKE CONCAT(CONCAT('%',");
                            this.Visit(methodExp.Arguments[0]);
                            SqlBuilder.Append("),'%')");
                        }
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
                    if (DbType == DataBaseType.SqlServer)
                    {
                        SqlBuilder.Append(" CHARINDEX(");
                        this.Visit(methodExp.Arguments[0]);
                        SqlBuilder.Append(",");
                        this.Visit(methodExp.Object);
                        SqlBuilder.Append(")-1 ");
                    }
                    else if (DbType == DataBaseType.MySql)
                    {
                        SqlBuilder.Append(" LOCATE(");
                        this.Visit(methodExp.Arguments[0]);
                        SqlBuilder.Append(",");
                        this.Visit(methodExp.Object);
                        SqlBuilder.Append(")-1 ");
                    }
                    else if (DbType == DataBaseType.Oracle)
                    {
                        SqlBuilder.Append(" INSTR(");
                        this.Visit(methodExp.Object);
                        SqlBuilder.Append(",");
                        this.Visit(methodExp.Arguments[0]);
                        SqlBuilder.Append(")-1 ");
                    }
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
                        if (DbType == DataBaseType.SqlServer)
                        {
                            SqlBuilder.Append(" CAST(");
                            this.Visit(methodExp.Arguments[0]);
                            SqlBuilder.Append(" AS VARCHAR) ");
                        }
                        else if (DbType == DataBaseType.MySql)
                        {
                            SqlBuilder.Append(" CAST(");
                            this.Visit(methodExp.Arguments[0]);
                            SqlBuilder.Append(" AS CHAR) ");
                        }
                        else if (DbType == DataBaseType.Oracle)
                        {
                            SqlBuilder.Append(" TO_CHAR(");
                            this.Visit(methodExp.Arguments[0]);
                            SqlBuilder.Append(") ");
                        }
                        return methodExp;
                    }
                    else if (methodExp.Method.DeclaringType == typeof(System.Object))
                    {
                        if (DbType == DataBaseType.SqlServer)
                        {
                            SqlBuilder.Append(" CAST(");
                            this.Visit(methodExp.Object);
                            SqlBuilder.Append(" AS VARCHAR) ");
                        }
                        else if (DbType == DataBaseType.MySql)
                        {
                            SqlBuilder.Append(" CAST(");
                            this.Visit(methodExp.Object);
                            SqlBuilder.Append(" AS CHAR) ");
                        }
                        else if (DbType == DataBaseType.Oracle)
                        {
                            SqlBuilder.Append(" TO_CHAR(");
                            this.Visit(methodExp.Object);
                            SqlBuilder.Append(") ");
                        }
                        return methodExp;
                    }
                    else if (methodExp.Method.DeclaringType == typeof(Nullable<DateTime>) || methodExp.Method.DeclaringType == typeof(DateTime))
                    {
                        if (methodExp.Arguments[0] is ConstantExpression)
                        {
                            string timeFormat = ((ConstantExpression)methodExp.Arguments[0]).Value as string;
                            if (DbType == DataBaseType.SqlServer)
                            {
                                this.IsGetFieldNameForTemplate = true;
                                this.Visit(methodExp.Object);
                                this.IsGetFieldNameForTemplate = false;
                                //非列就转向了
                                if (string.IsNullOrEmpty(FieldNameForTemplate))
                                {
                                    goto default;
                                }
                                SqlBuilder.AppendFormat("(" + ParseTimeFormat(timeFormat, DataBaseType.SqlServer) + ")", FieldNameForTemplate);
                                FieldNameForTemplate = string.Empty;
                            }
                            else if (DbType == DataBaseType.MySql)
                            {
                                SqlBuilder.Append(" DATE_FORMAT(");
                                this.Visit(methodExp.Object);
                                SqlBuilder.AppendFormat(" ,'{0}') ", ParseTimeFormat(timeFormat, DataBaseType.MySql));
                            }
                            else if (DbType == DataBaseType.Oracle)
                            {
                                SqlBuilder.Append(" TO_CHAR(");
                                this.Visit(methodExp.Object);
                                SqlBuilder.AppendFormat(" ,'{0}') ", ParseTimeFormat(timeFormat, DataBaseType.MySql));
                            }
                            return methodExp;
                        }
                    }
                    goto default;
                case "ToInt32":
                    //Convert.ToInt32()
                    if (methodExp.Method.DeclaringType == typeof(System.Convert))
                    {
                        if (DbType == DataBaseType.SqlServer)
                        {
                            SqlBuilder.Append(" CAST(");
                            this.Visit(methodExp.Arguments[0]);
                            SqlBuilder.Append(" AS INT) ");
                        }
                        else if (DbType == DataBaseType.MySql)
                        {
                            SqlBuilder.Append(" CAST(");
                            this.Visit(methodExp.Arguments[0]);
                            SqlBuilder.Append(" AS INT) ");
                        }
                        else if (DbType == DataBaseType.Oracle)
                        {
                            SqlBuilder.Append(" TO_NUMBER(");
                            this.Visit(methodExp.Arguments[0]);
                            SqlBuilder.Append(") ");
                        }
                        return methodExp;
                    }
                    goto default;
                case "StartsWith":
                    this.Visit(methodExp.Object);
                    if (DbType == DataBaseType.SqlServer)
                    {
                        SqlBuilder.Append(" LIKE ");
                        this.Visit(methodExp.Arguments[0]);
                        SqlBuilder.Append("+'%' ");
                    }
                    else
                    {
                        SqlBuilder.Append(" LIKE CONCAT(");
                        this.Visit(methodExp.Arguments[0]);
                        SqlBuilder.Append(",'%')");
                    }
                    return methodExp;

                case "EndsWith":
                    this.Visit(methodExp.Object);
                    if (DbType == DataBaseType.SqlServer)
                    {
                        SqlBuilder.Append(" LIKE '%'+");
                        this.Visit(methodExp.Arguments[0]);
                        SqlBuilder.Append(" ");
                    }
                    else
                    {
                        SqlBuilder.Append(" LIKE CONCAT('%',");
                        this.Visit(methodExp.Arguments[0]);
                        SqlBuilder.Append(")");
                    }
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
                SqlBuilder.Append(ParameterPrefix+paramName);
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
                    if (!IsGetFieldNameForTemplate)
                    {
                        SqlBuilder.Append(fieldName);
                    }
                    else
                    {
                        FieldNameForTemplate = fieldName;
                    }
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

        /// <summary>
        /// 时间格式转化
        /// </summary>
        /// <param name="clrFormat"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        private string ParseTimeFormat(string clrFormat, DataBaseType dbType)
        {
            if (dbType == DataBaseType.SqlServer)
            {
                clrFormat = clrFormat
                   .Replace("yyyy", "'+CAST(DATEPART(yy,{0}) AS VARCHAR)+'")
                   .Replace("MM", "'+CAST(DATEPART(m,{0}) AS VARCHAR)+'")
                   .Replace("dd", "'+CAST(DATEPART(d,{0}) AS VARCHAR)+'")
                   .Replace("HH", "'+CAST(DATEPART(hh,{0}) AS VARCHAR)+'")
                   .Replace("mm", "'+CAST(DATEPART(mi,{0}) AS VARCHAR)+'")
                   .Replace("ss", "'+CAST(DATEPART(ss,{0}) AS VARCHAR)+'")
                   .TrimStart("'+".ToCharArray())
                   .TrimEnd("+'".ToCharArray());
            }
            else if (dbType == DataBaseType.MySql)
            {
                clrFormat = clrFormat
                    .Replace("yyyy", "%Y")
                    .Replace("MM", "%m")
                    .Replace("dd", "%d")
                    .Replace("HH", "%H")
                    .Replace("mm", "%i")
                    .Replace("ss", "%s");
            }
            else if (dbType == DataBaseType.Oracle)
            {
                clrFormat = clrFormat
                    .Replace("HH", "HH24")
                    .Replace("mm", "mi");
            }
            return clrFormat;
        }
    }
}
