using FluentValidation.TestHelper;
using PartyService.Application.Queries.GetPartyById;

namespace PartyService.Application.Tests.Validators;

public class GetPartyByIdValidatorTests
{
    private readonly GetPartyByIdValidator _validator = new();

    [Fact]
    public void Validate_ValidQuery_ShouldPass()
    {
        var query = new GetPartyByIdQuery(Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldFail()
    {
        var query = new GetPartyByIdQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
