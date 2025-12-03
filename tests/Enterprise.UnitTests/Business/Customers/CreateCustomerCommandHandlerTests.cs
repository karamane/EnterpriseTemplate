using Enterprise.Business.Features.Customers.Commands.CreateCustomer;
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Domain.Entities.Sample;
using Enterprise.Core.Shared.Exceptions;
using Enterprise.UnitTests.Base;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Enterprise.UnitTests.Business.Customers;

/// <summary>
/// CreateCustomerCommandHandler unit testleri
/// Örnek test implementasyonu
/// </summary>
public class CreateCustomerCommandHandlerTests : TestBase
{
    private readonly Mock<IRepository<Customer, long>> _customerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICorrelationContext> _correlationContextMock;
    private readonly Mock<ILogService> _logServiceMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _customerRepositoryMock = CreateMock<IRepository<Customer, long>>();
        _unitOfWorkMock = CreateMock<IUnitOfWork>();
        _correlationContextMock = CreateMock<ICorrelationContext>();
        _logServiceMock = CreateMock<ILogService>();

        // Correlation ID setup
        _correlationContextMock.Setup(x => x.CorrelationId).Returns("test-correlation-id");

        _handler = new CreateCustomerCommandHandler(
            _customerRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _correlationContextMock.Object,
            _logServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateCustomer()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            "John",
            "Doe",
            "john.doe@example.com",
            "+905551234567");

        _customerRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _customerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer c, CancellationToken _) => c);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FirstName.Should().Be("John");
        result.Data.LastName.Should().Be("Doe");
        result.Data.Email.Should().Be("john.doe@example.com");

        _customerRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldThrowBusinessException()
    {
        // Arrange
        var command = new CreateCustomerCommand(
            "John",
            "Doe",
            "existing@example.com",
            null);

        _customerRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<Expression<Func<Customer, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*email*");

        _customerRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("", "Doe", "test@example.com")]
    [InlineData("John", "", "test@example.com")]
    [InlineData("John", "Doe", "")]
    public async Task Handle_InvalidInput_ShouldFailValidation(
        string firstName, string lastName, string email)
    {
        // Bu test validation behavior ile birlikte çalışır
        // Burada sadece command'ın validation kurallarını test ediyoruz
        var validator = new CreateCustomerCommandValidator();
        var command = new CreateCustomerCommand(firstName, lastName, email, null);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }
}

