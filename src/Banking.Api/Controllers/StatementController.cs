using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Banking.Application.Exceptions;
using Banking.Application.Statements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/accounts/{accountId:guid}/statement")]
public sealed class StatementController(IStatementService statementService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedStatementResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatement(
        Guid accountId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await statementService.GetStatementAsync(
                new GetStatementQuery(accountId, from, to, page, pageSize, GetUserId(), GetRole()),
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
