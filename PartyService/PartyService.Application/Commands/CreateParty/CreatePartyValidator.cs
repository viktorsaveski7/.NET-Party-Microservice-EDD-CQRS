using FluentValidation;

namespace PartyService.Application.Commands.CreateParty;

public class CreatePartyValidator : AbstractValidator<CreatePartyCommand>
{
    public CreatePartyValidator()
    {
        RuleFor(x => x.BirthdayChildName)
            .NotEmpty()
            .WithMessage("Birthday child name is required")
            .MaximumLength(100)
            .WithMessage("Birthday child name cannot exceed 100 characters");

        RuleFor(x => x.Title)
            .MaximumLength(100)
            .WithMessage("Title cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.BirthdayChildPhotoUrl)
            .MaximumLength(500)
            .WithMessage("Photo URL cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.BirthdayChildPhotoUrl));
    }
}