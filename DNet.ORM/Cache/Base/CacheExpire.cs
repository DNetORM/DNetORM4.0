﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNet.Cache
{
    /// <summary>
    /// 过期缓存
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class CacheExpire<TValue>
    {
        /// <summary>
        /// 最新访问时间
        /// </summary>
        public DateTime VisitTime { get; set; }

        public TValue Data { get; set; }

        public CacheExpire()
        { 
        }

        public CacheExpire(TValue data,DateTime dateTime)
        {
            this.Data = data;
            this.VisitTime = dateTime;
        }
    }
}
