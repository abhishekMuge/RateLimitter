using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class ApiController : ControllerBase
{
    [HttpPost("data")]
    public async Task<IActionResult> Receive([FromBody] RequestPacket requestPacket)
    {
        if (requestPacket?.ForwardPacket == null)
            return BadRequest("Missing request packet");

        var payload = requestPacket.ForwardPacket.Payload;
        return Ok($"Request processed successfully: {payload}");
    }
}
