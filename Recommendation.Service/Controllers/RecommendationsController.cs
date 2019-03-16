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

        public RecommendationsController(Database.DatabaseContext databaseContext)
        {
            _storage = new QueuedRecommendationStorage();
            _queue = new RecommendationQueue(_storage);
            _handler = new QueueHandler(databaseContext, _queue, _storage);
        }

        public RecommendationsController(IQueuedRecommendationStorage storage, IRecommendationQueue queue, IQueueHandler handler)
        {
            _storage = storage;
            _queue = queue;
            _handler = handler;
        }

        [HttpPost]
        public ActionResult<int> QueueRecommendation(int? userId, [FromBody]List<int> requestedTagIds)
        {
            //// If something bad with services
            //return StatusCode(500);

            // If request params are wrong
            if (!userId.HasValue || requestedTagIds is null)
                return BadRequest();

            var queuedRecommendationId = _queue.QueueRecommendation(userId.Value, requestedTagIds);

            return queuedRecommendationId;
        }

        [HttpPost]
        public ActionResult StopRecommendation(int queuedRecommendationId)
        {
            _handler.StopRecommendation(queuedRecommendationId);

            return Ok();
        }

        [HttpGet]
        public ActionResult<Database.RecommendationStatus> Status(int queuedRecommendationId)
        {
            var status = _storage.GetRecommendationStatus(queuedRecommendationId);
            return status;
        }

        [HttpGet]
        public ActionResult<int> RecommendationId(int queuedRecommendationId)
        {
            var recommendationId = _storage.GetRecommendationId(queuedRecommendationId);
            return recommendationId;
        }
    }
}