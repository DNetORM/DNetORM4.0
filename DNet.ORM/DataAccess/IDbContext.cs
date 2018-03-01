using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNet.DataAccess
{
    public interface IDbContext: IDisposable
    {
        IDatabase DataBase { get; set; }
    }
}
