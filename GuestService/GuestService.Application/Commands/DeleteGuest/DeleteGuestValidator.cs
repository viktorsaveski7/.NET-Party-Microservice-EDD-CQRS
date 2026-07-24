using FluentValidation;

namespace GuestService.Application.Commands.DeleteGuest;

public class DeleteGuestValidator : AbstractValidator<DeleteGuestCommand>
{
    public DeleteGuestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Guest ID is required")
            .Must(id => id != Guid.Empty)
            .WithMessage("Guest ID cannot be empty GUID");
    }
}