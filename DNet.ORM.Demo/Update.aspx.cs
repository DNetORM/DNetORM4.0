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

                db.Update<Author>(m => { m.token = "";m.Age = 20;m.AuthorName = "testname"; }, m => m.AuthorID == 12);

                var author = db.GetSingle<Author>(m => true, q => q.OrderBy(m => m.AuthorID));
                if (author != null)
                {
                    author.AuthorName = "jim";
                    var effect = db.Update(author);
                }
                db.Update<Author>(m => m.AuthorName = "jim", m => m.AuthorID == 1);
                db.Update<Author>(m => { m.AuthorName = "jim"; m.Age = 30; }, m => m.AuthorID == 1);
                db.Update<Author>(m => new Author { AuthorName = m.AuthorName + "123", IsValid = true }, m => m.AuthorID == 1);
                db.UpdateOnlyFields<Author>(new Author { AuthorName = "123", Age = 20, AuthorID = 1, IsValid = true }, m => new { m.AuthorName, m.Age }, m => m.AuthorID == 1);
                db.UpdateIgnoreFields<Author>(new Author { AuthorName = "123", Age = 20, AuthorID = 1, IsValid = true }, m => m.AuthorName, m => m.AuthorID == 1);
            }
            stopwatch.Stop(); //  停止监视  
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间  
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数 
            Response.Write("执行时间：" + milliseconds + "毫秒");
        }
    }
}