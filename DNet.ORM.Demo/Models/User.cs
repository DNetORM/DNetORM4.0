using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DNet.Entity
{
    public class Author
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //[Sequence("emp_sequence")]
        public int? AuthorID { get; set; }
        public string AuthorName { get; set; }
        public int? Age { get; set; }
        public bool? IsValid { get; set; }
    }
}
