using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movies.Models;
using Movies.ViewModels;
using NToastNotify;

namespace Movies.Controllers
{
	public class MoviesController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IToastNotification _ToastNotification;
        public MoviesController(ApplicationDbContext context,IToastNotification toastNotification)
        {
			_context = context;
			_ToastNotification = toastNotification;
        }
        public async Task<IActionResult> Index()
		{
			List<Movie> movies = await _context.Movies.OrderByDescending(i=>i.Rate).ToListAsync();
			return View(movies);
		}

		public async Task<IActionResult> Create()
		{
			var viewmodel = new MovieFormViewModel
			{
				Genres = await _context.Genres.OrderBy(n => n.Name).ToListAsync()
			};
			
			return View("MovieForm", viewmodel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(MovieFormViewModel model)
		{
			if(!ModelState.IsValid)
			{
				model.Genres = await _context.Genres.OrderBy(n => n.Name).ToListAsync();
				return View("MovieForm",model);
			}
			var file = Request.Form.Files;
			if(!file.Any())
			{
				model.Genres = await _context.Genres.OrderBy(n => n.Name).ToListAsync();
				ModelState.AddModelError("Poster", "Please add Movie Poster");
				return View("MovieForm",model);
			}

			var poster = file.FirstOrDefault();
			var allowedExtenstions = new List<string> { ".jpg", ".png"};

			if (!allowedExtenstions.Contains(Path.GetExtension(poster.FileName).ToLower()))
			{
				model.Genres = await _context.Genres.OrderBy(n => n.Name).ToListAsync();
				ModelState.AddModelError("Poster", "Please add Movie Poster");
				return View("MovieForm", model);
			}

			if(poster.Length > 1048576)
			{
				model.Genres = await _context.Genres.OrderBy(n => n.Name).ToListAsync();
				ModelState.AddModelError("Poster", "poster can't be more than 1 MB");
				return View("MovieForm", model);
			}

			using var datastream = new MemoryStream();
			await poster.CopyToAsync(datastream);

			var movie = new Movie
			{
				Title = model.Title,
				GenreId = model.GenreId,
				Year = model.Year,
				Rate = model.Rate,
				Storyline = model.Storyline,
				Poster = datastream.ToArray()
			};

			_context.Movies.Add(movie);
			_context.SaveChanges();

			_ToastNotification.AddSuccessToastMessage("Movie Created");

			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
				return BadRequest();

			var movie = await _context.Movies.FindAsync(id);

			if (movie == null)
				return NotFound();

			var viewmodel = new MovieFormViewModel
			{
				Id = movie.Id,
				Title = movie.Title,
				Rate = movie.Rate,
				Storyline = movie.Storyline,
				Year = movie.Year,
				GenreId = movie.GenreId,
				Poster = movie.Poster,
				Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync()
			};
			return View("MovieForm",viewmodel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(MovieFormViewModel model)
		{
			if (!ModelState.IsValid)
			{
				model.Genres = await _context.Genres.OrderBy(n => n.Name).ToListAsync();
				return View("MovieForm", model);
			}

			var movie = await _context.Movies.FindAsync(model.Id);

			if (movie == null)
				return NotFound();

			var files = Request.Form.Files;

			if(files.Any())
			{
				var poster = files.FirstOrDefault();
				using var datastream = new MemoryStream();
				await poster.CopyToAsync(datastream);

				var allowedExtenstions = new List<string> { ".jpg", ".png" };

				model.Poster = datastream.ToArray();
				if (!allowedExtenstions.Contains(Path.GetExtension(poster.FileName).ToLower()))
				{
					model.Genres = await _context.Genres.OrderBy(n => n.Name).ToListAsync();
					ModelState.AddModelError("Poster", "Please add Movie Poster");
					return View("MovieForm", model);
				}

				if (poster.Length > 1048576)
				{
					model.Genres = await _context.Genres.OrderBy(n => n.Name).ToListAsync();
					ModelState.AddModelError("Poster", "poster can't be more than 1 MB");
					return View("MovieForm", model);
				}

				movie.Poster = datastream.ToArray();
			}



			movie.Title = model.Title;
			movie.GenreId = model.GenreId;
			movie.Rate = model.Rate;
			movie.Year = model.Year;
			movie.Storyline = model.Storyline;

			_context.SaveChanges();

			_ToastNotification.AddSuccessToastMessage("Movie Updated");

			return RedirectToAction("Index");
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
				return BadRequest();

			var movie = await _context.Movies.Include(m=>m.Genre).SingleOrDefaultAsync(i=>i.Id == id);

			if (movie == null)
				return NotFound();

			return View(movie);
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
				return BadRequest();

			var movie = await _context.Movies.FindAsync(id);

			if (movie == null)
				return NotFound();

			_context.Movies.Remove(movie);
			_context.SaveChanges();

			return Ok();
		}
	}
}
