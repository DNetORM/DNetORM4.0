using DNet.DataAccess;
using DNet.Entity;
using DNet.ORM.Demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DNet.ORM.Demo
{
    public partial class testweb : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            using (DNetContext db = new DNetContext())
            {
                //var r= db.GetList<Book>(m=>Convert.ToDateTime(m.PublishDate).ToString("yyyy-MM-dd")=="2017-10-11");
                var r = db.GetList<Book>(m => m.PublishDate>Convert.ToDateTime("2017-10-11"));
            }
        }
    }
}