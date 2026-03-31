using FluentValidation;
using System;

namespace PubinnoApi.Features.Pours;

public class CreatePourValidator : AbstractValidator<CreatePourRequest>
{
    public CreatePourValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("Invalid EventId.");

        RuleFor(x => x.ProductId)
            .Must(p => Constants.ProductIds.Contains(p))
            .WithMessage("Invalid ProductId.");

        RuleFor(x => x.LocationId)
            .Must(l => Constants.LocationIds.Contains(l))
            .WithMessage("Invalid LocationId.");

        RuleFor(x => x.VolumeMl)
            .Must(v => Constants.Volumes.Contains(v))
            .WithMessage("Invalid VolumeMl.");

        RuleFor(x => x.EndedAt)
            .GreaterThanOrEqualTo(x => x.StartedAt)
            .WithMessage("EndedAt cannot be earlier than StartedAt.");
    }
}
