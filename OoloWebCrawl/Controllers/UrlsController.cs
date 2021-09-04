using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OoloWebCrawl.Application;
using OoloWebCrawl.Storage;
using System.Linq;
using System.Threading.Tasks;

namespace OoloWebCrawl.Controllers
{
    [Route("api/[controller]")]
    public class UrlsController : ControllerBase
    {
        private readonly IOptions<ApplicationConfig> _config;
        private readonly IQueueProducerConsumer _crawlingService;
        private readonly ITitlesRepository _titles;

        public UrlsController(IOptions<ApplicationConfig> config, IQueueProducerConsumer crawlingService, ITitlesRepository titles)
        {
            _config = config;
            _crawlingService = crawlingService;
            _titles = titles;
        }
        [HttpGet(nameof(Titles))]
        public async Task<IActionResult> Titles([FromQuery] int? minutes)
        {
            var requestedSpan = minutes ?? _config.Value.DefaultMinutesAgo;
            var titles = await _titles.GetTitlesFrom(requestedSpan);
            return Ok(titles);
        }

        [HttpPost(nameof(Crawl))]
        public async Task<IActionResult> Crawl([FromBody] RequestDto request)
        {
            if (request == null || !request.Urls.Any())
            {
                return BadRequest("request.urls must contain at least on url");
            }

            await _crawlingService.SendCrawlingRequests(request.Urls);
            return Ok();
        }
    }
}
