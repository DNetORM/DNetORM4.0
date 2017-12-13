using DNet.DataAccess;
using DNet.Entity;
using DNet.ORM.Demo.Models;
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
    public partial class testweb : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代
            using (DNetContext db = new DNetContext())
            {
                Expression<Func<Author, dynamic>> ex = m => new { m.AuthorID, AuthorName = m.AuthorName + "aaa" };
                WhereVisitor wv = new WhereVisitor(DataBaseType.SqlServer);
                wv.Translate(ex);
              
                //获取动态类型
                List<dynamic> name = db.GetDistinctList<Author>(m => m.AuthorName.StartsWith("王五") && m.IsValid == true, m => m.AuthorName + "aaa" );
                var r = db.GetList<Test>(m =>true);
            }
            stopwatch.Stop(); //  停止监视  
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数 
            Response.Write("执行时间：" + milliseconds + "毫秒");
        }
    }
}