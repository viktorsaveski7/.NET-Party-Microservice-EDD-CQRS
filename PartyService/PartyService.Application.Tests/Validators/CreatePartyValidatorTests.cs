using FluentValidation.TestHelper;
using PartyService.Application.Commands.CreateParty;

namespace PartyService.Application.Tests.Validators;

public class CreatePartyValidatorTests
{
    private readonly CreatePartyValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new CreatePartyCommand("Alice", "Birthday Party", null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyBirthdayChildName_ShouldFail()
    {
        var command = new CreatePartyCommand("", null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BirthdayChildName);
    }

    [Fact]
    public void Validate_BirthdayChildNameTooLong_ShouldFail()
    {
        var command = new CreatePartyCommand(new string('A', 101), null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BirthdayChildName);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(50)]
    [InlineData(1)]
    public void Validate_BirthdayChildNameWithinLimit_ShouldPass(int length)
    {
        var command = new CreatePartyCommand(new string('A', length), null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.BirthdayChildName);
    }

    [Fact]
    public void Validate_TitleTooLong_ShouldFail()
    {
        var command = new CreatePartyCommand("Alice", new string('B', 101), null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_PhotoUrlTooLong_ShouldFail()
    {
        var command = new CreatePartyCommand("Alice", null, new string('C', 501));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BirthdayChildPhotoUrl);
    }

    [Fact]
    public void Validate_OnlyRequiredFields_ShouldPass()
    {
        var command = new CreatePartyCommand("Alice", null, null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
