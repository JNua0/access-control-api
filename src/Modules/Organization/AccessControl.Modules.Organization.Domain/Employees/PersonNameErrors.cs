using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.Employees;

public static class PersonNameErrors
{
    public static readonly Error FirstNameRequired = Error.Validation(
        "PersonName.FirstNameRequired",
        "El nombre del empleado es obligatorio.");

    public static readonly Error FirstNameInvalid = Error.Validation(
        "PersonName.FirstNameInvalid",
        "El nombre del empleado no puede tener números ni caracteres especiales.");

    public static readonly Error FirstNameTooLong = Error.Validation(
        "PersonName.FirstNameTooLong",
        $"El nombre del empleado no debe exceder {PersonName.MaxLength} caracteres.");

    public static readonly Error LastNameRequired = Error.Validation(
        "PersonName.LastNameRequired",
        "El empleado debe tener al menos un apellido.");

    public static readonly Error PaternalLastNameInvalid = Error.Validation(
        "PersonName.PaternalLastNameInvalid",
        "El apellido paterno del empleado no puede contener números ni caracteres especiales.");

    public static readonly Error PaternalLastNameTooLong = Error.Validation(
        "PersonName.PaternalLastNameTooLong",
        $"El apellido paterno del empleado no debe exceder {PersonName.MaxLength} caracteres.");

    public static readonly Error MaternalLastNameInvalid = Error.Validation(
        "PersonName.MaternalLastNameInvalid",
        "El apellido materno del empleado no puede contener números ni caracteres especiales.");

    public static readonly Error MaternalLastNameTooLong = Error.Validation(
        "PersonName.MaternalLastNameTooLong",
        $"El apellido materno del empleado no debe exceder {PersonName.MaxLength} caracteres.");
}
