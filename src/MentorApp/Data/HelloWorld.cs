using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MentorApp.Data
{
    public class HelloWorld
    {
        [MaxLength(50)]
        [Column("test")]
        public string Test { get; set; }
    }
}