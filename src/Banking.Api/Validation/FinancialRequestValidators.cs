using Banking.Api.Controllers;
using FluentValidation;

namespace Banking.Api.Validation;

public sealed class DepositRequestValidator : AbstractValidator<DepositRequest>
{
    public DepositRequestValidator()
    {
        RuleFor(request => request.AccountId)
            .NotEmpty();

        RuleFor(request => request.Amount)
            .GreaterThan(0);

        RuleFor(request => request.Description)
            .MaximumLength(512);
    }
}

public sealed class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(request => request.SourceAccountId)
            .NotEmpty();

        RuleFor(request => request.DestinationAccountId)
            .NotEmpty()
            .NotEqual(request => request.SourceAccountId);

        RuleFor(request => request.Amount)
            .GreaterThan(0);

        RuleFor(request => request.Description)
            .MaximumLength(512);
    }
}

public sealed class CreatePixKeyRequestValidator : AbstractValidator<CreatePixKeyRequest>
{
    private static readonly string[] SupportedTypes = ["Email", "FakeCpf", "Phone", "Random"];

    public CreatePixKeyRequestValidator()
    {
        RuleFor(request => request.AccountId)
            .NotEmpty();

        RuleFor(request => request.Type)
            .NotEmpty()
            .Must(type => SupportedTypes.Contains(type))
            .WithMessage("PIX key type must be Email, FakeCpf, Phone, or Random.");

        RuleFor(request => request.Key)
            .NotEmpty()
            .MaximumLength(256);
    }
}

public sealed class PayPixRequestValidator : AbstractValidator<PayPixRequest>
{
    public PayPixRequestValidator()
    {
        RuleFor(request => request.SourceAccountId)
            .NotEmpty();

        RuleFor(request => request.DestinationKey)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(request => request.Amount)
            .GreaterThan(0);

        RuleFor(request => request.Description)
            .MaximumLength(512);
    }
}
