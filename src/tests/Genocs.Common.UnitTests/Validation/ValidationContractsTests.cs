using Genocs.Common.Validation;
using Xunit;

namespace Genocs.Common.UnitTests.Validation;

public class ValidationContractsTests
{
    [Fact]
    public void Success_ShouldBeValid_WithNoErrors()
    {
        ValidationResult result = ValidationResult.Success;

        Assert.True(result.IsValid);
        Assert.False(result.IsInvalid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_ShouldBeInvalid_WithProvidedErrors()
    {
        ValidationError[] errors =
        [
            new("Name", "validation.required", "Name is required."),
            new("Email", "validation.email", "Email is invalid.")
        ];

        ValidationResult result = ValidationResult.Failure(errors);

        Assert.True(result.IsInvalid);
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal("Name", result.Errors[0].PropertyName);
        Assert.Equal("validation.email", result.Errors[1].ErrorCode);
    }

    [Fact]
    public void Validator_ShouldExposeAsyncValidationContract()
    {
        var method = typeof(IValidator<TestModel>).GetMethod(nameof(IValidator<TestModel>.ValidateAsync));

        Assert.NotNull(method);
        Assert.Equal(typeof(Task<ValidationResult>), method!.ReturnType);

        var parameters = method.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(TestModel), parameters[0].ParameterType);
        Assert.Equal(typeof(CancellationToken), parameters[1].ParameterType);
        Assert.True(parameters[1].IsOptional);
    }

    private sealed record TestModel(string Name);
}