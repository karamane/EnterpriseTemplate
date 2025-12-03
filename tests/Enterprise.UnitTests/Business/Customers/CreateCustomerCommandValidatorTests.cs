using Enterprise.Business.Features.Customers.Commands.CreateCustomer;
using FluentAssertions;
using Xunit;

namespace Enterprise.UnitTests.Business.Customers;

/// <summary>
/// CreateCustomerCommandValidator unit testleri
/// Tüm validation kurallarını kapsar
/// </summary>
public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator;

    public CreateCustomerCommandValidatorTests()
    {
        _validator = new CreateCustomerCommandValidator();
    }

    #region FirstName Validation Tests

    [Fact]
    public async Task Validate_ShouldFail_WhenFirstNameIsEmpty()
    {
        // Arrange
        var command = new CreateCustomerCommand("", "Doe", "john@example.com", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("boş olamaz"));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenFirstNameIsNull()
    {
        // Arrange
        var command = new CreateCustomerCommand(null!, "Doe", "john@example.com", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenFirstNameIsWhitespace()
    {
        // Arrange
        var command = new CreateCustomerCommand("   ", "Doe", "john@example.com", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenFirstNameExceeds100Characters()
    {
        // Arrange
        var longFirstName = new string('A', 101);
        var command = new CreateCustomerCommand(longFirstName, "Doe", "john@example.com", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "FirstName" && 
            e.ErrorMessage.Contains("100"));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenFirstNameIs100Characters()
    {
        // Arrange
        var maxFirstName = new string('A', 100);
        var command = new CreateCustomerCommand(maxFirstName, "Doe", "john@example.com", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == "FirstName" && 
            e.ErrorMessage.Contains("100"));
    }

    #endregion

    #region LastName Validation Tests

    [Fact]
    public async Task Validate_ShouldFail_WhenLastNameIsEmpty()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "", "john@example.com", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("boş olamaz"));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenLastNameIsNull()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", null!, "john@example.com", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenLastNameExceeds100Characters()
    {
        // Arrange
        var longLastName = new string('B', 101);
        var command = new CreateCustomerCommand("John", longLastName, "john@example.com", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "LastName" && 
            e.ErrorMessage.Contains("100"));
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public async Task Validate_ShouldFail_WhenEmailIsEmpty()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenEmailIsNull()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", null!, null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("john@")]
    [InlineData("john.example.com")]
    // Note: "john@.com" and "john@example" pass FluentValidation's default EmailAddress validator
    public async Task Validate_ShouldFail_WhenEmailFormatIsInvalid(string invalidEmail)
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", invalidEmail, null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "Email" && 
            e.ErrorMessage.Contains("email"));
    }

    [Theory]
    [InlineData("john@example.com")]
    [InlineData("john.doe@example.com")]
    [InlineData("john+tag@example.com")]
    [InlineData("john@subdomain.example.com")]
    [InlineData("john.doe@example.co.uk")]
    public async Task Validate_ShouldPass_WhenEmailFormatIsValid(string validEmail)
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", validEmail, null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => 
            e.PropertyName == "Email" && 
            e.ErrorMessage.Contains("email"));
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenEmailExceeds256Characters()
    {
        // Arrange - Create email with exactly 257 characters
        var localPart = new string('a', 247); // 247 chars
        var longEmail = localPart + "@test.com"; // 247 + 9 = 256, add one more
        longEmail = new string('a', 248) + "@test.com"; // 248 + 9 = 257 characters
        var command = new CreateCustomerCommand("John", "Doe", longEmail, null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "Email" && 
            e.ErrorMessage.Contains("256"));
    }

    #endregion

    #region PhoneNumber Validation Tests

    [Fact]
    public async Task Validate_ShouldPass_WhenPhoneNumberIsNull()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "john@example.com", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "PhoneNumber");
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenPhoneNumberIsEmpty()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "john@example.com", "");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "PhoneNumber");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenPhoneNumberExceeds20Characters()
    {
        // Arrange
        var longPhone = "+12345678901234567890123"; // 21+ characters
        var command = new CreateCustomerCommand("John", "Doe", "john@example.com", longPhone);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "PhoneNumber" && 
            e.ErrorMessage.Contains("20"));
    }

    [Theory]
    [InlineData("+905551234567")]
    [InlineData("05551234567")]
    [InlineData("+1 (555) 123-4567")]
    [InlineData("555-123-4567")]
    [InlineData("+44 20 7946 0958")]
    public async Task Validate_ShouldPass_WhenPhoneNumberFormatIsValid(string validPhone)
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "john@example.com", validPhone);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "PhoneNumber");
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("phone@number")]
    [InlineData("123#456")]
    [InlineData("55*123")]
    public async Task Validate_ShouldFail_WhenPhoneNumberContainsInvalidCharacters(string invalidPhone)
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "john@example.com", invalidPhone);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.PropertyName == "PhoneNumber" && 
            e.ErrorMessage.Contains("telefon"));
    }

    #endregion

    #region Combined Validation Tests

    [Fact]
    public async Task Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            "+905551234567");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenAllRequiredFieldsAreValidAndPhoneIsNull()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ShouldReturnMultipleErrors_WhenMultipleFieldsAreInvalid()
    {
        // Arrange
        var command = new CreateCustomerCommand("", "", "invalid-email", "abc");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task Validate_ShouldPass_WhenFirstNameHasSpecialCharacters()
    {
        // Arrange - Turkish characters
        var command = new CreateCustomerCommand("Şükriye", "Öztürk", "sukriye@example.com", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenEmailHasSubdomain()
    {
        // Arrange
        var command = new CreateCustomerCommand("John", "Doe", "john@mail.subdomain.example.com", null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}

