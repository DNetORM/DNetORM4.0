using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNet.DataAccess
{
    /// <summary>
    /// 连接条件
    /// Author Jack Liu
    /// </summary>
    public class JoinRelation
    {
        public JoinRelation() { }
        public JoinType JoinType { get; set; }

        public string LeftTable { get; set; }

        public string RightTable { get; set; }

        public string OnSql { get; set; }

        public List<DbParameter> Parameters { get; set; }
    }
}
