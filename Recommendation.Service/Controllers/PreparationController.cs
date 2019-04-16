using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Recommendation.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreparationController : ControllerBase
    {
        private readonly DbContextOptions<Database.DatabaseContext> _dbOptions;
        private readonly PythonEngineOptions _engineOptions;

        public PreparationController(IConfiguration configuration, IOptionsMonitor<PythonEngineOptions> engineOptions)
        {
            _engineOptions = engineOptions.CurrentValue;
            var optionsBuilder = new DbContextOptionsBuilder<Database.DatabaseContext>();
            optionsBuilder.UseSqlServer(configuration["DatabaseConnectionString"]);

            _dbOptions = optionsBuilder.Options;
        }


        [HttpPost("[action]")]
        public async Task<ActionResult> Vectorize()
        {
            var engine = new PythonRecommendationEngine(_dbOptions, _engineOptions);
            var sth = await engine.VectorizeDescriptions();
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> PrepareData()
        {
            var engine = new PythonRecommendationEngine(_dbOptions, _engineOptions);
            await engine.PrepareData();
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Generate()
        {
            var engine = new PythonRecommendationEngine(_dbOptions, _engineOptions);
            var sth = await engine.GenerateRecommendation(new RecommendationParameters {
                UserId = "1",
                RequestedTagIds = new List<int> { 16, 35 } // Animation, Comedy
            });
            return Ok();
        }
    }
}