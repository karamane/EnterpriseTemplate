using Enterprise.Business.Features.Customers.Queries.GetCustomerById;
using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Domain.Entities.Sample;
using Enterprise.Core.Shared.Exceptions;
using Enterprise.UnitTests.Base;
using FluentAssertions;
using Moq;
using Xunit;

namespace Enterprise.UnitTests.Business.Customers;

/// <summary>
/// GetCustomerByIdQueryHandler unit testleri
/// </summary>
public class GetCustomerByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IRepository<Customer, long>> _customerRepositoryMock;
    private readonly GetCustomerByIdQueryHandler _handler;

    public GetCustomerByIdQueryHandlerTests()
    {
        _customerRepositoryMock = CreateMock<IRepository<Customer, long>>();
        _handler = new GetCustomerByIdQueryHandler(_customerRepositoryMock.Object);
    }

    /// <summary>
    /// Creates a Customer for testing using the factory method
    /// </summary>
    private static Customer CreateTestCustomer(
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@example.com",
        string? phoneNumber = "+905551234567")
    {
        return Customer.Create(firstName, lastName, email, phoneNumber);
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var customerId = 1L;
        var customer = CreateTestCustomer();

        // Use reflection to set the Id since it's auto-generated
        typeof(Customer).GetProperty("Id")!.SetValue(customer, customerId);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var query = new GetCustomerByIdQuery(customerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(customerId);
        result.Data.FirstName.Should().Be("John");
        result.Data.LastName.Should().Be("Doe");
        result.Data.Email.Should().Be("john.doe@example.com");
        result.Data.PhoneNumber.Should().Be("+905551234567");
        result.Data.IsActive.Should().BeTrue();

        _customerRepositoryMock.Verify(
            x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenCustomerNotFound()
    {
        // Arrange
        var customerId = 999L;

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var query = new GetCustomerByIdQuery(customerId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .Where(e => e.Message.Contains("Customer") && e.Message.Contains("999"));

        _customerRepositoryMock.Verify(
            x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Handle_ShouldThrowNotFoundException_WhenIdIsInvalid(long invalidId)
    {
        // Arrange
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(invalidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var query = new GetCustomerByIdQuery(invalidId);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var customerId = 42L;
        var customer = CreateTestCustomer(
            firstName: "Jane",
            lastName: "Smith",
            email: "jane.smith@example.com",
            phoneNumber: "+905559876543");
        
        // Deactivate the customer for testing inactive state
        customer.Deactivate();

        typeof(Customer).GetProperty("Id")!.SetValue(customer, customerId);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var query = new GetCustomerByIdQuery(customerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(customerId);
        result.Data.FirstName.Should().Be("Jane");
        result.Data.LastName.Should().Be("Smith");
        result.Data.Email.Should().Be("jane.smith@example.com");
        result.Data.PhoneNumber.Should().Be("+905559876543");
        result.Data.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldHandleNullPhoneNumber()
    {
        // Arrange
        var customerId = 1L;
        var customer = CreateTestCustomer(phoneNumber: null);

        typeof(Customer).GetProperty("Id")!.SetValue(customer, customerId);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var query = new GetCustomerByIdQuery(customerId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PhoneNumber.Should().BeNull();
    }
}

