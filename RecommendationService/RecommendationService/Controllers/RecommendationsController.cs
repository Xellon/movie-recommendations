using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RecommendationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        [HttpPost]
        public ActionResult<int> Generate(int userId, [FromBody]List<int> requestedTagIds)
        {
            //// If something bad with services
            //return StatusCode(500);

            // If request params are wrong
            return BadRequest();

            //return Ok(generatedRecommendationId);
        }
    }
}