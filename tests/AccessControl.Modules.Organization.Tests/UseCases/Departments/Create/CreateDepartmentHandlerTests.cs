using AccessControl.Modules.Organization.Application.UseCases.Departments.Create;
using AccessControl.Modules.Organization.Domain.Departments;
using AccessControl.Modules.Organization.Tests.Fakes;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Tests.UseCases.Departments.Create;

public class CreateDepartmentHandlerTests
{
    // xUnit crea una instancia nueva de la clase por cada prueba,
    // así que cada prueba empieza con un repositorio y un unit of work limpios
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateDepartmentHandler CreateHandler(FakeDepartmentRepository repository)
        => new(repository, _unitOfWork);

    // ---------- Éxito ----------

    [Fact]
    public async Task HandleAsync_WithNewValidName_AddsDepartmentAndSaves()
    {
        // Arrange
        var repository = new FakeDepartmentRepository();
        CreateDepartmentHandler handler = CreateHandler(repository);

        // Act
        Result<int> result = await handler.HandleAsync(new CreateDepartmentCommand("Recursos Humanos"));

        // Assert
        Assert.True(result.IsSuccess);
        Department department = Assert.Single(repository.Departments);
        Assert.Equal("Recursos Humanos", department.Name);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task HandleAsync_WithExtraSpaces_StoresNormalizedName()
    {
        // Arrange
        var repository = new FakeDepartmentRepository();
        CreateDepartmentHandler handler = CreateHandler(repository);

        // Act
        Result<int> result = await handler.HandleAsync(new CreateDepartmentCommand("  Recursos   Humanos  "));

        // Assert: se guarda el nombre que limpió el dominio, no el que llegó
        Assert.True(result.IsSuccess);
        Assert.Equal("Recursos Humanos", Assert.Single(repository.Departments).Name);
    }

    [Fact]
    public async Task HandleAsync_WithNameDifferentFromExisting_ReturnsSuccess()
    {
        // Arrange
        var repository = new FakeDepartmentRepository("Sistemas");
        CreateDepartmentHandler handler = CreateHandler(repository);

        // Act
        Result<int> result = await handler.HandleAsync(new CreateDepartmentCommand("Compras"));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, repository.Departments.Count);
        Assert.Equal(1, _unitOfWork.SaveChangesCount);
    }

    // ---------- Errores del dominio ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithEmptyName_ReturnsNameRequiredAndDoesNotSave(string? name)
    {
        // Arrange
        var repository = new FakeDepartmentRepository();
        CreateDepartmentHandler handler = CreateHandler(repository);

        // Act: el ! es porque el comando espera string, pero desde un JSON puede llegar null
        Result<int> result = await handler.HandleAsync(new CreateDepartmentCommand(name!));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DepartmentErrors.NameRequired, result.Error);
        Assert.Empty(repository.Departments);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task HandleAsync_WithNameOverMaxLength_ReturnsNameTooLongAndDoesNotSave()
    {
        // Arrange
        var repository = new FakeDepartmentRepository();
        CreateDepartmentHandler handler = CreateHandler(repository);
        string name = new('a', Department.NameMaxLength + 1);

        // Act
        Result<int> result = await handler.HandleAsync(new CreateDepartmentCommand(name));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DepartmentErrors.NameTooLong, result.Error);
        Assert.Empty(repository.Departments);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    // ---------- Nombre repetido ----------

    [Fact]
    public async Task HandleAsync_WithExistingName_ReturnsNameAlreadyExistsAndDoesNotSave()
    {
        // Arrange
        var repository = new FakeDepartmentRepository("Sistemas");
        CreateDepartmentHandler handler = CreateHandler(repository);

        // Act
        Result<int> result = await handler.HandleAsync(new CreateDepartmentCommand("Sistemas"));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DepartmentErrors.NameAlreadyExists, result.Error);
        Assert.Single(repository.Departments);          // Solo el que ya existía
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }

    [Theory]
    [InlineData("  Sistemas  ")]
    [InlineData("Sistemas   ")]
    [InlineData("   Sistemas")]
    public async Task HandleAsync_WithExistingNameAndExtraSpaces_ReturnsNameAlreadyExists(string name)
    {
        // Arrange
        var repository = new FakeDepartmentRepository("Sistemas");
        CreateDepartmentHandler handler = CreateHandler(repository);

        // Act: el handler debe comparar con el nombre ya limpio, no con el que llegó
        Result<int> result = await handler.HandleAsync(new CreateDepartmentCommand(name));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DepartmentErrors.NameAlreadyExists, result.Error);
        Assert.Single(repository.Departments);
        Assert.Equal(0, _unitOfWork.SaveChangesCount);
    }
}
