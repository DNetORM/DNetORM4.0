﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNet.DataAccess.Dialect
{
    public class SQLiteDialect : ISqlDialect
    {
        public string Contains()
        {
            return " LIKE CONCAT('%',{0},'%')";
        }

        public string DateTimeToChar()
        {
           return " DATE_FORMAT({0},'{1}') ";
        }

        public string EndsWith()
        {
            return " LIKE CONCAT('%',{0})";
        }

        public string IndexOf()
        {
            return " LOCATE({0},{1})-1 ";
        }

        public string ParseTimeFormat(string clrFormat)
        {
            clrFormat = clrFormat
                   .Replace("yyyy", "%Y")
                   .Replace("MM", "%m")
                   .Replace("dd", "%d")
                   .Replace("HH", "%H")
                   .Replace("mm", "%i")
                   .Replace("ss", "%s");
            return clrFormat;
        }

        public string SelectIdentity()
        {
            return "; SELECT LAST_INSERT_ID()";
        }

        public string StartsWith()
        {
            return " LIKE CONCAT({0},'%')";
        }

        public string ToChar()
        {
            return " CAST({0} AS CHAR) ";
        }

        public string ToDateTime()
        {
            return " CAST({0} AS DATETIME) ";
        }

        public string ToNumber()
        {
            return " CAST({0} AS INT) ";
        }

        public string DateDiff(DateDiffType type)
        {
            switch (type)
            {
                case DateDiffType.Day:
                    return " DATEDIFF({1},{0}) ";
                case DateDiffType.Hour:
                case DateDiffType.Minute:
                default:
                    throw new NotImplementedException("MySql DateDiff函数不支持Hour Minute此格式");
            }

        }
    }
}
