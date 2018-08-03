using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagingServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace MessagingServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IBackgroundService _backgroundService;

        public MessageController(IBackgroundService backgroundService)
        {
            _backgroundService = backgroundService;
        }

        // GET: api/message/12345xyz/hello
        [HttpGet("{subscriberId}/{message}")]
        public async Task<IActionResult> Get(string subscriberId, string message)
        {
            // Start background task
            await _backgroundService.SendMessageAsync(subscriberId, message);
            return Ok();
        }
    }
}
