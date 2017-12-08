using DNet.DataAccess;
using DNet.Entity;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DNet.ORM.Demo
{
    public partial class SQLQuery : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代码
            using (DNetContext db = new DNetContext())
            {
                StringBuilder sql = new StringBuilder();
                List<DbParameter> parameters = new List<DbParameter>();

                sql.AppendFormat(@"SELECT {0},A.AuthorName FROM Book B 
LEFT JOIN Author A ON A.AuthorID=B.AuthorID WHERE", SqlBuilder.GetSelectAllFields<Book>("B"));
                sql.Append(" B.BookID>@BookID ");
                parameters.Add(db.GetDbParameter("BookID",1));

                PageDataSource<Book> books = db.GetPage<Book>(sql.ToString(),new PageFilter { PageIndex=1, PageSize=5 }, parameters.ToArray());
                List<Book> bks = db.GetList<Book>(sql.ToString(), parameters.ToArray());
            }
            stopwatch.Stop(); //  停止监视  
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数 
            Response.Write("执行时间：" + milliseconds + "毫秒");
        }
    }
}