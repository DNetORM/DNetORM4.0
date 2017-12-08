using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNet.Cache
{
    /// <summary>
    /// 属性访问器
    /// </summary>
    public interface IPropertyAccessor
    {
        object GetValue(object instance);

        void SetValue(object instance, object value);
    }
}
