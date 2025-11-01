using MediatR;
using Microsoft.AspNetCore.Mvc;
using CeibaFunds.Application.Commands;
using CeibaFunds.Application.Queries;
using CeibaFunds.Application.DTOs;

namespace CeibaFunds.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FundsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FundsController> _logger;

    public FundsController(IMediator mediator, ILogger<FundsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FundDto>>> GetAllFunds()
    {
        try
        {
            var funds = await _mediator.Send(new GetAllFundsQuery());
            return Ok(funds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving funds");
            return StatusCode(500, new { message = "Internal server error occurred while retrieving funds" });
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<FundDto>>> GetActiveFunds()
    {
        try
        {
            var funds = await _mediator.Send(new GetActiveFundsQuery());
            return Ok(funds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active funds");
            return StatusCode(500, new { message = "Internal server error occurred while retrieving active funds" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FundDto>> GetFundById(string id)
    {
        try
        {
            var fund = await _mediator.Send(new GetFundByIdQuery(id));
            if (fund == null)
            {
                return NotFound(new { message = $"Fund with ID {id} not found" });
            }
            return Ok(fund);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fund {FundId}", id);
            return StatusCode(500, new { message = "Internal server error occurred while retrieving fund" });
        }
    }

    [HttpPost("{fundId}/subscribe")]
    public async Task<ActionResult<SubscriptionDto>> SubscribeToFund(
        string fundId,
        [FromBody] SubscribeRequest request)
    {
        try
        {
            var command = new SubscribeToFundCommand(request.CustomerId, fundId, request.Amount);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetSubscriptionById), "Subscriptions", new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid subscription request for fund {FundId}", fundId);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Subscription operation failed for fund {FundId}", fundId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing subscription for fund {FundId}", fundId);
            return StatusCode(500, new { message = "Internal server error occurred while processing subscription" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<FundDto>> CreateFund([FromBody] CreateFundRequest request)
    {
        try
        {
            var command = new CreateFundCommand(request.Name, request.Description, request.MinimumAmount, request.Category);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetFundById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fund creation request");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fund");
            return StatusCode(500, new { message = "Internal server error occurred while creating fund" });
        }
    }

    [NonAction]
    public Task<ActionResult<SubscriptionDto>> GetSubscriptionById(string id)
    {
        return Task.FromResult<ActionResult<SubscriptionDto>>(NotFound());
    }
}
public record SubscribeRequest(string CustomerId, decimal Amount);
public record CreateFundRequest(string Name, string Description, decimal MinimumAmount, int Category);