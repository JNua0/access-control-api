using AccessControl.Modules.Organization.Domain.Departments;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Tests.Departments;

public class DepartmentTests
{
    // ---------- Create ----------

    [Fact]
    public void Create_WithValidName_ReturnsActiveDepartment()
    {
        // Act
        Result<Department> result = Department.Create("Recursos Humanos");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Recursos Humanos", result.Value.Name);
        Assert.True(result.Value.IsActive);
    }

    [Theory]
    [InlineData("Tecnologías de la Información (TI)")]
    [InlineData("I+D")]
    [InlineData("Almacén 2")]
    [InlineData("Calidad/Seguridad")]
    public void Create_WithDigitsAndSymbols_ReturnsSuccess(string name)
    {
        // Act: no hay regla de caracteres; los departamentos pueden llevar números y signos
        Result<Department> result = Department.Create(name);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(name, result.Value.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Create_WithoutName_ReturnsNameRequired(string? name)
    {
        // Act
        Result<Department> result = Department.Create(name);

        // Assert
        Assert.Equal(DepartmentErrors.NameRequired, result.Error);
    }

    [Theory]
    [InlineData("  Recursos Humanos  ", "Recursos Humanos")]
    [InlineData("Recursos     Humanos", "Recursos Humanos")]
    [InlineData("Recursos\tHumanos\n", "Recursos Humanos")]
    public void Create_WithExtraSpaces_CleansName(string name, string expectedName)
    {
        // Act
        Result<Department> result = Department.Create(name);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedName, result.Value.Name);
    }

    [Fact]
    public void Create_WithDecomposedAccent_ReturnsComposedLetter()
    {
        // Arrange: "o" + acento como carácter separado (U+0301)
        string name = "Producción";

        // Act
        Result<Department> result = Department.Create(name);

        // Assert: queda como una sola letra "ó" (U+00F3)
        Assert.True(result.IsSuccess);
        Assert.Equal("Producción", result.Value.Name);
    }

    // ---------- Longitud máxima (pruebas de límite) ----------

    [Fact]
    public void Create_WithNameAtMaxLength_ReturnsSuccess()
    {
        // Arrange: exactamente 60 caracteres
        string name = new string('a', Department.NameMaxLength);

        // Act
        Result<Department> result = Department.Create(name);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithNameOverMaxLength_ReturnsNameTooLong()
    {
        // Arrange: 61 caracteres
        string name = new string('a', Department.NameMaxLength + 1);

        // Act
        Result<Department> result = Department.Create(name);

        // Assert
        Assert.Equal(DepartmentErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Create_WithSurroundingSpacesAndMaxLength_ReturnsSuccess()
    {
        // Arrange: 60 caracteres rodeados de espacios; la longitud se mide después de limpiar
        string name = "     " + new string('a', Department.NameMaxLength) + "     ";

        // Act
        Result<Department> result = Department.Create(name);

        // Assert
        Assert.True(result.IsSuccess);
    }

    // ---------- Rename ----------

    [Fact]
    public void Rename_WithValidName_ChangesName()
    {
        // Arrange
        Department department = Department.Create("Sistemas").Value;

        // Act
        Result result = department.Rename("  Tecnologías   de la Información  ");

        // Assert: cambió, y se guardó limpio
        Assert.True(result.IsSuccess);
        Assert.Equal("Tecnologías de la Información", department.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithoutName_ReturnsNameRequiredAndKeepsName(string? newName)
    {
        // Arrange
        Department department = Department.Create("Sistemas").Value;

        // Act
        Result result = department.Rename(newName);

        // Assert: devuelve el error y el nombre no cambia
        Assert.Equal(DepartmentErrors.NameRequired, result.Error);
        Assert.Equal("Sistemas", department.Name);
    }

    [Fact]
    public void Rename_WithNameOverMaxLength_ReturnsNameTooLongAndKeepsName()
    {
        // Arrange
        Department department = Department.Create("Sistemas").Value;
        string newName = new string('a', Department.NameMaxLength + 1);

        // Act
        Result result = department.Rename(newName);

        // Assert
        Assert.Equal(DepartmentErrors.NameTooLong, result.Error);
        Assert.Equal("Sistemas", department.Name);
    }

    [Fact]
    public void Rename_DoesNotChangeActiveState()
    {
        // Arrange
        Department department = Department.Create("Sistemas").Value;
        department.Deactivate();

        // Act
        department.Rename("Tecnología");

        // Assert: renombrar no reactiva el departamento
        Assert.False(department.IsActive);
    }

    // ---------- Activate y Deactivate ----------

    [Fact]
    public void Deactivate_ActiveDepartment_SetsInactive()
    {
        // Arrange
        Department department = Department.Create("Sistemas").Value;

        // Act
        department.Deactivate();

        // Assert
        Assert.False(department.IsActive);
    }

    [Fact]
    public void Activate_InactiveDepartment_SetsActive()
    {
        // Arrange
        Department department = Department.Create("Sistemas").Value;
        department.Deactivate();

        // Act
        department.Activate();

        // Assert
        Assert.True(department.IsActive);
    }

    [Fact]
    public void Deactivate_Twice_StaysInactive()
    {
        // Arrange
        Department department = Department.Create("Sistemas").Value;

        // Act: desactivar dos veces deja el mismo resultado (idempotente)
        department.Deactivate();
        department.Deactivate();

        // Assert
        Assert.False(department.IsActive);
    }

    [Fact]
    public void Activate_AlreadyActive_StaysActive()
    {
        // Arrange
        Department department = Department.Create("Sistemas").Value;

        // Act
        department.Activate();

        // Assert
        Assert.True(department.IsActive);
    }

    // ---------- Identidad ----------

    [Fact]
    public void Create_TwiceWithSameName_ReturnsDifferentDepartments()
    {
        // Act
        Department first = Department.Create("Sistemas").Value;
        Department second = Department.Create("Sistemas").Value;

        // Assert: es una entidad (clase), no un record; dos objetos distintos no son iguales
        // aunque tengan el mismo nombre. Se distinguen por su Id, que asigna la base de datos.
        Assert.NotEqual(first, second);
    }
}
