using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.Employees;

public static class EmployeeErrors
{
    public static readonly Error OfficeIdInvalid = Error.Validation(
        "Employee.OfficeIdInvalid",
        "El identificador de oficina es inválido.");

    public static readonly Error DepartmentIdInvalid = Error.Validation(
        "Employee.DepartmentIdInvalid",
        "El identificador del departamento es inválido.");

    public static readonly Error WorkScheduleIdInvalid = Error.Validation(
        "Employee.WorkScheduleIdInvalid",
        "El identificador del horario de trabajo es inválido.");

    public static readonly Error PhotoPathTooLong = Error.Validation(
        "Employee.PhotoPathTooLong",
        $"La longitud de la ruta de la fotografía no debe exceder los {Employee.PhotoPathMaxLength} caracteres.");

    public static readonly Error PhotoPathRequired = Error.Validation(
        "Employee.PhotoPathRequired",
        "La ruta de la fotografía es obligatoria.");
}
