using MediatR;
using Microsoft.AspNetCore.Mvc;
using CeibaFunds.Application.Commands;
using CeibaFunds.Application.Queries;
using CeibaFunds.Application.DTOs;

namespace CeibaFunds.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(IMediator mediator, ILogger<SubscriptionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubscriptionDto>> GetSubscriptionById(string id)
    {
        try
        {
            var subscription = await _mediator.Send(new GetSubscriptionByIdQuery(id));
            if (subscription == null)
            {
                return NotFound(new { message = $"Subscription with ID {id} not found" });
            }
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscription {SubscriptionId}", id);
            return StatusCode(500, new { message = "Internal server error occurred while retrieving subscription" });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelSubscription(string id, [FromBody] CancelSubscriptionRequest request)
    {
        try
        {
            var command = new CancelSubscriptionCommand(id, request.CustomerId);
            var result = await _mediator.Send(command);

            if (result)
            {
                return Ok(new { message = "Subscription cancelled successfully", subscriptionId = id });
            }

            return BadRequest(new { message = "Failed to cancel subscription" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid cancellation request for subscription {SubscriptionId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cancellation operation failed for subscription {SubscriptionId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized cancellation attempt for subscription {SubscriptionId}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription {SubscriptionId}", id);
            return StatusCode(500, new { message = "Internal server error occurred while cancelling subscription" });
        }
    }
}
public record CancelSubscriptionRequest(string CustomerId);