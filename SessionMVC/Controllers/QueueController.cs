using Microsoft.AspNetCore.Mvc;
using Session.Services.Services.Interfaces;

namespace SessionMVC.Controllers;

[ApiController]
[Route("[controller]")]
public class QueueController(IQueueService queueService): ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var messages = await queueService.GetMessages();

        return Ok(messages);
    }

    [HttpPost("send")]
    public async Task<IActionResult> Post([FromQuery]string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return BadRequest("Message cannot be empty");
        }
        await queueService.SendMessage(message);

        return Ok();
    }
}
