using LoggerService.Application.Abstractions;
using LoggerService.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace LoggerService.Api.Controllers;

[ApiController]
[Route("api/logs")]
public class LogsController : ControllerBase
{
    private readonly ILoggingService _loggingService;

    public LogsController(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(LogEntryResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateLogEntryRequest request, CancellationToken cancellationToken)
    {
        var createdLog = await _loggingService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdLog.Id }, createdLog);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<LogEntryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatest([FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var normalizedTake = Math.Clamp(take, 1, 500);
        var logs = await _loggingService.GetLatestAsync(normalizedTake, cancellationToken);

        return Ok(logs);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LogEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var log = await _loggingService.GetByIdAsync(id, cancellationToken);
        if (log is null)
        {
            return NotFound();
        }

        return Ok(log);
    }
}
