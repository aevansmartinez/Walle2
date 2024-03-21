using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace testing2.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string IMDBLink { get; set; }
        public string Genre { get; set; }
        [Display(Name = "Release Year")]
        public int ReleaseYear { get; set; }
        public byte[]? Poster { get; set; }
    }
}
