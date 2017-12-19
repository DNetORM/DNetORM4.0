﻿using System;

namespace DNet.Entity
{
    public class Book
    {
        [Key(IsAutoGenerated = true)]
        public int? BookID { get; set; }
        public string BookName { get; set; }
        public int? AuthorID { get; set; }

        [NotColumn]
        public string AuthorName { get; set; }
        public double? Price { get; set; }

        public DateTime? PublishDate { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
