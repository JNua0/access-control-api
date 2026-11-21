using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.Employees;

public static class EmployeeNumberErrors
{
    public static readonly Error NumberRequired = Error.Validation(
        "EmployeeNumber.Required",
        "El número de empleado es obligatorio.");

    public static readonly Error NumberInvalidLength = Error.Validation(
        "EmployeeNumber.InvalidLength",
        $"El número de empleado debe tener {EmployeeNumber.NumberLength} caracteres de longitud.");

    public static readonly Error NumberInvalid = Error.Validation(
        "EmployeeNumber.Invalid",
        "El número del empleado no debe contener letras ni caracteres especiales, solo dígitos del 0 al 9.");

    public static readonly Error NumberInvalidFormat = Error.Validation(
        "EmployeeNumber.InvalidFormat",
        "El número de empleado tiene un mes inválido: los dígitos 3 y 4 deben estar entre 01 y 12.");

    public static readonly Error SequenceOutOfRange = Error.Conflict(
        "EmployeeNumber.SequenceOutOfRange",
        $"La secuencia del número de empleado debe estar entre {EmployeeNumber.MinSequence} y {EmployeeNumber.MaxSequence}.");
}
