using MediatR;
using Microsoft.AspNetCore.Mvc;
using CeibaFunds.Application.Commands;
using CeibaFunds.Application.Queries;
using CeibaFunds.Application.DTOs;

namespace CeibaFunds.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(IMediator mediator, ILogger<CustomersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomerById(string id)
    {
        try
        {
            var customer = await _mediator.Send(new GetCustomerByIdQuery(id));
            if (customer == null)
            {
                return NotFound(new { message = $"Customer with ID {id} not found" });
            }
            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
            return StatusCode(500, new { message = "Internal server error occurred while retrieving customer" });
        }
    }

    [HttpGet("{id}/subscriptions")]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetCustomerSubscriptions(string id)
    {
        try
        {
            var subscriptions = await _mediator.Send(new GetCustomerSubscriptionsQuery(id));
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subscriptions for customer {CustomerId}", id);
            return StatusCode(500, new { message = "Internal server error occurred while retrieving subscriptions" });
        }
    }

    [HttpGet("{id}/transactions")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetCustomerTransactionHistory(
        string id,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? lastTransactionId = null)
    {
        try
        {
            var transactions = await _mediator.Send(new GetCustomerTransactionHistoryQuery(id, pageSize, lastTransactionId));
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction history for customer {CustomerId}", id);
            return StatusCode(500, new { message = "Internal server error occurred while retrieving transaction history" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        try
        {
            var command = new CreateCustomerCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.DateOfBirth);

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetCustomerById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid customer creation request");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, new { message = "Internal server error occurred while creating customer" });
        }
    }
}
public record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateTime DateOfBirth);