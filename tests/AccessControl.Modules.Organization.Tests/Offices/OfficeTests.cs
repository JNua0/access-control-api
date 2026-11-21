using AccessControl.Modules.Organization.Domain.Offices;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Tests.Offices;

public class OfficeTests
{
    // Una dirección válida para las pruebas. Office recibe un Address ya validado,
    // así que aquí no se prueba la dirección (eso lo hace AddressTests).
    private static Address CreateAddress(string street = "Av. Insurgentes Sur")
        => Address.Create(street, "1234", null, "03100", "Benito Juárez", "Ciudad de México").Value;

    // ---------- Create ----------

    [Fact]
    public void Create_WithValidData_ReturnsActiveOffice()
    {
        // Arrange
        Address address = CreateAddress();

        // Act
        Result<Office> result = Office.Create("Matriz", address);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Matriz", result.Value.Name);
        Assert.Equal(address, result.Value.Address);
        Assert.True(result.Value.IsActive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Create_WithoutName_ReturnsNameRequired(string? name)
    {
        // Act
        Result<Office> result = Office.Create(name, CreateAddress());

        // Assert
        Assert.Equal(OfficeErrors.NameRequired, result.Error);
    }

    [Theory]
    [InlineData("  Sucursal Norte  ", "Sucursal Norte")]
    [InlineData("Sucursal     Norte", "Sucursal Norte")]
    [InlineData("Sucursal\tNorte\n", "Sucursal Norte")]
    public void Create_WithExtraSpaces_CleansName(string name, string expectedName)
    {
        // Act
        Result<Office> result = Office.Create(name, CreateAddress());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedName, result.Value.Name);
    }

    [Fact]
    public void Create_WithNameAtMaxLength_ReturnsSuccess()
    {
        // Arrange: exactamente 60 caracteres
        string name = new string('a', Office.NameMaxLength);

        // Act
        Result<Office> result = Office.Create(name, CreateAddress());

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithNameOverMaxLength_ReturnsNameTooLong()
    {
        // Arrange: 61 caracteres
        string name = new string('a', Office.NameMaxLength + 1);

        // Act
        Result<Office> result = Office.Create(name, CreateAddress());

        // Assert
        Assert.Equal(OfficeErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Create_WithSurroundingSpacesAndMaxLength_ReturnsSuccess()
    {
        // Arrange: la longitud se mide después de limpiar
        string name = "     " + new string('a', Office.NameMaxLength) + "     ";

        // Act
        Result<Office> result = Office.Create(name, CreateAddress());

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithNullAddress_ThrowsArgumentNullException()
    {
        // Act y Assert: un Address nulo es un bug de programación, no un error del usuario.
        // "null!" simula el bug: pasar un nulo callando al compilador.
        Assert.Throws<ArgumentNullException>(() => Office.Create("Matriz", null!));
    }

    [Fact]
    public void Create_WithNullAddressAndInvalidName_ThrowsArgumentNullException()
    {
        // Act y Assert: la revisión del nulo va primero; un bug no se oculta detrás de un error de validación
        Assert.Throws<ArgumentNullException>(() => Office.Create("", null!));
    }

    // ---------- Rename ----------

    [Fact]
    public void Rename_WithValidName_ChangesName()
    {
        // Arrange
        Office office = Office.Create("Matriz", CreateAddress()).Value;

        // Act
        Result result = office.Rename("  Oficina   Central  ");

        // Assert: cambió, y se guardó limpio
        Assert.True(result.IsSuccess);
        Assert.Equal("Oficina Central", office.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithoutName_ReturnsNameRequiredAndKeepsName(string? newName)
    {
        // Arrange
        Office office = Office.Create("Matriz", CreateAddress()).Value;

        // Act
        Result result = office.Rename(newName);

        // Assert: devuelve el error y el nombre no cambia
        Assert.Equal(OfficeErrors.NameRequired, result.Error);
        Assert.Equal("Matriz", office.Name);
    }

    [Fact]
    public void Rename_WithNameOverMaxLength_ReturnsNameTooLongAndKeepsName()
    {
        // Arrange
        Office office = Office.Create("Matriz", CreateAddress()).Value;

        // Act
        Result result = office.Rename(new string('a', Office.NameMaxLength + 1));

        // Assert
        Assert.Equal(OfficeErrors.NameTooLong, result.Error);
        Assert.Equal("Matriz", office.Name);
    }

    [Fact]
    public void Rename_DoesNotChangeAddressOrState()
    {
        // Arrange
        Address address = CreateAddress();
        Office office = Office.Create("Matriz", address).Value;
        office.Deactivate();

        // Act
        office.Rename("Oficina Central");

        // Assert
        Assert.Equal(address, office.Address);
        Assert.False(office.IsActive);
    }

    // ---------- ChangeAddress ----------

    [Fact]
    public void ChangeAddress_WithNewAddress_ReplacesAddress()
    {
        // Arrange
        Office office = Office.Create("Matriz", CreateAddress()).Value;
        Address newAddress = CreateAddress(street: "Paseo de la Reforma");

        // Act
        office.ChangeAddress(newAddress);

        // Assert
        Assert.Equal(newAddress, office.Address);
    }

    [Fact]
    public void ChangeAddress_DoesNotChangeNameOrState()
    {
        // Arrange
        Office office = Office.Create("Matriz", CreateAddress()).Value;
        office.Deactivate();

        // Act
        office.ChangeAddress(CreateAddress(street: "Paseo de la Reforma"));

        // Assert
        Assert.Equal("Matriz", office.Name);
        Assert.False(office.IsActive);
    }

    [Fact]
    public void ChangeAddress_WithNullAddress_ThrowsAndKeepsAddress()
    {
        // Arrange
        Address address = CreateAddress();
        Office office = Office.Create("Matriz", address).Value;

        // Act y Assert: lanza la excepción y la dirección anterior se conserva
        Assert.Throws<ArgumentNullException>(() => office.ChangeAddress(null!));
        Assert.Equal(address, office.Address);
    }

    // ---------- Activate y Deactivate ----------

    [Fact]
    public void Deactivate_ActiveOffice_SetsInactive()
    {
        // Arrange
        Office office = Office.Create("Matriz", CreateAddress()).Value;

        // Act
        office.Deactivate();

        // Assert
        Assert.False(office.IsActive);
    }

    [Fact]
    public void Activate_InactiveOffice_SetsActive()
    {
        // Arrange
        Office office = Office.Create("Matriz", CreateAddress()).Value;
        office.Deactivate();

        // Act
        office.Activate();

        // Assert
        Assert.True(office.IsActive);
    }

    [Fact]
    public void Deactivate_Twice_StaysInactive()
    {
        // Arrange
        Office office = Office.Create("Matriz", CreateAddress()).Value;

        // Act: desactivar dos veces deja el mismo resultado (idempotente)
        office.Deactivate();
        office.Deactivate();

        // Assert
        Assert.False(office.IsActive);
    }

    [Fact]
    public void Activate_AlreadyActive_StaysActive()
    {
        // Arrange
        Office office = Office.Create("Matriz", CreateAddress()).Value;

        // Act
        office.Activate();

        // Assert
        Assert.True(office.IsActive);
    }

    // ---------- Identidad ----------

    [Fact]
    public void Create_TwiceWithSameData_ReturnsDifferentOffices()
    {
        // Act
        Office first = Office.Create("Matriz", CreateAddress()).Value;
        Office second = Office.Create("Matriz", CreateAddress()).Value;

        // Assert: es una entidad, no un record; se distinguen por su Id, que asigna la base de datos
        Assert.NotEqual(first, second);
    }
}
