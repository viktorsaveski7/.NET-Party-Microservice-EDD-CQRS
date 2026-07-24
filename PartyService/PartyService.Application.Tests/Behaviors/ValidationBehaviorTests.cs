using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace PartyService.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Mock<IValidator<TestRequest>> _validatorMock = new();
    private readonly List<IValidator<TestRequest>> _validators;

    public ValidationBehaviorTests()
    {
        _validators = new List<IValidator<TestRequest>> { _validatorMock.Object };
    }

    [Fact]
    public async Task Handle_NoValidators_ShouldCallNext()
    {
        var behavior = new PartyService.Application.Behaviors.ValidationBehavior<TestRequest, TestResponse>(
            new List<IValidator<TestRequest>>());
        var request = new TestRequest();
        var nextCalled = false;

        var result = await behavior.Handle(request, (ct) =>
        {
            nextCalled = true;
            return Task.FromResult(new TestResponse());
        }, CancellationToken.None);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCallNext()
    {
        var behavior = new PartyService.Application.Behaviors.ValidationBehavior<TestRequest, TestResponse>(_validators);
        var request = new TestRequest();

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var nextCalled = false;
        var result = await behavior.Handle(request, (ct) =>
        {
            nextCalled = true;
            return Task.FromResult(new TestResponse());
        }, CancellationToken.None);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        var behavior = new PartyService.Application.Behaviors.ValidationBehavior<TestRequest, TestResponse>(_validators);
        var request = new TestRequest();

        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Name is required")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        await Assert.ThrowsAsync<PartyService.Application.Exceptions.ValidationException>(
            () => behavior.Handle(request, (ct) => Task.FromResult(new TestResponse()), CancellationToken.None));
    }

    public class TestRequest : IRequest<TestResponse> { }

    public class TestResponse { }
}
