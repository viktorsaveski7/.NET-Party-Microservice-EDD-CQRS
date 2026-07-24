using FluentValidation.TestHelper;
using PartyService.Application.Commands.DeleteParty;

namespace PartyService.Application.Tests.Validators;

public class DeletePartyValidatorTests
{
    private readonly DeletePartyValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new DeletePartyCommand(Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldFail()
    {
        var command = new DeletePartyCommand(Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
