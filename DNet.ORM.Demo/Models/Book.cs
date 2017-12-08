using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DNet.Entity
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? BookID { get; set; }
        public string BookName { get; set; }
        public int? AuthorID { get; set; }

        [NotMapped]
        public string AuthorName { get; set; }
        public double? Price { get; set; }

        public DateTime? PublishDate { get; set; }
    }
}
