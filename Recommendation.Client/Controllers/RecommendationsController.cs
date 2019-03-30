using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Recommendation.Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly Database.DatabaseContext _context;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;

        public RecommendationsController(IConfiguration configuration, Database.DatabaseContext context, HttpClient httpClient)
        {
            _context = context;
            _configuration = configuration;
            _client = httpClient;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<int>> Generate([FromBody]IEnumerable<int> tagIds, string userId)
        {
            var host = _configuration["Services:Recommendation:Uri"];
            var port = _configuration["Services:Recommendation:Port"];
            var uri = new Uri($"http://{host}:{port}/api/recommendations/QueueRecommendation?userId={userId}");

            var result = await _client.PostAsJsonAsync(uri, tagIds);

            if (result.IsSuccessStatusCode)
                return BadRequest();

            var queuedRecommendationId = await result.Content.ReadAsAsync<int>();

            var timeoutDate = DateTime.Now + TimeSpan.FromMinutes(10);

            while(true)
            {
                if (timeoutDate <= DateTime.Now)
                    return BadRequest("Recommendation timed out");

                var statusResult = await CheckStatus(queuedRecommendationId);

                if (statusResult.Result.GetType() != typeof(OkObjectResult))
                    return BadRequest();

                if (statusResult.Value != Database.RecommendationStatus.InProgress
                    && statusResult.Value != Database.RecommendationStatus.Queued)
                    break;
            }

            var recommendationResult = await GetRecommendationId(queuedRecommendationId);

            if (recommendationResult.Result.GetType() != typeof(OkObjectResult))
                return BadRequest();

            return Ok(recommendationResult.Value);
        }

        [HttpPost("status")]
        public async Task<ActionResult<Database.RecommendationStatus>> CheckStatus(int queuedRecommendationId)
        {
            var host = _configuration["Services:Recommendation:Uri"];
            var port = _configuration["Services:Recommendation:Port"];
            var uri = new Uri($"http://{host}:{port}/api/recommendations/status?queuedRecommendationId={queuedRecommendationId}");

            var result = await _client.GetAsync(uri);

            if (result.IsSuccessStatusCode)
                return BadRequest();

            var status = await result.Content.ReadAsAsync<Database.RecommendationStatus>();

            return Ok(status);
        }

        [HttpPost("id")]
        public async Task<ActionResult<int>> GetRecommendationId(int queuedRecommendationId)
        {
            var host = _configuration["Services:Recommendation:Uri"];
            var port = _configuration["Services:Recommendation:Port"];
            var uri = new Uri($"http://{host}:{port}/api/recommendations/RecommendationId?queuedRecommendationId={queuedRecommendationId}");

            var result = await _client.GetAsync(uri);

            if (result.IsSuccessStatusCode)
                return BadRequest();

            var recommendationId = await result.Content.ReadAsAsync<int>();

            return Ok(recommendationId);
        }
    }
}