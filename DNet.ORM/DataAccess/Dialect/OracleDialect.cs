using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNet.DataAccess.Dialect
{
    public class OracleDialect : ISqlDialect
    {
        public string Contains()
        {
            return " LIKE CONCAT(CONCAT('%',{0}),'%')";
        }

        public string DateTimeToChar()
        {
            return " TO_CHAR({0} ,'{1}') ";
        }

        public string EndsWith()
        {
            return " LIKE CONCAT('%',{0})";
        }

        public string IndexOf()
        {
            return " INSTR({1},{0})-1 ";
        }

        public string ParseTimeFormat(string clrFormat)
        {
            clrFormat = clrFormat
                  .Replace("HH", "HH24")
                  .Replace("mm", "mi");
            return clrFormat;
        }

        public string SelectIdentity()
        {
            throw new NotImplementedException();
        }

        public string StartsWith()
        {
            return " LIKE CONCAT({0}),'%')";
        }

        public string ToChar()
        {
            return " TO_CHAR({0}) ";
        }

        public string ToDateTime()
        {
            return " TO_DATE({0},'yyyy-MM-dd HH24:mi:ss') ";
        }

        public string ToNumber()
        {
            return " TO_NUMBER({0}) ";
        }
    }
}
