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
    public partial class Update : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代
            using (DNetContext db = new DNetContext())
            {
                //var author = db.GetSingle<Author>(m => true, q => q.OrderBy(m => m.AuthorID));
                //if (author!=null)
                //{
                //    author.AuthorName = "jim";
                //    var effect = db.Update(author);
                //}

                //int authorid = db.GetMax<Author>(m => (int)m.AuthorID);
                db.Update<Author>(m=>m.AuthorID= m.AuthorID, m=>m.Age, m => m.AuthorID == 1);

            }
            stopwatch.Stop(); //  停止监视  
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数 
            Response.Write("执行时间：" + milliseconds + "毫秒");
        }
    }
}