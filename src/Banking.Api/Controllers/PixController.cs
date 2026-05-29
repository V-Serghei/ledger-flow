using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Banking.Application.Exceptions;
using Banking.Application.Pix;
using Banking.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/pix")]
public sealed class PixController(IPixService pixService) : ControllerBase
{
    [HttpPost("keys")]
    [ProducesResponseType<PixKeyResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateKey(
        CreatePixKeyRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await pixService.CreateKeyAsync(
                new CreatePixKeyCommand(request.AccountId, request.Type, request.Key, GetUserId(), GetRole()),
                cancellationToken);

            return Created($"/api/v1/pix/keys/{response.Id}", response);
        }
        catch (ArgumentException exception)
        {
            return Problem(title: "Invalid request", detail: exception.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (ForbiddenException exception)
        {
            return Problem(title: "Forbidden", detail: exception.Message, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (NotFoundException exception)
        {
            return Problem(title: "Not found", detail: exception.Message, statusCode: StatusCodes.Status404NotFound);
        }
        catch (ConflictException exception)
        {
            return Problem(title: "Conflict", detail: exception.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }

    [HttpGet("keys")]
    [ProducesResponseType<IReadOnlyCollection<PixKeyResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKeys(CancellationToken cancellationToken)
    {
        var response = await pixService.GetKeysAsync(
            new GetPixKeysQuery(GetUserId(), GetRole()),
            cancellationToken);

        return Ok(response);
    }

    [HttpPost("payments")]
    [ProducesResponseType<PixPaymentResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Pay(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        PayPixRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await pixService.PayAsync(
                new PayPixCommand(
                    request.SourceAccountId,
                    request.DestinationKey,
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
