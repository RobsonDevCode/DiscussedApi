using DiscussedApi.Processing.Profile;
using DiscussedApi.Processing.Topics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace DiscussedApi.Controllers.V1.Topic
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly ITopicProcessing _topicProcessing;
        public TopicController(ITopicProcessing topicProcessing)
        {
            _topicProcessing = topicProcessing;
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("generate")]
        public async Task<IActionResult> GenerateTopicAsync()
        {
            try
            {
                return Ok(await _topicProcessing.GetTopicAsync());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }


    }
}
