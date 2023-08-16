using Movies.Models;
using System.ComponentModel.DataAnnotations;

namespace Movies.ViewModels
{
	public class MovieFormViewModel
	{
        public int Id { get; set; }
        [Required, StringLength(250)]
		public string Title { get; set; }

		public int Year { get; set; }

		[Range(0,10)]
		public double Rate { get; set; }

		[Required, StringLength(2500)]
		public string Storyline { get; set; }

		public byte[] Poster { get; set; }

		[Display(Name = "Genre")]
		public byte GenreId { get; set; }
		public IEnumerable<Genre> Genres { get; set; }
	}
}
