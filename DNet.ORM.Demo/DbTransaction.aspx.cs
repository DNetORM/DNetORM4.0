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
    public partial class DbTransaction : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代
            using (DNetContext db = new DNetContext())
            {
                try
                {
                    db.DataBase.BeginTransaction();
                    List<Author> authors = new List<Author>();
                    for (int i = 0; i <= 10; i++)
                    {
                        authors.Add(new Author { AuthorName = "jack" + i.ToString(), Age = 20, IsValid = true });
                    }
                    db.Add(authors);

                    var aus = db.GetList<Author>(m=>true);
                    db.DataBase.Commit();
                }
                catch(Exception ex)
                {
                    db.DataBase.Rollback();
                }
            }
            stopwatch.Stop(); //  停止监视  
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数 
            Response.Write("执行时间：" + milliseconds + "毫秒");
        }
    }
}