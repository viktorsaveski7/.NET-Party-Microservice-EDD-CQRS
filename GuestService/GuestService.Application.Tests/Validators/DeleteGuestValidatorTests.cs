using FluentValidation.TestHelper;
using GuestService.Application.Commands.DeleteGuest;

namespace GuestService.Application.Tests.Validators;

public class DeleteGuestValidatorTests
{
    private readonly DeleteGuestValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new DeleteGuestCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldFail()
    {
        var command = new DeleteGuestCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
