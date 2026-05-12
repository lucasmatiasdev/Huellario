using application.implementations;
using application.dtos.Address;
using domain.entities;
using domain.enums;
using domain.interfaces;
using Moq;
using Shouldly;

namespace tests;

public class AddressServiceTests
{
    private readonly Mock<IAddressRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AddressService _sut;

    public AddressServiceTests()
    {
        _repositoryMock = new Mock<IAddressRepository>();
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Address>())).Returns(Task.CompletedTask);
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Addresses).Returns(_repositoryMock.Object);
        _sut = new AddressService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnAddresses_WhenUserHasAddresses()
    {
        var addresses = new List<Address>
        {
            new() { Id = 1, UserId = 1, Street = "Av Siempre Viva", Number = "123", City = "Springfield", ZipCode = "12345" },
            new() { Id = 2, UserId = 1, Street = "Av Principal", Number = "456", City = "Springfield", ZipCode = "12345" }
        };
        _repositoryMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(addresses);

        var result = await _sut.GetByUserIdAsync(1);

        result.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoAddresses()
    {
        _repositoryMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(new List<Address>());

        var result = await _sut.GetByUserIdAsync(1);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAddress_WhenAddressBelongsToUser()
    {
        var address = new Address { Id = 1, UserId = 1, Street = "Av Siempre Viva", Number = "123", City = "Springfield", ZipCode = "12345" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);

        var result = await _sut.GetByIdAsync(1, 1);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowKeyNotFoundException_WhenAddressDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Address?)null);

        var act = () => _sut.GetByIdAsync(1, 999);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowKeyNotFoundException_WhenAddressBelongsToDifferentUser()
    {
        var address = new Address { Id = 1, UserId = 2, Street = "Av Siempre Viva", Number = "123", City = "Springfield", ZipCode = "12345" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);

        var act = () => _sut.GetByIdAsync(1, 1);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AddAsync_ShouldCreateAddress_WhenUserHasLessThanFive()
    {
        var dto = new CreateAddressDto { Street = "Av Siempre Viva", Number = "123", City = "Springfield", ZipCode = "12345", IsDefault = false, Type = AddressType.Shipping };
        _repositoryMock.Setup(r => r.GetCountByUserIdAsync(1)).ReturnsAsync(2);
        Address? captured = null;
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Address>()))
            .Callback<Address>(a => captured = a);

        var result = await _sut.AddAsync(1, dto);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Address>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        result.ShouldNotBeNull();
        result.Street.ShouldBe("Av Siempre Viva");
    }

    [Fact]
    public async Task AddAsync_ShouldThrowInvalidOperationException_WhenUserHasFiveAddresses()
    {
        var dto = new CreateAddressDto { Street = "Av Siempre Viva", Number = "123", City = "Springfield", ZipCode = "12345", Type = AddressType.Shipping };
        _repositoryMock.Setup(r => r.GetCountByUserIdAsync(1)).ReturnsAsync(5);

        var act = () => _sut.AddAsync(1, dto);

        var ex = await act.ShouldThrowAsync<InvalidOperationException>();
        ex.Message.ShouldContain("5");
    }

    [Fact]
    public async Task AddAsync_ShouldUnsetPreviousDefault_WhenNewAddressIsDefault()
    {
        var dto = new CreateAddressDto { Street = "Av Siempre Viva", Number = "123", City = "Springfield", ZipCode = "12345", IsDefault = true, Type = AddressType.Shipping };
        var currentDefault = new Address { Id = 1, UserId = 1, Street = "Default", Number = "0", City = "City", ZipCode = "00000", IsDefault = true, Type = AddressType.Shipping };
        _repositoryMock.Setup(r => r.GetCountByUserIdAsync(1)).ReturnsAsync(2);
        _repositoryMock.Setup(r => r.GetDefaultByUserIdAsync(1, AddressType.Shipping)).ReturnsAsync(currentDefault);

        await _sut.AddAsync(1, dto);

        currentDefault.IsDefault.ShouldBeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(currentDefault), Times.Once);
    }

    [Fact]
    public async Task AddAsync_ShouldNotUnsetPreviousDefault_WhenNewAddressIsNotDefault()
    {
        var dto = new CreateAddressDto { Street = "Av Siempre Viva", Number = "123", City = "Springfield", ZipCode = "12345", IsDefault = false, Type = AddressType.Shipping };
        _repositoryMock.Setup(r => r.GetCountByUserIdAsync(1)).ReturnsAsync(2);

        await _sut.AddAsync(1, dto);

        _repositoryMock.Verify(r => r.GetDefaultByUserIdAsync(It.IsAny<int>(), It.IsAny<AddressType>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingAddress()
    {
        var existing = new Address { Id = 1, UserId = 1, Street = "Old Street", Number = "123", City = "Springfield", ZipCode = "12345", IsDefault = false, Type = AddressType.Shipping };
        var dto = new UpdateAddressDto { Street = "New Street", Number = "456", City = "Shelbyville", ZipCode = "54321", IsDefault = false, Type = AddressType.Billing };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        await _sut.UpdateAsync(1, 1, dto);

        existing.Street.ShouldBe("New Street");
        existing.City.ShouldBe("Shelbyville");
        existing.Type.ShouldBe(AddressType.Billing);
        _repositoryMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenAddressDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Address?)null);

        var act = () => _sut.UpdateAsync(1, 999, new UpdateAddressDto { Street = "x", Number = "x", City = "x", ZipCode = "x", Type = AddressType.Shipping });

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenAddressBelongsToDifferentUser()
    {
        var address = new Address { Id = 1, UserId = 2, Street = "Street", Number = "123", City = "City", ZipCode = "12345" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);

        var act = () => _sut.UpdateAsync(1, 1, new UpdateAddressDto { Street = "x", Number = "x", City = "x", ZipCode = "x", Type = AddressType.Shipping });

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUnsetPreviousDefault_WhenSettingAsDefault()
    {
        var existing = new Address { Id = 2, UserId = 1, Street = "Street", Number = "123", City = "City", ZipCode = "12345", IsDefault = false, Type = AddressType.Shipping };
        var currentDefault = new Address { Id = 1, UserId = 1, Street = "Default", Number = "456", City = "City", ZipCode = "12345", IsDefault = true, Type = AddressType.Shipping };
        var dto = new UpdateAddressDto { Street = "Street", Number = "123", City = "City", ZipCode = "12345", IsDefault = true, Type = AddressType.Shipping };
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.GetDefaultByUserIdAsync(1, AddressType.Shipping)).ReturnsAsync(currentDefault);

        await _sut.UpdateAsync(1, 2, dto);

        currentDefault.IsDefault.ShouldBeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(currentDefault), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteAddress()
    {
        var address = new Address { Id = 1, UserId = 1, Street = "Street", Number = "123", City = "City", ZipCode = "12345" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);

        await _sut.DeleteAsync(1, 1);

        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenAddressDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Address?)null);

        var act = () => _sut.DeleteAsync(1, 999);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenAddressBelongsToDifferentUser()
    {
        var address = new Address { Id = 1, UserId = 2, Street = "Street", Number = "123", City = "City", ZipCode = "12345" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);

        var act = () => _sut.DeleteAsync(1, 1);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SetDefaultAsync_ShouldSetAddressAsDefault()
    {
        var address = new Address { Id = 2, UserId = 1, Street = "Street", Number = "123", City = "City", ZipCode = "12345", IsDefault = false };
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(address);

        var result = await _sut.SetDefaultAsync(1, 2);

        result.IsDefault.ShouldBeTrue();
        _repositoryMock.Verify(r => r.UpdateAsync(address), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task SetDefaultAsync_ShouldUnsetPreviousDefault()
    {
        var address = new Address { Id = 2, UserId = 1, Street = "Street", Number = "123", City = "City", ZipCode = "12345", IsDefault = false, Type = AddressType.Shipping };
        var currentDefault = new Address { Id = 1, UserId = 1, Street = "Default", Number = "456", City = "City", ZipCode = "12345", IsDefault = true, Type = AddressType.Shipping };
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(address);
        _repositoryMock.Setup(r => r.GetDefaultByUserIdAsync(1, AddressType.Shipping)).ReturnsAsync(currentDefault);

        await _sut.SetDefaultAsync(1, 2);

        currentDefault.IsDefault.ShouldBeFalse();
    }

    [Fact]
    public async Task SetDefaultAsync_ShouldNotUnset_WhenSameAddressIsAlreadyDefault()
    {
        var address = new Address { Id = 1, UserId = 1, Street = "Street", Number = "123", City = "City", ZipCode = "12345", IsDefault = true, Type = AddressType.Shipping };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);
        _repositoryMock.Setup(r => r.GetDefaultByUserIdAsync(1, AddressType.Shipping)).ReturnsAsync(address);

        await _sut.SetDefaultAsync(1, 1);

        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Address>(a => a.Id != 1)), Times.Never);
    }

    [Fact]
    public async Task SetDefaultAsync_ShouldThrowKeyNotFoundException_WhenAddressDoesNotExist()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Address?)null);

        var act = () => _sut.SetDefaultAsync(1, 999);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SetDefaultAsync_ShouldThrowKeyNotFoundException_WhenAddressBelongsToDifferentUser()
    {
        var address = new Address { Id = 1, UserId = 2, Street = "Street", Number = "123", City = "City", ZipCode = "12345" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(address);

        var act = () => _sut.SetDefaultAsync(1, 1);

        await act.ShouldThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AddAsync_ShouldNotUnsetBillingDefault_WhenAddingShippingDefault()
    {
        var shippingDefault = new Address { Id = 1, UserId = 1, Street = "Shipping", Number = "1", City = "City", ZipCode = "12345", IsDefault = true, Type = AddressType.Shipping };
        var billingDefault = new Address { Id = 2, UserId = 1, Street = "Billing", Number = "2", City = "City", ZipCode = "12345", IsDefault = true, Type = AddressType.Billing };
        var dto = new CreateAddressDto { Street = "New Shipping", Number = "3", City = "City", ZipCode = "12345", IsDefault = true, Type = AddressType.Shipping };
        _repositoryMock.Setup(r => r.GetCountByUserIdAsync(1)).ReturnsAsync(2);
        _repositoryMock.Setup(r => r.GetDefaultByUserIdAsync(1, AddressType.Shipping)).ReturnsAsync(shippingDefault);

        await _sut.AddAsync(1, dto);

        shippingDefault.IsDefault.ShouldBeFalse();
        billingDefault.IsDefault.ShouldBeTrue();
        _repositoryMock.Verify(r => r.GetDefaultByUserIdAsync(1, It.Is<AddressType>(t => t == AddressType.Shipping)), Times.Once);
        _repositoryMock.Verify(r => r.GetDefaultByUserIdAsync(1, It.Is<AddressType>(t => t == AddressType.Billing)), Times.Never);
    }

    [Fact]
    public async Task SetDefaultAsync_ShouldOnlyUnsetDefaultOfSameType()
    {
        var address = new Address { Id = 3, UserId = 1, Street = "New", Number = "3", City = "City", ZipCode = "12345", IsDefault = false, Type = AddressType.Billing };
        var currentBillingDefault = new Address { Id = 2, UserId = 1, Street = "Billing", Number = "2", City = "City", ZipCode = "12345", IsDefault = true, Type = AddressType.Billing };
        var shippingDefault = new Address { Id = 1, UserId = 1, Street = "Shipping", Number = "1", City = "City", ZipCode = "12345", IsDefault = true, Type = AddressType.Shipping };
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(address);
        _repositoryMock.Setup(r => r.GetDefaultByUserIdAsync(1, AddressType.Billing)).ReturnsAsync(currentBillingDefault);

        await _sut.SetDefaultAsync(1, 3);

        currentBillingDefault.IsDefault.ShouldBeFalse();
        shippingDefault.IsDefault.ShouldBeTrue();
        _repositoryMock.Verify(r => r.GetDefaultByUserIdAsync(1, It.Is<AddressType>(t => t == AddressType.Billing)), Times.Once());
        _repositoryMock.Verify(r => r.GetDefaultByUserIdAsync(1, It.Is<AddressType>(t => t == AddressType.Shipping)), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUnsetDefaultOfNewType_WhenTypeChangedAndSetAsDefault()
    {
        var existing = new Address { Id = 2, UserId = 1, Street = "Street", Number = "123", City = "City", ZipCode = "12345", IsDefault = false, Type = AddressType.Shipping };
        var billingDefault = new Address { Id = 1, UserId = 1, Street = "Billing Default", Number = "456", City = "City", ZipCode = "12345", IsDefault = true, Type = AddressType.Billing };
        var dto = new UpdateAddressDto { Street = "Street", Number = "123", City = "City", ZipCode = "12345", IsDefault = true, Type = AddressType.Billing };
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(existing);
        _repositoryMock.Setup(r => r.GetDefaultByUserIdAsync(1, AddressType.Billing)).ReturnsAsync(billingDefault);

        await _sut.UpdateAsync(1, 2, dto);

        existing.Type.ShouldBe(AddressType.Billing);
        existing.IsDefault.ShouldBeTrue();
        billingDefault.IsDefault.ShouldBeFalse();
        _repositoryMock.Verify(r => r.GetDefaultByUserIdAsync(1, AddressType.Billing), Times.Once);
    }
}
