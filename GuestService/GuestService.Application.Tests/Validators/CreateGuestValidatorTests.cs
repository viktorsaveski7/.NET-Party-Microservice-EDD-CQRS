using FluentValidation.TestHelper;
using GuestService.Application.Commands.CreateGuest;

namespace GuestService.Application.Tests.Validators;

public class CreateGuestValidatorTests
{
    private readonly CreateGuestValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new CreateGuestCommand(Guid.NewGuid(), "John Doe", 10);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidCommandWithoutAge_ShouldPass()
    {
        var command = new CreateGuestCommand(Guid.NewGuid(), "John Doe", null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyPartyId_ShouldFail()
    {
        var command = new CreateGuestCommand(Guid.Empty, "John Doe", 10);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PartyId);
    }

    [Fact]
    public void Validate_EmptyFullName_ShouldFail()
    {
        var command = new CreateGuestCommand(Guid.NewGuid(), "", 10);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_FullNameTooLong_ShouldFail()
    {
        var command = new CreateGuestCommand(Guid.NewGuid(), new string('A', 201), 10);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_AgeZeroOrNegative_ShouldFail(int age)
    {
        var command = new CreateGuestCommand(Guid.NewGuid(), "John Doe", age);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Age);
    }

    [Fact]
    public void Validate_AgeTooHigh_ShouldFail()
    {
        var command = new CreateGuestCommand(Guid.NewGuid(), "John Doe", 150);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Age);
    }
}
