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

            using (DNetContext db = new DNetContext())
            {
                var author = db.GetSingle<Author>(m => true, q => q.OrderBy(m => m.AuthorID));

                var book = db.GetSingle<Book>(m => ((DateTime)m.PublishDate).ToString("yyyy-MM-dd") == "2017-11-11");

                var authors = db.GetList<Author>(m => string.IsNullOrEmpty(m.AuthorName) && m.IsValid == true);

                List<dynamic> name = db.GetDistinctList<Author>(m => m.AuthorName.StartsWith("jim") && m.IsValid == true, m => m.AuthorName + "aaa");

                List<string> name1 = db.GetDistinctList<Author, string>(m => m.AuthorName.IndexOf("jim") == 2 && m.IsValid == true, m => m.AuthorName);

                var books = db.GetList<Book>(m => SubQuery.GetList<Author>(n => n.AuthorID > 10, n => n.AuthorID).Contains(m.AuthorID));

                books = db.GetList<Book>(m => m.AuthorID >= SubQuery.GetSingle<Author>(n => n.AuthorID == 10, n => n.AuthorID));

                var authorid = db.GetMax<Author>(m => (int)m.AuthorID);

                WhereBuilder<Author> where = new WhereBuilder<Author>();
                where.And(m => m.AuthorName.Contains("jim"));
                where.And(m => m.AuthorID == 3);
                where.Or(m => m.IsValid == true);
                db.GetList<Author>(where.WhereExpression);


                PageFilter page = new PageFilter { PageIndex = 1, PageSize = 10 };
                page.And<Author>(m => "jim green".Contains(m.AuthorName));
                page.OrderBy<Author>(q => q.OrderBy(m => m.AuthorName).OrderByDescending(m => m.AuthorID));
                PageDataSource<Author> pageSource = db.GetPage<Author>(page);
            }
            stopwatch.Stop(); //  停止监视  
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数 
            Response.Write("执行时间：" + milliseconds + "毫秒");
        }
    }
}