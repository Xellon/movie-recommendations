using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Recommendation.Database;

namespace Recommendation.Client.Controllers
{
    [Route("api/[controller]")]
    public partial class DataController : ControllerBase
    {
        private DatabaseContext _context;

        public DataController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpGet("[action]")]
        public IEnumerable<Movie> Movies()
        {
            return _context.Movies;
        }

        [HttpGet("[action]")]
        public IEnumerable<Tag> Tags()
        {
            return _context.Tags;
        }
    }
}