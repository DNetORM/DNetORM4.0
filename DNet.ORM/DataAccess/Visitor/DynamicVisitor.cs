using DNet.Cache;
using System;
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
    /// Lambda转动态对象
    /// Author Jack Liu
    /// </summary>
    public class DynamicVisitor : ExpressionVisitor, IVisitor
    {
        protected Dictionary<string, EntityInfo> Es { get; set; }

        /// <summary>
        /// 动态属性
        /// </summary>
        public List<DynamicMember> DynamicMembers { get; set; }

        public DynamicVisitor()
        {
            Es = new Dictionary<string, EntityInfo>();
            DynamicMembers = new List<DynamicMember>();
        }

        public void Translate<T1,TResult>(Expression<Func<T1, TResult>> exp)
        {
            Es[typeof(T1).Name] = Caches.EntityInfoCache.Get(typeof(T1));
            this.DynamicMembers.Clear();
            if (exp != null)
            {
                this.Visit(exp.Body);
            }
        }

        public void Translate<T1,T2, TResult>(Expression<Func<T1,T2, TResult>> exp)
        {
            Es[typeof(T1).Name] = Caches.EntityInfoCache.Get(typeof(T1));
            Es[typeof(T2).Name] = Caches.EntityInfoCache.Get(typeof(T2));
            this.DynamicMembers.Clear();
            if (exp != null)
            {
                this.Visit(exp.Body);
            }
        }

        public void Translate<T1, T2,T3, TResult>(Expression<Func<T1, T2,T3, TResult>> exp)
        {
            Es[typeof(T1).Name] = Caches.EntityInfoCache.Get(typeof(T1));
            Es[typeof(T2).Name] = Caches.EntityInfoCache.Get(typeof(T2));
            Es[typeof(T3).Name] = Caches.EntityInfoCache.Get(typeof(T3));
            this.DynamicMembers.Clear();
            if (exp != null)
            {
                this.Visit(exp.Body);
            }
        }

        public void Translate<T1, T2, T3,T4, TResult>(Expression<Func<T1, T2, T3,T4, TResult>> exp)
        {
            Es[typeof(T1).Name] = Caches.EntityInfoCache.Get(typeof(T1));
            Es[typeof(T2).Name] = Caches.EntityInfoCache.Get(typeof(T2));
            Es[typeof(T3).Name] = Caches.EntityInfoCache.Get(typeof(T3));
            Es[typeof(T4).Name] = Caches.EntityInfoCache.Get(typeof(T3));
            this.DynamicMembers.Clear();
            if (exp != null)
            {
                this.Visit(exp.Body);
            }
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            string typeName = node.Type.Name;
            DynamicMembers.Add(new DynamicMember { Key = "*", Field = Es[typeName].TableName + ".*" });
            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                if (DynamicMembers.Count(m => m.Key == node.Members[i].Name) == 0)
                {
                    DynamicMembers.Add(new DynamicMember { Key = node.Members[i].Name });
                }
                this.Visit(node.Arguments[i]);

            }
            return node;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            if (DynamicMembers.Count(m => m.Key == assignment.Member.Name) == 0)
            {
                DynamicMembers.Add(new DynamicMember { Key = assignment.Member.Name });
            }
            Expression e = this.Visit(assignment.Expression);
            return assignment;
        }

        protected override Expression VisitMember(MemberExpression memberExp)
        {
            if (memberExp.Expression != null && memberExp.Expression.NodeType == ExpressionType.Parameter)
            {
                var typeName = memberExp.Expression.Type.Name;
                string fieldName = Es[typeName].TableName + "." + GetFieldName(typeName,memberExp.Member.Name);
                if (DynamicMembers.Count > 0)
                {
                    DynamicMembers.Last().Field = fieldName;
                }
                else
                {
                    DynamicMembers.Add(new DynamicMember { Key = memberExp.Member.Name, Field = fieldName });
                }
                return memberExp;
            }
            else if (memberExp.NodeType == ExpressionType.MemberAccess)
            {
                object result = Expression.Lambda(memberExp).Compile().DynamicInvoke();
                var r = Expression.Constant(result, result.GetType());
                this.Visit(r);
                return r;
            }
            throw new NotSupportedException(string.Format("成员 '{0}’不支持", memberExp.Member.Name));
        }


        protected override Expression VisitMethodCall(MethodCallExpression methodExp)
        {
            if (methodExp.Object != null && methodExp.Object.NodeType == ExpressionType.MemberAccess)
            {
                Expression obj = this.Visit(methodExp.Object);
                return obj;
            }
            else if (methodExp.Arguments.Count > 0)
            {
                for (int i = 0; i < methodExp.Arguments.Count; i++)
                {
                    Expression obj = this.Visit(methodExp.Arguments[i]);
                    return obj;
                }
            }
            throw new NotSupportedException(string.Format("lambda表达式不支持使用此'{0}'方法", methodExp.Method.Name));
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
            this.Visit(binaryExp.Left);
            this.Visit(binaryExp.Right);
            return binaryExp;
        }

        public string ParameterPrefix
        {
            get { throw new NotImplementedException(); }
        }

        public DbParameter GetDbParameter(string parameterName, object value)
        {
            throw new NotImplementedException();
        }

        public string GetFieldName(string typeName,string memberName)
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
    }
}
