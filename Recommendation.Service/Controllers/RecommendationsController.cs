using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Recommendation.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationQueue _queue;
        private readonly IQueuedRecommendationStorage _storage;
        private readonly IQueueHandler _handler;

        public RecommendationsController(Database.DatabaseContext databaseContext = null, IQueuedRecommendationStorage storage = null, IRecommendationQueue queue = null, IQueueHandler handler = null)
        {
            _storage = storage is null ? new QueuedRecommendationStorage() : storage;
            _queue = queue is null ? new RecommendationQueue(_storage) : queue;
            _handler = handler is null ? new QueueHandler(databaseContext, _queue, _storage) : handler;
        }

        [HttpPost("[action]")]
        public ActionResult<int> QueueRecommendation(string userId, [FromBody]List<int> requestedTagIds)
        {
            //// If something bad with services
            //return StatusCode(500);

            // If request params are wrong
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