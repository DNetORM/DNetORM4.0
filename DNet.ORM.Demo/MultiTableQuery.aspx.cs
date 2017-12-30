using DNet.DataAccess;
using DNet.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DNet.ORM.Demo
{
    /// <summary>
    /// 多表查询
    /// </summary>
    public partial class MultiTableQuery : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代
            using (DNetContext db = new DNetContext())
            {

                var books = db.JoinQuery.LeftJoin<Book, Author>((m, n) => m.AuthorID == n.AuthorID && n.IsValid == true)
                    .Fields<Book, Author>((m, n) => new { BookName = m.BookName + "123", AuthorName = SqlFunctions.Count(n.AuthorName) })
                    .OrderByAsc<Book>(m => m.BookName)
                    .GroupBy<Book, Author>((m, n) => new { m.BookName, n.AuthorName })
                    .Where<Book, Author>((m, n) => m.Price > 10 && n.IsValid == true&&SubQuery.GetList<Author>(n1 => n1.AuthorID >= 1, n1 => n1.AuthorID).Contains(m.AuthorID))
                    .GetList<Book>();



                var join = db.JoinQueryAlias.LeftJoin<Book, Author>((m, n) => m.AuthorID == n.AuthorID && n.IsValid == true)
                    .InnerJoin<Book, Author>((m1, n) => m1.AuthorID == n.AuthorID && n.IsValid == true)
                    .Fields<Book>(m1 => new Book { BookName=m1.BookName+"123" })
                    .OrderByAsc<Book>(m => m.BookName);
                PageFilter page = new PageFilter { PageIndex = 1, PageSize = 10 };//分页参数前台传来
                join.GetPage<Book>(page);

               

            }
            stopwatch.Stop(); //  停止监视  
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数 
            Response.Write("执行时间：" + milliseconds + "毫秒");
        }
    }
}