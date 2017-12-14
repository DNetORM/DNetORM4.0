using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNet
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotColumnAttribute:Attribute
    {
        public NotColumnAttribute()
        {
        
        }
       
    }
}
