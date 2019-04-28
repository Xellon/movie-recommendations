using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Recommendation.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationQueue _queue;
        private readonly IQueuedRecommendationStorage _storage;
        private readonly IQueueHandler _handler;

        public RecommendationsController(
            IConfiguration configuration, IQueuedRecommendationStorage storage,
            IRecommendationQueue queue = null, IQueueHandler handler = null,
            ILogger<RecommendationsController> logger = null)
        {
            _storage = storage;
            _queue = queue is null ? new RecommendationQueue(_storage) : queue;

            if (handler is null)
            {
                var optionsBuilder = new DbContextOptionsBuilder<Database.DatabaseContext>();
                optionsBuilder.UseSqlServer(configuration["DatabaseConnectionString"]);
                _handler = QueueHandler.GetOrCreate(optionsBuilder.Options, configuration, _queue, _storage, logger);
            }
            else
                _handler = handler;
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