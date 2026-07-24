using FluentValidation.TestHelper;
using GuestService.Application.Queries.GetGuestById;

namespace GuestService.Application.Tests.Validators;

public class GetGuestByIdValidatorTests
{
    private readonly GetGuestByIdValidator _validator = new();

    [Fact]
    public void Validate_ValidQuery_ShouldPass()
    {
        var query = new GetGuestByIdQuery(Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldFail()
    {
        var query = new GetGuestByIdQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
