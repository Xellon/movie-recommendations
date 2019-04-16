using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Recommendation.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreparationController : ControllerBase
    {
        private readonly DbContextOptions<Database.DatabaseContext> _dbOptions;

        public PreparationController(IConfiguration configuration)
        {
            var optionsBuilder = new DbContextOptionsBuilder<Database.DatabaseContext>();
            optionsBuilder.UseSqlServer(configuration["DatabaseConnectionString"]);

            _dbOptions = optionsBuilder.Options;
        }


        [HttpPost("[action]")]
        public async Task<ActionResult> Vectorize()
        {
            var engine = new PythonRecommendationEngine(_dbOptions);
            var sth = await engine.VectorizeDescriptions();
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> PrepareData()
        {
            var engine = new PythonRecommendationEngine(_dbOptions);
            await engine.PrepareData();
            return Ok();
        }
    }
}