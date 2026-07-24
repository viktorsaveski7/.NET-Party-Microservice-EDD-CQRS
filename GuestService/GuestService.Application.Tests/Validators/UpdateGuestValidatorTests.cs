using FluentValidation.TestHelper;
using GuestService.Application.Commands.UpdateGuest;

namespace GuestService.Application.Tests.Validators;

public class UpdateGuestValidatorTests
{
    private readonly UpdateGuestValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new UpdateGuestCommand(Guid.NewGuid(), Guid.NewGuid(), "John Doe", 10);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyGuestId_ShouldFail()
    {
        var command = new UpdateGuestCommand(Guid.Empty, Guid.NewGuid(), "John Doe", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_EmptyPartyId_ShouldFail()
    {
        var command = new UpdateGuestCommand(Guid.NewGuid(), Guid.Empty, "John Doe", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PartyId);
    }

    [Fact]
    public void Validate_EmptyFullName_ShouldFail()
    {
        var command = new UpdateGuestCommand(Guid.NewGuid(), Guid.NewGuid(), "", null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_FullNameTooLong_ShouldFail()
    {
        var command = new UpdateGuestCommand(Guid.NewGuid(), Guid.NewGuid(), new string('A', 201), null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }
}
