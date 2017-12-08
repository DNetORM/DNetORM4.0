using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace DNet
{
    public enum Flag
    {
        Warning,
        OK,
        Error
    }

    /// <summary>
    /// 操作结果
    /// </summary>
    public class Result  
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Result()
        {
            this.Flag = Flag.OK;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bFlag"></param>
        public Result(Flag bFlag)
        {
            this.Flag = bFlag;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bFlag"></param>
        /// <param name="sMessage"></param>
        public Result(Flag bFlag, string sMessage)
        {
            this.Flag = bFlag;
            this.Message = sMessage;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bFlag"></param>
        /// <param name="sMessage"></param>
        /// <param name="sCode"></param>
        public Result(Flag bFlag, string sMessage, string sCode)
        {
            this.Flag = bFlag;
            this.Message = sMessage;
            this.Code = sCode;
        }

        /// <summary>
        /// 操作编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 操作成功失败标识
        /// </summary>
        public Flag Flag { get; set; }

        /// <summary>
        /// 操作信息
        /// </summary>
        public string Message { get; set; }

    }


    /// <summary>
    /// 查询结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T> : Result
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Result()
            : base()
        {
            ResultObj=default(T);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Result(Flag bFlag)
            : base(bFlag)
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Result(Flag bFlag, T oObject)
        {
            this.Flag = bFlag;
            this.ResultObj = oObject;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Result(Flag bFlag, string sMessage)
            : base(bFlag, sMessage)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Result(Flag bFlag, string sMessage, T oObject)
        {
            this.Flag = bFlag;
            this.Message = sMessage;
            this.ResultObj = oObject;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Result(Flag bFlag, string sMessage, string sCode)
            : base(bFlag, sMessage, sCode)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Result(Flag bFlag, string sMessage, string sCode, T oObject)
        {
            this.Flag = bFlag;
            this.Message = sMessage;
            this.Code = sCode;
            this.ResultObj = oObject;
        }

        /// <summary>
        /// 查询结果
        /// </summary>
        public T ResultObj { get; set; }
    }
}
