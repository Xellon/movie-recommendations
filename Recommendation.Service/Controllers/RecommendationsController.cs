using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Recommendation.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationQueue _queue;

        public RecommendationsController(Database.DatabaseContext databaseContext)
        {
            _queue = new RecommendationQueue(databaseContext);
        }

        public RecommendationsController(IRecommendationQueue queue)
        {
            _queue = queue;
        }

        [HttpPost]
        public ActionResult<int> QueueRecommendation(int? userId, [FromBody]List<int> requestedTagIds)
        {
            //// If something bad with services
            //return StatusCode(500);

            // If request params are wrong
            if (!userId.HasValue || requestedTagIds is null)
                return BadRequest();

            _queue.QueueRecommendation(userId.Value, requestedTagIds);

            return Ok();
        }

        [HttpPost]
        public ActionResult StopRecommendation(int queuedRecommendationId)
        {
            _queue.StopRecommendation(queuedRecommendationId);

            return Ok();
        }

        [HttpGet]
        public ActionResult<Database.RecommendationStatus> Status(int queuedRecommendationId)
        {
            return _queue.GetRecommendationStatus(queuedRecommendationId);
        }

        [HttpGet]
        public ActionResult<int> RecommendationId(int queuedRecommendationId)
        {
            return _queue.GetRecommendationId(queuedRecommendationId);
        }
    }
}