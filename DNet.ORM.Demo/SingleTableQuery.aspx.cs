using DNet.DataAccess;
using DNet.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DNet.ORM.Demo
{


    /// <summary>
    /// 单表查询
    /// </summary>
    public partial class SingleTableQuery : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代码

            //Expression<Func<Author,Book, bool>> ex1 = (m,n) => m.AuthorName.Contains("张三") &&n.AuthorID==3;
            //Expression<Func<Book, bool>> ex2 = m =>"c#".Contains(m.BookName);
            //ReplaceExpressionVisitor1 v1 = new ReplaceExpressionVisitor1();
            //BinaryExpression bex = Expression.AndAlso(v1.Visit(ex1.Body), v1.Visit(ex2.Body));
            //Expression<Func<Author, Book, bool>> b=  Expression.Lambda<Func<Author, Book, bool>>(bex, Expression.Parameter(typeof(Author)), Expression.Parameter(typeof(Book)));
            //WhereVisitor where1 = new WhereVisitor(DataBaseType.SqlServer);
            //var sql= where1.Translate<Author, Book>(b);


            using (DNetContext db = new DNetContext())
            {
                var author = db.GetSingle<Author>(m => true, q => q.OrderBy(m => m.AuthorID));

                author = db.GetSingle<Author>(m => m.AuthorName.Contains("李四") && m.IsValid == true);

                var authors= db.GetList<Author>(m => m.AuthorName.StartsWith("张三") && m.IsValid == true);

                //获取动态类型
                List<dynamic> name = db.GetDistinctList<Author>(m => m.AuthorName.StartsWith("王五") && m.IsValid == true,m=>m.AuthorName);
                
                List<string> name1 = db.GetDistinctList<Author,string>(m => m.AuthorName.StartsWith("王五") && m.IsValid == true, m => m.AuthorName);
                
                //获取最大值
                var authorid = db.GetMax<Author>(m => (int)m.AuthorID);

                //动态查询
                WhereBuilder<Author> where = new WhereBuilder<Author>();
                where.And(m=>m.AuthorName.Contains("张三"));
                where.And(m => m.AuthorID==3);
                where.Or(m=>m.IsValid==true);
                db.GetList<Author>(where.WhereExpression);

                //分页参数由前台传来
                PageFilter page = new PageFilter { PageIndex=1, PageSize=10 };
                page.And<Author>(m=> "守望者的天空".Contains(m.AuthorName));
                page.OrderBy<Author>(q=>q.OrderBy(m=>m.AuthorName).OrderByDescending(m=>m.AuthorID));
                PageDataSource<Author> pageSource= db.GetPage<Author>(page);
            }
            stopwatch.Stop(); //  停止监视  
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数 
            Response.Write("执行时间：" + milliseconds + "毫秒");
        }
    }
}