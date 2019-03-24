using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recommendation.Database;

namespace Recommendation.Client.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public partial class DataController : ControllerBase
    {
        private DatabaseContext _context;

        public DataController(DatabaseContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<IEnumerable<Movie>> Movies(bool full = false)
        {
            if (full)
                return await _context.Movies.ToListAsync();

            return await _context.Movies
                .Select(m => new Movie
                {
                    AverageRating = m.AverageRating,
                    Id = m.Id,
                    ImageUrl = m.ImageUrl,
                    Title = m.Title,
                    Tags = m.Tags
                }).ToListAsync();
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<Tag>> Tags()
        {
            return await _context.Tags.ToListAsync();
        }

    }
}