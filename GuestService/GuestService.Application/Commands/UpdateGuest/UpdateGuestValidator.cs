using FluentValidation;

namespace GuestService.Application.Commands.UpdateGuest;

public class UpdateGuestValidator : AbstractValidator<UpdateGuestCommand>
{
    public UpdateGuestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Guest ID is required")
            .Must(id => id != Guid.Empty)
            .WithMessage("Guest ID cannot be empty GUID");

        RuleFor(x => x.PartyId)
            .NotEmpty()
            .WithMessage("Party ID is required")
            .Must(id => id != Guid.Empty)
            .WithMessage("Party ID cannot be empty GUID");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required")
            .MaximumLength(200)
            .WithMessage("Full name cannot exceed 200 characters");

        RuleFor(x => x.Age)
            .GreaterThan(0)
            .WithMessage("Age must be greater than 0")
            .LessThan(150)
            .WithMessage("Age must be less than 150")
            .When(x => x.Age.HasValue);
    }
}