using FluentValidation;

namespace PartyService.Application.Queries.GetPartyById;

public class GetPartyByIdValidator : AbstractValidator<GetPartyByIdQuery>
{
    public GetPartyByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Party ID is required")
            .Must(id => id != Guid.Empty)
            .WithMessage("Party ID cannot be empty GUID");
    }
}