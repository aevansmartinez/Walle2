using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using testing2.Data;
using testing2.Models;
using VaderSharp2;

namespace testing2.Controllers
{
    public class MoviesController : Controller
    {
        public async Task<IActionResult> GetMoviePoster(int id)
        {
            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            var imageData = movie.Poster;
            return File(imageData, "image/jpg");
        }

        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
            /* return _context.Movie != null ? 
                         View(await _context.Movie.ToListAsync()) :
                         Problem("Entity set 'ApplicationDbContext.Movie'  is null.");
            */
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Movie == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            MovieVM movieDetailsVM = new MovieVM();
            movieDetailsVM.Movie = movie;
            movieDetailsVM.ActorMovie = _context.ActorMovie.Where(m => m.MovieId == movie.Id).Include(m => m.Movie).Include(m => m.Actor).ToList();
            movieDetailsVM.wikiPages = new List<string>();
            movieDetailsVM.pageSentiment = new List<double>();

            //WIKIPEDIA~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
            List<string> textToExamine = await SearchWikipediaAsync(movie.Title);

            var analyzer = new SentimentIntensityAnalyzer();
            int validResults = 0;
            double resultsTotal = 0;

            foreach (string textValue in textToExamine)
            {
                var results = analyzer.PolarityScores(textValue.Substring(0, textValue.Length < 2500 ? textValue.Length : 2500));
                if (results.Compound != 0)
                {
                    resultsTotal += results.Compound;
                    if (validResults <= 100)
                    {
                        string shortenedText = textValue.Substring(0, Math.Min(500, textValue.Length));
                        if (textValue.Length > 500)
                            shortenedText += "...";
                        movieDetailsVM.wikiPages.Add(shortenedText);
                        movieDetailsVM.pageSentiment.Add(results.Compound);
                    }
                    validResults++;
                }
            }

            double avgResult = Math.Round(resultsTotal / validResults, 2);
            movieDetailsVM.Sentiment = avgResult.ToString();

            //END WIKIPEDIA~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

            return View(movieDetailsVM);
        }

        //BLACKBOARD WIKIPEDIA~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        public static readonly HttpClient client = new HttpClient();
        public static async Task<List<string>> SearchWikipediaAsync(string searchQuery)
        {
            string baseUrl = "https://en.wikipedia.org/w/api.php";
            string url = $"{baseUrl}?action=query&list=search&srlimit=100&srsearch={Uri.EscapeDataString(searchQuery)}&format=json";

            List<string> textToExamine = new List<string>();

            try
            {
                //Ask WikiPedia for a list of pages that relate to the query
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonDocument = JsonDocument.Parse(responseBody);
                var searchResults = jsonDocument.RootElement.GetProperty("query").GetProperty("search");

                foreach (var item in searchResults.EnumerateArray())
                {
                    var pageId = item.GetProperty("pageid").ToString();
                    //Ask WikiPedia for the text of each page in the query results
                    string pageUrl = $"{baseUrl}?action=query&pageids={pageId}&prop=extracts&explaintext&format=json";

                    HttpResponseMessage pageResponse = await client.GetAsync(pageUrl);
                    pageResponse.EnsureSuccessStatusCode();
                    string pageResponseBody = await pageResponse.Content.ReadAsStringAsync();

                    var jsonPageDocument = JsonDocument.Parse(pageResponseBody);
                    var pageContent = jsonPageDocument.RootElement.GetProperty("query").GetProperty("pages").GetProperty(pageId).GetProperty("extract").GetString();

                    textToExamine.Add(pageContent);
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

            return textToExamine;
        }
        //END BLACKBOARD WIKIPEDIA~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,IMDBLink,Genre,ReleaseYear,Poster")] Movie movie, IFormFile Poster)
        {
            if (ModelState.IsValid)
            {
                if (Poster != null && Poster.Length > 0)
                {
                    var memoryStream = new MemoryStream();
                    await Poster.CopyToAsync(memoryStream);
                    movie.Poster = memoryStream.ToArray();
                }
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Movie == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,IMDBLink,Genre,ReleaseYear,Poster")] Movie movie, IFormFile Poster)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(movie.Poster));
            Movie existingMovie = _context.Movie.AsNoTracking().FirstOrDefault(m => m.Id == id);

            if (ModelState.IsValid)
            {
                try
                {
                    if (Poster != null && Poster.Length > 0)
                    {
                        var memoryStream = new MemoryStream();
                        await Poster.CopyToAsync(memoryStream);
                        movie.Poster = memoryStream.ToArray();
                    }
                    else if (existingMovie != null)
                    {
                        movie.Poster = existingMovie.Poster;
                    }
                    else
                    {
                        movie.Poster = new byte[0];
                    }
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Movie == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Movie == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Movie'  is null.");
            }
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
            // return (_context.Movie?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
