using DashboardService.Application.Abstractions;
using DashboardService.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashboardService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DashboardItemResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateDashboardItemRequest request, CancellationToken cancellationToken)
    {
        var createdItem = await _dashboardService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdItem.Id }, createdItem);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<DashboardItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatest([FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var normalizedTake = Math.Clamp(take, 1, 500);
        var items = await _dashboardService.GetLatestAsync(normalizedTake, cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DashboardItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _dashboardService.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }
}


