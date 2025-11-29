using Enterprise.Infrastructure.Logging.Helpers;
using Enterprise.Infrastructure.Logging.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Enterprise.UnitTests.Infrastructure.Logging;

/// <summary>
/// SensitiveDataMasker unit testleri
/// </summary>
public class SensitiveDataMaskerTests
{
    private readonly ISensitiveDataMasker _masker;

    public SensitiveDataMaskerTests()
    {
        // Default options ile masker oluştur
        _masker = SensitiveDataMasker.Default;
    }

    [Fact]
    public void MaskJson_WithPassword_ShouldMask()
    {
        // Arrange
        var json = """{"username": "john", "password": "secret123"}""";

        // Act
        var result = _masker.MaskJson(json);

        // Assert
        result.Should().Contain("username");
        result.Should().Contain("john");
        result.Should().Contain("***MASKED***");
        result.Should().NotContain("secret123");
    }

    [Fact]
    public void MaskJson_WithCreditCard_ShouldMask()
    {
        // Arrange
        var json = """{"cardNumber": "4111-1111-1111-1111", "name": "John Doe"}""";

        // Act
        var result = _masker.MaskJson(json);

        // Assert
        result.Should().NotContain("4111-1111-1111");
        result.Should().Contain("****");
    }

    [Fact]
    public void MaskJson_WithNestedObject_ShouldMaskNested()
    {
        // Arrange
        var json = """{"user": {"name": "john", "credentials": {"password": "secret"}}}""";

        // Act
        var result = _masker.MaskJson(json);

        // Assert
        result.Should().Contain("***MASKED***");
        result.Should().NotContain("secret");
    }

    [Fact]
    public void SanitizeForLogging_WithNewlines_ShouldRemove()
    {
        // Arrange
        var input = "Line1\r\nLine2\nLine3\rLine4";

        // Act
        var result = _masker.SanitizeForLogging(input);

        // Assert
        result.Should().NotContain("\r");
        result.Should().NotContain("\n");
    }

    [Fact]
    public void SanitizeForLogging_WithAnsiEscape_ShouldRemove()
    {
        // Arrange
        var input = "\x1B[31mRed Text\x1B[0m";

        // Act
        var result = _masker.SanitizeForLogging(input);

        // Assert
        result.Should().NotContain("\x1B");
        result.Should().Contain("Red Text");
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    public void MaskJson_WithEmptyOrNull_ShouldReturnExpected(string? input, string? expected)
    {
        // Act
        var result = _masker.MaskJson(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void MaskJson_WithCustomOptions_ShouldUseCustomFields()
    {
        // Arrange
        var customOptions = Options.Create(new SensitiveDataOptions
        {
            SensitiveFields = new[] { "customSecret", "myPassword" }
        });
        var customMasker = new SensitiveDataMasker(customOptions);
        var json = """{"customSecret": "hidden", "normalField": "visible"}""";

        // Act
        var result = customMasker.MaskJson(json);

        // Assert
        result.Should().Contain("***MASKED***");
        result.Should().NotContain("hidden");
        result.Should().Contain("visible");
    }
}
