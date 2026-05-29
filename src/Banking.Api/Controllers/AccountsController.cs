using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Banking.Application.Accounts;
using Banking.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/accounts")]
public sealed class AccountsController(IAccountService accountService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<AccountDetailsResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Open(CancellationToken cancellationToken)
    {
        try
        {
            var response = await accountService.OpenAsync(
                new OpenAccountCommand(GetUserId(), GetRole()),
                cancellationToken);

            return Created($"/api/v1/accounts/{response.AccountId}", response);
        }
        catch (ForbiddenException exception)
        {
            return Problem(title: "Forbidden", detail: exception.Message, statusCode: StatusCodes.Status403Forbidden);
        }
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<AccountDetailsResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        var response = await accountService.GetAccountsAsync(
            new AccountsQuery(GetUserId(), GetRole()),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{accountId:guid}")]
    [ProducesResponseType<AccountDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await accountService.GetByIdAsync(
                new GetAccountByIdQuery(accountId, GetUserId(), GetRole()),
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

    [Authorize(Policy = "AdminOnly")]
    [HttpPatch("{accountId:guid}/activate")]
    [ProducesResponseType<AccountDetailsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Activate(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var response = await accountService.ActivateAsync(
            new ActivateAccountCommand(accountId, GetUserId(), GetRole()),
            cancellationToken);

        return Ok(response);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPatch("{accountId:guid}/deactivate")]
    [ProducesResponseType<AccountDetailsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Deactivate(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var response = await accountService.DeactivateAsync(
            new DeactivateAccountCommand(accountId, GetUserId(), GetRole()),
            cancellationToken);

        return Ok(response);
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
