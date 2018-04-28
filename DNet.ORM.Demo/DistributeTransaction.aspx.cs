using DNet.DataAccess;
using DNet.Entity;
using DNet.Transaction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DNet.ORM.Demo
{
    public partial class DistributeTransaction : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代
            DNetTransaction transaction = new DNetTransaction();
            transaction.BeginTransaction();
            try
            {
                using (DNetContext db = new DNetContext())
                {
                    List<Author> authors = new List<Author>();
                    for (int i = 0; i <= 100; i++)
                    {
                        authors.Add(new Author { AuthorName = "测试" + i.ToString(), Age = 20, IsValid = true });
                    }
                    db.Add(authors);
                    transaction.Commit();
                }
            }
            catch
            {
                transaction.Rollback();
            }
            stopwatch.Stop(); //  停止监视  
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数 
            Response.Write("执行时间：" + milliseconds + "毫秒");
        }
    }
}