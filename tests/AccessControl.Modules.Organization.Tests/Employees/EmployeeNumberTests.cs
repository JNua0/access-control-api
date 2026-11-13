using AccessControl.Modules.Organization.Domain.Employees;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Tests.Employees;

public class EmployeeNumberTests
{
    // ---------- Create: números válidos ----------

    [Fact]
    public void Create_WithValidNumber_ReturnsSuccess()
    {
        // Act
        Result<EmployeeNumber> result = EmployeeNumber.Create("2608001");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("2608001", result.Value.Value);
    }

    [Theory]
    [InlineData(" 2608001 ")]      // Espacios en los extremos
    [InlineData("2608 001")]       // Espacio en medio
    [InlineData("\t2608001\n")]    // Tabulador y salto de línea
    public void Create_WithSpaces_RemovesThem(string number)
    {
        // Act
        Result<EmployeeNumber> result = EmployeeNumber.Create(number);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("2608001", result.Value.Value);
    }

    [Theory]
    [InlineData("2601001")]   // Enero
    [InlineData("2612999")]   // Diciembre, último consecutivo
    public void Create_WithBoundaryMonths_ReturnsSuccess(string number)
    {
        // Act
        Result<EmployeeNumber> result = EmployeeNumber.Create(number);

        // Assert
        Assert.True(result.IsSuccess);
    }

    // ---------- Create: números inválidos (cada caso falla por una sola razón) ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutNumber_ReturnsNumberRequired(string? number)
    {
        // Act
        Result<EmployeeNumber> result = EmployeeNumber.Create(number);

        // Assert
        Assert.Equal(EmployeeNumberErrors.NumberRequired, result.Error);
    }

    [Theory]
    [InlineData("26O8001")]   // Letra O en lugar de cero
    [InlineData("2608-01")]   // Guion
    [InlineData("260800A")]   // Letra al final
    public void Create_WithNonDigitCharacters_ReturnsNumberInvalid(string number)
    {
        // Act
        Result<EmployeeNumber> result = EmployeeNumber.Create(number);

        // Assert
        Assert.Equal(EmployeeNumberErrors.NumberInvalid, result.Error);
    }

    [Theory]
    [InlineData("260800")]     // 6 dígitos
    [InlineData("26080011")]   // 8 dígitos
    [InlineData("1")]          // 1 dígito
    public void Create_WithWrongLength_ReturnsNumberInvalidLength(string number)
    {
        // Act
        Result<EmployeeNumber> result = EmployeeNumber.Create(number);

        // Assert
        Assert.Equal(EmployeeNumberErrors.NumberInvalidLength, result.Error);
    }

    [Theory]
    [InlineData("2600001")]   // Mes 00
    [InlineData("2613001")]   // Mes 13
    [InlineData("2699001")]   // Mes 99
    public void Create_WithInvalidMonth_ReturnsNumberInvalidFormat(string number)
    {
        // Act
        Result<EmployeeNumber> result = EmployeeNumber.Create(number);

        // Assert
        Assert.Equal(EmployeeNumberErrors.NumberInvalidFormat, result.Error);
    }

    // ---------- Generate ----------

    [Theory]
    [InlineData(2026, 8, 15, 3, "2608003")]
    [InlineData(2026, 9, 1, 1, "2609001")]
    [InlineData(2026, 12, 31, 999, "2612999")]
    [InlineData(2030, 1, 10, 42, "3001042")]
    public void Generate_WithDateAndSequence_ReturnsFormattedNumber(
        int year,
        int month,
        int day,
        int sequence,
        string expectedNumber)
    {
        // Arrange
        var date = new DateOnly(year, month, day);

        // Act
        Result<EmployeeNumber> result = EmployeeNumber.Generate(date, sequence);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedNumber, result.Value.Value);
    }

    [Theory]
    [InlineData(EmployeeNumber.MinSequence)]   // 1
    [InlineData(EmployeeNumber.MaxSequence)]   // 999
    public void Generate_WithSequenceAtLimits_ReturnsSuccess(int sequence)
    {
        // Act
        Result<EmployeeNumber> result = EmployeeNumber.Generate(new DateOnly(2026, 8, 1), sequence);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1000)]
    public void Generate_WithSequenceOutOfRange_ReturnsSequenceOutOfRange(int sequence)
    {
        // Act
        Result<EmployeeNumber> result = EmployeeNumber.Generate(new DateOnly(2026, 8, 1), sequence);

        // Assert
        Assert.Equal(EmployeeNumberErrors.SequenceOutOfRange, result.Error);
    }

    [Fact]
    public void Generate_WithSequenceOutOfRange_ReturnsConflictError()
    {
        // Act
        Result<EmployeeNumber> result = EmployeeNumber.Generate(new DateOnly(2026, 8, 1), 1000);

        // Assert: se agotaron los números del mes; depende del estado del sistema, no de los datos enviados
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Theory]
    [InlineData(2026, 1, 1, 1)]
    [InlineData(2026, 8, 15, 3)]
    [InlineData(2026, 12, 31, 999)]
    public void Generate_ResultIsAcceptedByCreate(int year, int month, int day, int sequence)
    {
        // Arrange
        EmployeeNumber generated = EmployeeNumber.Generate(new DateOnly(year, month, day), sequence).Value;

        // Act: lo que genera Generate siempre debe ser válido para Create
        Result<EmployeeNumber> created = EmployeeNumber.Create(generated.Value);

        // Assert
        Assert.True(created.IsSuccess);
        Assert.Equal(generated, created.Value);
    }

    // ---------- Igualdad y ToString ----------

    [Fact]
    public void Create_WithSameNumber_ReturnsEqualNumbers()
    {
        // Act
        EmployeeNumber first = EmployeeNumber.Create("2608001").Value;
        EmployeeNumber second = EmployeeNumber.Create(" 2608001 ").Value;

        // Assert: es un record, así que se compara por valor
        Assert.Equal(first, second);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Act
        EmployeeNumber number = EmployeeNumber.Create("2608001").Value;

        // Assert
        Assert.Equal("2608001", number.ToString());
    }
}
