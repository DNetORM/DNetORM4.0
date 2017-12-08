using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DNet.ORM.Demo.Models
{
    public class Test
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
    }
}