using FluentValidation.TestHelper;
using PartyService.Application.Commands.UpdateParty;

namespace PartyService.Application.Tests.Validators;

public class UpdatePartyValidatorTests
{
    private readonly UpdatePartyValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new UpdatePartyCommand(Guid.NewGuid(), "Alice", "Updated Party", null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldFail()
    {
        var command = new UpdatePartyCommand(Guid.Empty, "Alice", null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_EmptyBirthdayChildName_ShouldFail()
    {
        var command = new UpdatePartyCommand(Guid.NewGuid(), "", null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BirthdayChildName);
    }

    [Fact]
    public void Validate_BirthdayChildNameTooLong_ShouldFail()
    {
        var command = new UpdatePartyCommand(Guid.NewGuid(), new string('A', 101), null, null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BirthdayChildName);
    }
}
