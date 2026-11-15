using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.Offices;

public static class OfficeErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "Office.NameRequired",
        "El nombre de la oficina es obligatorio.");

    public static readonly Error NameTooLong = Error.Validation(
        "Office.NameTooLong",
        $"El nombre de la oficina no debe exceder {Office.NameMaxLength} caracteres.");
}
