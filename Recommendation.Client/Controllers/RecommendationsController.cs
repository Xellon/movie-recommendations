using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Recommendation.Client.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
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

        private bool IsResultOk<T>(ActionResult<T> result)
        {
            return result.Result is null || result.Result.GetType() == typeof(OkResult);
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<int>> QueueRecommendation([FromBody]IEnumerable<int> tagIds, string userId)
        {
            var host = _configuration["Services:Recommendation:Uri"];
            var port = _configuration["Services:Recommendation:Port"];
            var uri = new Uri($"http://{host}:{port}/api/recommendations/QueueRecommendation?userId={userId}");

            var result = await _client.PostAsJsonAsync(uri, tagIds);

            if (!result.IsSuccessStatusCode)
                return BadRequest();

            return await result.Content.ReadAsAsync<int>();
        }

        [HttpGet("status")]
        public async Task<ActionResult<Database.RecommendationStatus>> CheckStatus(int queuedRecommendationId)
        {
            var host = _configuration["Services:Recommendation:Uri"];
            var port = _configuration["Services:Recommendation:Port"];
            var uri = new Uri($"http://{host}:{port}/api/recommendations/status?queuedRecommendationId={queuedRecommendationId}");

            var result = await _client.GetAsync(uri);

            if (!result.IsSuccessStatusCode)
                return BadRequest();

            var status = await result.Content.ReadAsAsync<Database.RecommendationStatus>();

            return status;
        }

        [HttpPost("id")]
        public async Task<ActionResult<int>> GetRecommendationId(int queuedRecommendationId)
        {
            var host = _configuration["Services:Recommendation:Uri"];
            var port = _configuration["Services:Recommendation:Port"];
            var uri = new Uri($"http://{host}:{port}/api/recommendations/RecommendationId?queuedRecommendationId={queuedRecommendationId}");

            var result = await _client.GetAsync(uri);

            if (!result.IsSuccessStatusCode)
                return BadRequest();

            var recommendationId = await result.Content.ReadAsAsync<int>();

            return recommendationId;
        }

        [HttpPost("stop")]
        public async Task<ActionResult> StopRecommendation(int queuedRecommendationId)
        {
            var host = _configuration["Services:Recommendation:Uri"];
            var port = _configuration["Services:Recommendation:Port"];
            var uri = new Uri($"http://{host}:{port}/api/recommendations/StopRecommendation?queuedRecommendationId={queuedRecommendationId}");

            var result = await _client.PostAsync(uri, null);

            if (!result.IsSuccessStatusCode)
                return BadRequest();

            return Ok();
        }
    }
}