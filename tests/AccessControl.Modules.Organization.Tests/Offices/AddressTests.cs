using AccessControl.Modules.Organization.Domain.Offices;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Tests.Offices;

public class AddressTests
{
    // Crea una dirección válida. Cada prueba cambia solo el campo que le interesa,
    // usando argumentos con nombre: CreateAddress(street: null)
    private static Result<Address> CreateAddress(
        string? street = "Av. Insurgentes Sur",
        string? exteriorNumber = "1234",
        string? interiorNumber = null,
        string? postalCode = "03100",
        string? municipality = "Benito Juárez",
        string? state = "Ciudad de México")
        => Address.Create(street, exteriorNumber, interiorNumber, postalCode, municipality, state);

    // ---------- Casos válidos ----------

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Act
        Result<Address> result = CreateAddress(interiorNumber: "Piso 3");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Av. Insurgentes Sur", result.Value.Street);
        Assert.Equal("1234", result.Value.ExteriorNumber);
        Assert.Equal("Piso 3", result.Value.InteriorNumber);
        Assert.Equal("03100", result.Value.PostalCode);
        Assert.Equal("Benito Juárez", result.Value.Municipality);
        Assert.Equal("Ciudad de México", result.Value.State);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutInteriorNumber_SetsInteriorNumberToNull(string? interiorNumber)
    {
        // Act
        Result<Address> result = CreateAddress(interiorNumber: interiorNumber);

        // Assert: el número interior es opcional
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.InteriorNumber);
    }

    [Theory]
    [InlineData("S/N")]    // Sin número
    [InlineData("12-B")]
    [InlineData("Km 5.5")]
    public void Create_WithExteriorNumberWithLettersAndSymbols_ReturnsSuccess(string exteriorNumber)
    {
        // Act: el número exterior no tiene regla de caracteres
        Result<Address> result = CreateAddress(exteriorNumber: exteriorNumber);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(exteriorNumber, result.Value.ExteriorNumber);
    }

    // ---------- Limpieza ----------

    [Fact]
    public void Create_WithExtraSpaces_CleansText()
    {
        // Act
        Result<Address> result = CreateAddress(
            street: "  Av.    Insurgentes   Sur  ",
            interiorNumber: "  Depto    4  ",
            municipality: "Benito\tJuárez",
            state: "  Ciudad   de México ");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Av. Insurgentes Sur", result.Value.Street);
        Assert.Equal("Depto 4", result.Value.InteriorNumber);
        Assert.Equal("Benito Juárez", result.Value.Municipality);
        Assert.Equal("Ciudad de México", result.Value.State);
    }

    [Theory]
    [InlineData(" 03100 ")]
    [InlineData("03 100")]
    public void Create_WithSpacesInPostalCode_RemovesThem(string postalCode)
    {
        // Act
        Result<Address> result = CreateAddress(postalCode: postalCode);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("03100", result.Value.PostalCode);
    }

    // ---------- Campos obligatorios ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutStreet_ReturnsStreetRequired(string? street)
    {
        Result<Address> result = CreateAddress(street: street);

        Assert.Equal(AddressErrors.StreetRequired, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutExteriorNumber_ReturnsExteriorNumberRequired(string? exteriorNumber)
    {
        Result<Address> result = CreateAddress(exteriorNumber: exteriorNumber);

        Assert.Equal(AddressErrors.ExteriorNumberRequired, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutPostalCode_ReturnsPostalCodeRequired(string? postalCode)
    {
        Result<Address> result = CreateAddress(postalCode: postalCode);

        Assert.Equal(AddressErrors.PostalCodeRequired, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutMunicipality_ReturnsMunicipalityRequired(string? municipality)
    {
        Result<Address> result = CreateAddress(municipality: municipality);

        Assert.Equal(AddressErrors.MunicipalityRequired, result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutState_ReturnsStateRequired(string? state)
    {
        Result<Address> result = CreateAddress(state: state);

        Assert.Equal(AddressErrors.StateRequired, result.Error);
    }

    // ---------- Código postal inválido ----------

    [Theory]
    [InlineData("0310A")]   // Letra
    [InlineData("03-10")]   // Guion
    public void Create_WithNonDigitPostalCode_ReturnsPostalCodeInvalid(string postalCode)
    {
        Result<Address> result = CreateAddress(postalCode: postalCode);

        Assert.Equal(AddressErrors.PostalCodeInvalid, result.Error);
    }

    [Theory]
    [InlineData("0310")]     // 4 dígitos
    [InlineData("031000")]   // 6 dígitos
    public void Create_WithWrongPostalCodeLength_ReturnsPostalCodeInvalidLength(string postalCode)
    {
        Result<Address> result = CreateAddress(postalCode: postalCode);

        Assert.Equal(AddressErrors.PostalCodeInvalidLength, result.Error);
    }

    // ---------- Longitud máxima (pruebas de límite) ----------

    [Fact]
    public void Create_WithAllFieldsAtMaxLength_ReturnsSuccess()
    {
        // Act: cada campo con exactamente su longitud máxima
        Result<Address> result = CreateAddress(
            street: new string('a', Address.StreetMaxLength),
            exteriorNumber: new string('1', Address.ExteriorNumberMaxLength),
            interiorNumber: new string('b', Address.InteriorNumberMaxLength),
            municipality: new string('c', Address.MunicipalityMaxLength),
            state: new string('d', Address.StateMaxLength));

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithStreetOverMaxLength_ReturnsStreetTooLong()
    {
        Result<Address> result = CreateAddress(street: new string('a', Address.StreetMaxLength + 1));

        Assert.Equal(AddressErrors.StreetTooLong, result.Error);
    }

    [Fact]
    public void Create_WithExteriorNumberOverMaxLength_ReturnsExteriorNumberTooLong()
    {
        Result<Address> result = CreateAddress(exteriorNumber: new string('1', Address.ExteriorNumberMaxLength + 1));

        Assert.Equal(AddressErrors.ExteriorNumberTooLong, result.Error);
    }

    [Fact]
    public void Create_WithInteriorNumberOverMaxLength_ReturnsInteriorNumberTooLong()
    {
        Result<Address> result = CreateAddress(interiorNumber: new string('b', Address.InteriorNumberMaxLength + 1));

        Assert.Equal(AddressErrors.InteriorNumberTooLong, result.Error);
    }

    [Fact]
    public void Create_WithMunicipalityOverMaxLength_ReturnsMunicipalityTooLong()
    {
        Result<Address> result = CreateAddress(municipality: new string('c', Address.MunicipalityMaxLength + 1));

        Assert.Equal(AddressErrors.MunicipalityTooLong, result.Error);
    }

    [Fact]
    public void Create_WithStateOverMaxLength_ReturnsStateTooLong()
    {
        Result<Address> result = CreateAddress(state: new string('d', Address.StateMaxLength + 1));

        Assert.Equal(AddressErrors.StateTooLong, result.Error);
    }

    [Fact]
    public void Create_WithSurroundingSpacesAndMaxLength_ReturnsSuccess()
    {
        // Arrange: la longitud se mide después de limpiar
        string street = "   " + new string('a', Address.StreetMaxLength) + "   ";

        // Act
        Result<Address> result = CreateAddress(street: street);

        // Assert
        Assert.True(result.IsSuccess);
    }

    // ---------- Orden de las validaciones ----------

    [Fact]
    public void Create_WithSeveralInvalidFields_ReturnsFirstErrorInAddressOrder()
    {
        // Act: la calle y el estado están vacíos; la calle va primero
        Result<Address> result = CreateAddress(street: "", state: "");

        // Assert
        Assert.Equal(AddressErrors.StreetRequired, result.Error);
    }

    // ---------- Igualdad ----------

    [Fact]
    public void Create_WithSameData_ReturnsEqualAddresses()
    {
        // Act
        Address first = CreateAddress().Value;
        Address second = CreateAddress(street: "  Av.  Insurgentes Sur ").Value;

        // Assert: es un record; después de limpiar, los datos son los mismos
        Assert.Equal(first, second);
    }

    [Fact]
    public void Create_WithDifferentInteriorNumber_ReturnsDifferentAddresses()
    {
        // Act
        Address first = CreateAddress(interiorNumber: "Piso 3").Value;
        Address second = CreateAddress(interiorNumber: "Piso 4").Value;

        // Assert
        Assert.NotEqual(first, second);
    }
}
