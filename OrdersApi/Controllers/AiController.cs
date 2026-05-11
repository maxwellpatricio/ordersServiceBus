using Microsoft.AspNetCore.Mvc;
using Orders.Api.Services;

namespace Orders.Api.Controllers;

[ApiController]
[Route("ai")]
public class AiController(PerplexityService ai) : ControllerBase
{
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AiAskRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Question))
            return BadRequest(new { error = "A pergunta não pode ser vazia." });

        var response = await ai.AskAsync(req.Question);
        return Ok(response);
    }
}
