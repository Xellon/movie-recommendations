using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Recommendation.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationQueue _queue;
        private readonly IQueuedRecommendationStorage _storage;
        private readonly IQueueHandler _handler;

        public RecommendationsController(IConfiguration configuration = null, IQueuedRecommendationStorage storage = null, IRecommendationQueue queue = null, IQueueHandler handler = null)
        {
            _storage = storage;
            _queue = queue is null ? new RecommendationQueue(_storage) : queue;
            
            var optionsBuilder = new DbContextOptionsBuilder<Database.DatabaseContext>();
            optionsBuilder.UseSqlServer(configuration["DatabaseConnectionString"]);

            _handler = handler is null ? QueueHandler.GetOrCreate(optionsBuilder.Options, _queue, _storage) : handler;
        }

        [HttpPost("[action]")]
        public ActionResult<int> QueueRecommendation(string userId, [FromBody]List<int> requestedTagIds)
        {
            if (userId is null || requestedTagIds is null)
                return BadRequest();

            var queuedRecommendationId = _queue.QueueRecommendation(userId, requestedTagIds);

            return queuedRecommendationId;
        }

        [HttpPost("[action]")]
        public ActionResult StopRecommendation(int queuedRecommendationId)
        {
            _handler.StopRecommendation(queuedRecommendationId);

            return Ok();
        }

        [HttpGet("[action]")]
        public ActionResult<Database.RecommendationStatus> Status(int queuedRecommendationId)
        {
            var status = _storage.GetRecommendationStatus(queuedRecommendationId);
            return status;
        }

        [HttpGet("[action]")]
        public ActionResult<int> RecommendationId(int queuedRecommendationId)
        {
            var recommendationId = _storage.GetRecommendationId(queuedRecommendationId);
            return recommendationId;
        }
    }
}