using AccessControl.Modules.Organization.Domain.Employees;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Tests.Employees;

public class EmployeeTests
{
    // Employee recibe objetos de valor ya validados; aquí no se prueban sus reglas
    // (eso lo hacen PersonNameTests y EmployeeNumberTests).
    private static EmployeeNumber CreateNumber(string value = "2608001") => EmployeeNumber.Create(value).Value;

    private static PersonName CreateName(string firstName = "Juan") => PersonName.Create(firstName, "Pérez", "López").Value;

    private static Result<Employee> CreateEmployee(int officeId = 1, int departmentId = 2, DateOnly? hiredAt = null)
        => Employee.Create(CreateNumber(), officeId, departmentId, CreateName(), hiredAt);

    private static Employee NewEmployee() => CreateEmployee().Value;

    // ---------- Create ----------

    [Fact]
    public void Create_WithValidData_ReturnsActiveEmployeeWithoutScheduleOrPhoto()
    {
        // Arrange
        EmployeeNumber number = CreateNumber();
        PersonName name = CreateName();

        // Act
        Result<Employee> result = Employee.Create(number, 1, 2, name, null);

        // Assert
        Assert.True(result.IsSuccess);
        Employee employee = result.Value;
        Assert.Equal(number, employee.Number);
        Assert.Equal(1, employee.OfficeId);
        Assert.Equal(2, employee.DepartmentId);
        Assert.Equal(name, employee.Name);
        Assert.True(employee.IsActive);
        Assert.Null(employee.WorkScheduleId);   // Se puede dar de alta sin horario
        Assert.Null(employee.PhotoPath);
        Assert.Null(employee.HiredAt);
    }

    [Fact]
    public void Create_WithHireDate_KeepsIt()
    {
        // Act
        Result<Employee> result = CreateEmployee(hiredAt: new DateOnly(2019, 3, 15));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2019, 3, 15), result.Value.HiredAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidOfficeId_ReturnsOfficeIdInvalid(int officeId)
    {
        // Act
        Result<Employee> result = CreateEmployee(officeId: officeId);

        // Assert
        Assert.Equal(EmployeeErrors.OfficeIdInvalid, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_WithInvalidDepartmentId_ReturnsDepartmentIdInvalid(int departmentId)
    {
        // Act
        Result<Employee> result = CreateEmployee(departmentId: departmentId);

        // Assert
        Assert.Equal(EmployeeErrors.DepartmentIdInvalid, result.Error);
    }

    [Fact]
    public void Create_WithInvalidOfficeAndDepartment_ReturnsOfficeErrorFirst()
    {
        // Act
        Result<Employee> result = CreateEmployee(officeId: 0, departmentId: 0);

        // Assert
        Assert.Equal(EmployeeErrors.OfficeIdInvalid, result.Error);
    }

    [Fact]
    public void Create_WithNullNumber_ThrowsArgumentNullException()
    {
        // Act y Assert: un objeto de valor nulo es un bug, no un error del usuario
        Assert.Throws<ArgumentNullException>(() => Employee.Create(null!, 1, 2, CreateName(), null));
    }

    [Fact]
    public void Create_WithNullName_ThrowsArgumentNullException()
    {
        // Act y Assert
        Assert.Throws<ArgumentNullException>(() => Employee.Create(CreateNumber(), 1, 2, null!, null));
    }

    // ---------- Rename ----------

    [Fact]
    public void Rename_WithNewName_ChangesName()
    {
        // Arrange
        Employee employee = NewEmployee();
        PersonName newName = CreateName("Pedro");

        // Act
        employee.Rename(newName);

        // Assert
        Assert.Equal(newName, employee.Name);
    }

    [Fact]
    public void Rename_DoesNotChangeEmployeeNumber()
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act
        employee.Rename(CreateName("Pedro"));

        // Assert: el número de empleado nunca cambia (RN-ORG-01)
        Assert.Equal(CreateNumber(), employee.Number);
    }

    [Fact]
    public void Rename_WithNullName_ThrowsAndKeepsName()
    {
        // Arrange
        Employee employee = NewEmployee();
        PersonName originalName = employee.Name;

        // Act y Assert
        Assert.Throws<ArgumentNullException>(() => employee.Rename(null!));
        Assert.Equal(originalName, employee.Name);
    }

    // ---------- ChangeOffice y ChangeDepartment ----------

    [Fact]
    public void ChangeOffice_WithValidId_ChangesOffice()
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act
        Result result = employee.ChangeOffice(7);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(7, employee.OfficeId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeOffice_WithInvalidId_ReturnsErrorAndKeepsOffice(int officeId)
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act
        Result result = employee.ChangeOffice(officeId);

        // Assert
        Assert.Equal(EmployeeErrors.OfficeIdInvalid, result.Error);
        Assert.Equal(1, employee.OfficeId);
    }

    [Fact]
    public void ChangeDepartment_WithValidId_ChangesDepartment()
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act
        Result result = employee.ChangeDepartment(9);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(9, employee.DepartmentId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeDepartment_WithInvalidId_ReturnsErrorAndKeepsDepartment(int departmentId)
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act
        Result result = employee.ChangeDepartment(departmentId);

        // Assert
        Assert.Equal(EmployeeErrors.DepartmentIdInvalid, result.Error);
        Assert.Equal(2, employee.DepartmentId);
    }

    // ---------- AssignWorkSchedule ----------

    [Fact]
    public void AssignWorkSchedule_WithoutSchedule_AssignsIt()
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act
        Result result = employee.AssignWorkSchedule(3);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, employee.WorkScheduleId);
    }

    [Fact]
    public void AssignWorkSchedule_WithSchedule_ReplacesIt()
    {
        // Arrange
        Employee employee = NewEmployee();
        employee.AssignWorkSchedule(3);

        // Act
        Result result = employee.AssignWorkSchedule(4);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(4, employee.WorkScheduleId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AssignWorkSchedule_WithInvalidId_ReturnsErrorAndKeepsNoSchedule(int workScheduleId)
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act
        Result result = employee.AssignWorkSchedule(workScheduleId);

        // Assert
        Assert.Equal(EmployeeErrors.WorkScheduleIdInvalid, result.Error);
        Assert.Null(employee.WorkScheduleId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AssignWorkSchedule_WithInvalidIdAfterAssigned_KeepsPreviousSchedule(int workScheduleId)
    {
        // Arrange
        Employee employee = NewEmployee();
        employee.AssignWorkSchedule(3);

        // Act
        Result result = employee.AssignWorkSchedule(workScheduleId);

        // Assert: una vez asignado, el horario nunca vuelve a quedar vacío
        Assert.Equal(EmployeeErrors.WorkScheduleIdInvalid, result.Error);
        Assert.Equal(3, employee.WorkScheduleId);
    }

    // ---------- Foto ----------

    [Fact]
    public void ChangePhoto_WithValidPath_SetsPhoto()
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act
        Result result = employee.ChangePhoto("employees/1/3f2a9c.jpg");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("employees/1/3f2a9c.jpg", employee.PhotoPath);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ChangePhoto_WithEmptyPath_ReturnsPhotoPathRequiredAndKeepsPhoto(string photoPath)
    {
        // Arrange
        Employee employee = NewEmployee();
        employee.ChangePhoto("employees/1/original.jpg");

        // Act
        Result result = employee.ChangePhoto(photoPath);

        // Assert
        Assert.Equal(EmployeeErrors.PhotoPathRequired, result.Error);
        Assert.Equal("employees/1/original.jpg", employee.PhotoPath);
    }

    [Fact]
    public void ChangePhoto_WithPathAtMaxLength_ReturnsSuccess()
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act: exactamente 200 caracteres
        Result result = employee.ChangePhoto(new string('a', Employee.PhotoPathMaxLength));

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ChangePhoto_WithPathOverMaxLength_ReturnsPhotoPathTooLong()
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act: 201 caracteres
        Result result = employee.ChangePhoto(new string('a', Employee.PhotoPathMaxLength + 1));

        // Assert
        Assert.Equal(EmployeeErrors.PhotoPathTooLong, result.Error);
        Assert.Null(employee.PhotoPath);
    }

    [Fact]
    public void RemovePhoto_WithPhoto_SetsPhotoToNull()
    {
        // Arrange
        Employee employee = NewEmployee();
        employee.ChangePhoto("employees/1/3f2a9c.jpg");

        // Act
        employee.RemovePhoto();

        // Assert
        Assert.Null(employee.PhotoPath);
    }

    // ---------- Fecha de ingreso ----------

    [Fact]
    public void ChangeHiredAt_WithDate_SetsIt()
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act
        employee.ChangeHiredAt(new DateOnly(2020, 1, 6));

        // Assert
        Assert.Equal(new DateOnly(2020, 1, 6), employee.HiredAt);
    }

    [Fact]
    public void ChangeHiredAt_WithNull_ClearsIt()
    {
        // Arrange
        Employee employee = CreateEmployee(hiredAt: new DateOnly(2019, 3, 15)).Value;

        // Act: la fecha es opcional; se puede quitar si se capturó por error
        employee.ChangeHiredAt(null);

        // Assert
        Assert.Null(employee.HiredAt);
    }

    // ---------- Activate y Deactivate ----------

    [Fact]
    public void Deactivate_ActiveEmployee_SetsInactive()
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act
        employee.Deactivate();

        // Assert
        Assert.False(employee.IsActive);
    }

    [Fact]
    public void Activate_InactiveEmployee_SetsActive()
    {
        // Arrange
        Employee employee = NewEmployee();
        employee.Deactivate();

        // Act
        employee.Activate();

        // Assert
        Assert.True(employee.IsActive);
    }

    [Fact]
    public void Deactivate_Twice_StaysInactive()
    {
        // Arrange
        Employee employee = NewEmployee();

        // Act
        employee.Deactivate();
        employee.Deactivate();

        // Assert
        Assert.False(employee.IsActive);
    }

    [Fact]
    public void Deactivate_KeepsEmployeeData()
    {
        // Arrange
        Employee employee = NewEmployee();
        employee.AssignWorkSchedule(3);

        // Act: dar de baja no borra datos (RN-ORG-02: los datos y registros se conservan)
        employee.Deactivate();

        // Assert
        Assert.Equal(CreateNumber(), employee.Number);
        Assert.Equal(3, employee.WorkScheduleId);
    }

    // ---------- Identidad ----------

    [Fact]
    public void Create_TwiceWithSameData_ReturnsDifferentEmployees()
    {
        // Act
        Employee first = NewEmployee();
        Employee second = NewEmployee();

        // Assert: es una entidad; se distingue por su Id, no por sus datos
        Assert.NotEqual(first, second);
    }
}
