using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.Departments;

public static class DepartmentErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "Department.NameRequired",
        "El nombre del departamento es obligatorio.");

    public static readonly Error NameTooLong = Error.Validation(
        "Department.NameTooLong",
        $"El nombre del departamento no debe superar los {Department.NameMaxLength} caracteres.");
}
