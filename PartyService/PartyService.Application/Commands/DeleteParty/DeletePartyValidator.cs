using FluentValidation;

namespace PartyService.Application.Commands.DeleteParty;

public class DeletePartyValidator : AbstractValidator<DeletePartyCommand>
{
    public DeletePartyValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Party ID is required")
            .Must(id => id != Guid.Empty)
            .WithMessage("Party ID cannot be empty GUID");
    }
}