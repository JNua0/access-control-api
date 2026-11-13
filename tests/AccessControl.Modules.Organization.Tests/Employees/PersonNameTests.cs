using AccessControl.Modules.Organization.Domain.Employees;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Tests.Employees;

public class PersonNameTests
{
    // ---------- Casos válidos ----------

    [Fact]
    public void Create_WithAllNames_ReturnsSuccess()
    {
        // Arrange
        string firstName = "Juan";
        string paternalLastName = "Pérez";
        string maternalLastName = "López";

        // Act
        Result<PersonName> result = PersonName.Create(firstName, paternalLastName, maternalLastName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Juan", result.Value.FirstName);
        Assert.Equal("Pérez", result.Value.PaternalLastName);
        Assert.Equal("López", result.Value.MaternalLastName);
    }

    [Fact]
    public void Create_WithoutPaternalLastName_ReturnsSuccess()
    {
        // Act
        Result<PersonName> result = PersonName.Create("Juan", null, "López");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.PaternalLastName);
        Assert.Equal("López", result.Value.MaternalLastName);
    }

    [Fact]
    public void Create_WithoutMaternalLastName_ReturnsSuccess()
    {
        // Act
        Result<PersonName> result = PersonName.Create("Juan", "Pérez", null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Pérez", result.Value.PaternalLastName);
        Assert.Null(result.Value.MaternalLastName);
    }

    [Fact]
    public void Create_WithLowercaseName_KeepsCase()
    {
        // Act
        Result<PersonName> result = PersonName.Create("juan", "de la cruz", null);

        // Assert: las mayúsculas y minúsculas se respetan tal como se capturaron
        Assert.True(result.IsSuccess);
        Assert.Equal("juan", result.Value.FirstName);
        Assert.Equal("de la cruz", result.Value.PaternalLastName);
    }

    // ---------- Nombre completo y ToString ----------

    [Theory]
    [InlineData("Juan", "Chabelo", "Díaz", "Juan Chabelo Díaz")]
    [InlineData("Jesús", null, "Ramón", "Jesús Ramón")]
    [InlineData("María José", "de González", null, "María José de González")]
    [InlineData("Ana", "de la Ochoa", null, "Ana de la Ochoa")]
    [InlineData("Liam", "O'Brien", null, "Liam O'Brien")]
    public void FullName_WithValidNames_JoinsPartsWithSpaces(
        string firstName,
        string? paternalLastName,
        string? maternalLastName,
        string expectedFullName)
    {
        // Act
        Result<PersonName> result = PersonName.Create(firstName, paternalLastName, maternalLastName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedFullName, result.Value.FullName);
    }

    [Fact]
    public void ToString_ReturnsFullName()
    {
        // Act
        Result<PersonName> result = PersonName.Create("Juan", "Pérez", "López");

        // Assert
        Assert.Equal("Juan Pérez López", result.Value.ToString());
    }

    // ---------- Limpieza ----------

    [Fact]
    public void Create_WithRepeatedSpaces_CollapsesToOne()
    {
        // Act
        Result<PersonName> result = PersonName.Create("   Juan    Dev     ", "de     Los  Altos    ", "     García");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Juan Dev", result.Value.FirstName);
        Assert.Equal("de Los Altos", result.Value.PaternalLastName);
        Assert.Equal("García", result.Value.MaternalLastName);
    }

    [Fact]
    public void Create_WithTabsAndLineBreaks_ReplacesWithSingleSpace()
    {
        // Act
        Result<PersonName> result = PersonName.Create("José\tLuis", "de la\nCruz", null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("José Luis", result.Value.FirstName);
        Assert.Equal("de la Cruz", result.Value.PaternalLastName);
    }

    [Fact]
    public void Create_WithRepeatedApostrophes_CollapsesToOne()
    {
        // Act
        Result<PersonName> result = PersonName.Create("Juan''''Dev", "O'''''Brien", "Aya'''la");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Juan'Dev", result.Value.FirstName);
        Assert.Equal("O'Brien", result.Value.PaternalLastName);
        Assert.Equal("Aya'la", result.Value.MaternalLastName);
    }

    [Fact]
    public void Create_WithDecomposedAccent_ReturnsComposedLetter()
    {
        // Arrange: "e" + acento como carácter separado (U+0301), como llega al copiar de algunos PDF
        string firstName = "José";

        // Act
        Result<PersonName> result = PersonName.Create(firstName, "Pérez", null);

        // Assert: se acepta y queda como una sola letra "é" (U+00E9)
        Assert.True(result.IsSuccess);
        Assert.Equal("José", result.Value.FirstName);
    }

    // ---------- Campos obligatorios ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Create_WithoutFirstName_ReturnsFirstNameRequired(string? firstName)
    {
        // Act
        Result<PersonName> result = PersonName.Create(firstName, "Pérez", "López");

        // Assert
        Assert.Equal(PersonNameErrors.FirstNameRequired, result.Error);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", null)]
    [InlineData(null, "   ")]
    public void Create_WithoutLastNames_ReturnsLastNameRequired(string? paternalLastName, string? maternalLastName)
    {
        // Act
        Result<PersonName> result = PersonName.Create("Juan", paternalLastName, maternalLastName);

        // Assert
        Assert.Equal(PersonNameErrors.LastNameRequired, result.Error);
    }

    // ---------- Caracteres inválidos (cada caso falla por una sola razón) ----------

    [Theory]
    [InlineData("Juan3")]       // Número
    [InlineData("Ja$")]         // Símbolo
    [InlineData("(Jesús")]      // Paréntesis
    [InlineData("Ramón-Das")]   // Guion: no se acepta
    [InlineData("@@@")]         // Solo símbolos
    public void Create_WithInvalidCharactersInFirstName_ReturnsFirstNameInvalid(string firstName)
    {
        // Act
        Result<PersonName> result = PersonName.Create(firstName, "Pérez", "López");

        // Assert
        Assert.Equal(PersonNameErrors.FirstNameInvalid, result.Error);
    }

    [Theory]
    [InlineData("'Ana")]        // Apóstrofo al inicio
    [InlineData("Ana'")]        // Apóstrofo al final
    [InlineData("O ' Brien")]   // Espacio junto al apóstrofo
    [InlineData("' '")]         // Solo separadores
    [InlineData("''")]          // Solo apóstrofos (se reducen a uno)
    public void Create_WithMisplacedSeparatorsInFirstName_ReturnsFirstNameInvalid(string firstName)
    {
        // Act
        Result<PersonName> result = PersonName.Create(firstName, "Pérez", "López");

        // Assert
        Assert.Equal(PersonNameErrors.FirstNameInvalid, result.Error);
    }

    [Theory]
    [InlineData("Pérez2")]
    [InlineData("Pérez-López")]
    [InlineData("'Pérez")]
    public void Create_WithInvalidPaternalLastName_ReturnsPaternalLastNameInvalid(string paternalLastName)
    {
        // Act
        Result<PersonName> result = PersonName.Create("Juan", paternalLastName, "López");

        // Assert
        Assert.Equal(PersonNameErrors.PaternalLastNameInvalid, result.Error);
    }

    [Theory]
    [InlineData("López2")]
    [InlineData("López-Pérez")]
    [InlineData("López'")]
    public void Create_WithInvalidMaternalLastName_ReturnsMaternalLastNameInvalid(string maternalLastName)
    {
        // Act
        Result<PersonName> result = PersonName.Create("Juan", "Pérez", maternalLastName);

        // Assert
        Assert.Equal(PersonNameErrors.MaternalLastNameInvalid, result.Error);
    }

    // ---------- Longitud máxima (pruebas de límite) ----------

    [Fact]
    public void Create_WithFirstNameAtMaxLength_ReturnsSuccess()
    {
        // Arrange: exactamente 40 letras
        string firstName = new string('a', PersonName.MaxLength);

        // Act
        Result<PersonName> result = PersonName.Create(firstName, "Pérez", null);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithFirstNameOverMaxLength_ReturnsFirstNameTooLong()
    {
        // Arrange: 41 letras
        string firstName = new string('a', PersonName.MaxLength + 1);

        // Act
        Result<PersonName> result = PersonName.Create(firstName, "Pérez", null);

        // Assert
        Assert.Equal(PersonNameErrors.FirstNameTooLong, result.Error);
    }

    [Fact]
    public void Create_WithSurroundingSpacesAndMaxLength_ReturnsSuccess()
    {
        // Arrange: 40 letras rodeadas de espacios; la longitud se mide después de limpiar
        string firstName = "   " + new string('a', PersonName.MaxLength) + "   ";

        // Act
        Result<PersonName> result = PersonName.Create(firstName, "Pérez", null);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithPaternalLastNameOverMaxLength_ReturnsPaternalLastNameTooLong()
    {
        // Arrange
        string paternalLastName = new string('a', PersonName.MaxLength + 1);

        // Act
        Result<PersonName> result = PersonName.Create("Juan", paternalLastName, null);

        // Assert
        Assert.Equal(PersonNameErrors.PaternalLastNameTooLong, result.Error);
    }

    [Fact]
    public void Create_WithMaternalLastNameOverMaxLength_ReturnsMaternalLastNameTooLong()
    {
        // Arrange
        string maternalLastName = new string('a', PersonName.MaxLength + 1);

        // Act
        Result<PersonName> result = PersonName.Create("Juan", null, maternalLastName);

        // Assert
        Assert.Equal(PersonNameErrors.MaternalLastNameTooLong, result.Error);
    }

    // ---------- Orden de las validaciones ----------

    [Fact]
    public void Create_WithInvalidFirstNameAndNoLastNames_ReturnsFirstNameInvalid()
    {
        // Act: el nombre se valida por completo antes de revisar los apellidos
        Result<PersonName> result = PersonName.Create("Juan3", null, null);

        // Assert
        Assert.Equal(PersonNameErrors.FirstNameInvalid, result.Error);
    }

    // ---------- Igualdad ----------

    [Fact]
    public void Create_WithSameData_ReturnsEqualNames()
    {
        // Act
        PersonName first = PersonName.Create("Juan", "Pérez", "López").Value;
        PersonName second = PersonName.Create("Juan", "Pérez", "López").Value;

        // Assert: es un record, así que se compara por valor
        Assert.Equal(first, second);
    }
}
