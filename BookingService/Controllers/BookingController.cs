using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers;

[ApiController]
[Route("booking")]
public class BookingController : ControllerBase
{
    private readonly Services.BookingProcessor _service;

    public BookingController(Services.BookingProcessor service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Process(
        [FromHeader(Name = "X-Correlation-Id")] string correlationId,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        [FromQuery] string processId,
        [FromQuery] string action)
    {
        await _service.HandleEvent(processId, idempotencyKey, correlationId, action);

        return Ok();
    }
}