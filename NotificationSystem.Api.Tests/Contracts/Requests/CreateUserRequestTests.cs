using System.ComponentModel.DataAnnotations;
using NotificationSystem.Api.Contracts.Requests;

namespace NotificationSystem.Api.Tests.Contracts.Requests;

public class CreateUserRequestTests
{
    private static IList<ValidationResult> Validate(CreateUserRequest request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, context, results, validateAllProperties: true);
        return results;
    }

    private static CreateUserRequest ValidRequest() =>
        new() { Name = "João Silva", Email = "joao@email.com" };

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsNoErrors()
    {
        var request = ValidRequest();

        var results = Validate(request);

        Assert.Empty(results);
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ReturnsRequiredError()
    {
        var request = ValidRequest();
        request.Name = string.Empty;

        var results = Validate(request);

        Assert.Contains(results, r => r.ErrorMessage == "Name is required.");
    }

    [Fact]
    public void Validate_WhenNameIsTooShort_ReturnsMinLengthError()
    {
        var request = ValidRequest();
        request.Name = "A";

        var results = Validate(request);

        Assert.Contains(results, r => r.ErrorMessage == "Name must be at least 2 characters.");
    }

    [Fact]
    public void Validate_WhenNameIsExactlyTwoCharacters_ReturnsNoNameErrors()
    {
        var request = ValidRequest();
        request.Name = "Ab";

        var results = Validate(request);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(CreateUserRequest.Name)));
    }

    [Fact]
    public void Validate_WhenNameExceedsMaxLength_ReturnsMaxLengthError()
    {
        var request = ValidRequest();
        request.Name = new string('A', 101);

        var results = Validate(request);

        Assert.Contains(results, r => r.ErrorMessage == "Name must not exceed 100 characters.");
    }

    [Fact]
    public void Validate_WhenNameIsExactlyMaxLength_ReturnsNoNameErrors()
    {
        var request = ValidRequest();
        request.Name = new string('A', 100);

        var results = Validate(request);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(CreateUserRequest.Name)));
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_ReturnsRequiredError()
    {
        var request = ValidRequest();
        request.Email = string.Empty;

        var results = Validate(request);

        Assert.Contains(results, r => r.ErrorMessage == "Email is required.");
    }

    [Fact]
    public void Validate_WhenEmailIsInvalid_ReturnsEmailAddressError()
    {
        var request = ValidRequest();
        request.Email = "not-an-email";

        var results = Validate(request);

        Assert.Contains(results, r => r.ErrorMessage == "Email is invalid.");
    }

    [Fact]
    public void Validate_WhenEmailExceedsMaxLength_ReturnsMaxLengthError()
    {
        var request = ValidRequest();
        request.Email = new string('a', 245) + "@email.com";

        var results = Validate(request);

        Assert.Contains(results, r => r.ErrorMessage == "Email must not exceed 254 characters.");
    }

    [Fact]
    public void Validate_WhenEmailIsExactlyMaxLength_ReturnsNoEmailErrors()
    {
        var request = ValidRequest();
        request.Email = new string('a', 249) + "@b.co";

        var results = Validate(request);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(CreateUserRequest.Email)));
    }

    [Fact]
    public void Validate_WhenMultipleFieldsAreInvalid_ReturnsAllErrors()
    {
        var request = new CreateUserRequest { Name = "A", Email = "invalid" };

        var results = Validate(request);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.ErrorMessage == "Name must be at least 2 characters.");
        Assert.Contains(results, r => r.ErrorMessage == "Email is invalid.");
    }
}
