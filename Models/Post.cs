using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SelenicSparkApp_v2_WebAPI.Models
{
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required int PostID { get; set; }
        public required string Title { get; set; }
        public string? Text { get; set; }
        public required string Author { get; set; }
        public required DateTimeOffset Created { get; set; }

        public Post() { }
    }
}
