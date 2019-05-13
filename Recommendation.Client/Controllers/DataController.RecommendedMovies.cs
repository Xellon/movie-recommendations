using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Recommendation.Client.Controllers
{
    public partial class DataController : ControllerBase
    {
        [HttpGet("recommendedmovies")]
        public async Task<IEnumerable<RecommendedMovie>> RecommendedMovies(string userId) => 
            await _context.Recommendations
                .Where(r => r.UserId == userId)
                .Join(_context.RecommendedMovies,
                    r => r.Id,
                    m => m.RecommendationId,
                    (r, m) => m)
                .Select(r => new RecommendedMovie { MovieId = r.MovieId, PossibleRating = (float)r.PossibleRating })
                .ToListAsync();

        [HttpGet("recommendedmovies/{id}")]
        public async Task<IEnumerable<RecommendedMovie>> RecommendedMovies([FromRoute]int id) =>
            await _context.RecommendedMovies
                .Where(m => m.RecommendationId == id)
                .Select(r => new RecommendedMovie { MovieId = r.MovieId, PossibleRating = (float)r.PossibleRating })
                .ToListAsync();

        [HttpGet("recommendedmovies/{id}")]
        public async Task<ActionResult<IEnumerable<RecommendedMovie>>> RecommendedMovies([FromRoute]int id, string userId)
        {
            var recommendation = await _context.Recommendations.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (recommendation is null)
                return BadRequest();

            return Ok(await RecommendedMovies(id));
        }

        [HttpGet("recommendedmovies/latest")]
        public async Task<ActionResult<IEnumerable<RecommendedMovie>>> LatestRecommendedMovies(string userId)
        {
            var recommendation = await _context.Recommendations.OrderByDescending(r => r.Date).FirstOrDefaultAsync(r => r.UserId == userId);

            if (recommendation is null)
                return BadRequest();

            return Ok(await RecommendedMovies(recommendation.Id));
        }
    }
 
    public struct RecommendedMovie
    {
        public int MovieId { get; set; }
        public float PossibleRating { get; set; }
    }
}
