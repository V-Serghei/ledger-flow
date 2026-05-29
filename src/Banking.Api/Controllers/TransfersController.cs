using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Banking.Application.Exceptions;
using Banking.Application.Transfers;
using Banking.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/transfers")]
public sealed class TransfersController(ITransferService transferService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<TransferResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Transfer(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        TransferRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await transferService.TransferAsync(
                new TransferMoneyCommand(
                    request.SourceAccountId,
                    request.DestinationAccountId,
                    request.Amount,
                    request.Description,
                    idempotencyKey ?? string.Empty,
                    GetUserId(),
                    GetRole()),
                cancellationToken);

            return Created($"/api/v1/transfers/{response.TransferId}", response);
        }
        catch (ArgumentException exception)
        {
            return Problem(title: "Invalid request", detail: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidMoneyAmountException exception)
        {
            return Problem(title: "Invalid amount", detail: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (ForbiddenException exception)
        {
            return Problem(title: "Forbidden", detail: exception.Message, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (NotFoundException exception)
        {
            return Problem(title: "Not found", detail: exception.Message, statusCode: StatusCodes.Status404NotFound);
        }
        catch (IdempotencyConflictException exception)
        {
            return Problem(title: "Idempotency conflict", detail: exception.Message, statusCode: StatusCodes.Status409Conflict);
        }
        catch (ConcurrencyConflictException exception)
        {
            return Problem(title: "Concurrency conflict", detail: exception.Message, statusCode: StatusCodes.Status409Conflict);
        }
        catch (InactiveAccountException exception)
        {
            return Problem(title: "Inactive account", detail: exception.Message, statusCode: StatusCodes.Status409Conflict);
        }
        catch (InsufficientFundsException exception)
        {
            return Problem(title: "Insufficient funds", detail: exception.Message, statusCode: StatusCodes.Status409Conflict);
        }
        catch (SameAccountTransferException exception)
        {
            return Problem(title: "Invalid transfer", detail: exception.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }

    [HttpGet("{transferId:guid}")]
    [ProducesResponseType<TransferResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid transferId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await transferService.GetByIdAsync(
                new GetTransferByIdQuery(transferId, GetUserId(), GetRole()),
                cancellationToken);

            return Ok(response);
        }
        catch (ForbiddenException exception)
        {
            return Problem(title: "Forbidden", detail: exception.Message, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (NotFoundException exception)
        {
            return Problem(title: "Not found", detail: exception.Message, statusCode: StatusCodes.Status404NotFound);
        }
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(sub, out var userId)
            ? userId
            : throw new InvalidOperationException("Authenticated user id claim is missing or invalid.");
    }

    private string GetRole()
    {
        return User.FindFirst("role")?.Value
            ?? User.FindFirst(ClaimTypes.Role)?.Value
            ?? throw new InvalidOperationException("Authenticated user role claim is missing.");
    }
}
