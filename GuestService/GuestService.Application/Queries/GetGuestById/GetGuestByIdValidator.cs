using FluentValidation;

namespace GuestService.Application.Queries.GetGuestById;

public class GetGuestByIdValidator : AbstractValidator<GetGuestByIdQuery>
{
    public GetGuestByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Guest ID is required")
            .Must(id => id != Guid.Empty)
            .WithMessage("Guest ID cannot be empty GUID");
    }
}