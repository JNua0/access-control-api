using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.Offices;

public static class AddressErrors
{
    // Street Errors
    public static readonly Error StreetRequired = Error.Validation(
        "Address.StreetRequired",
        "La calle es obligatoria.");

    public static readonly Error StreetTooLong = Error.Validation(
        "Address.StreetTooLong",
        $"La calle no debe exceder los {Address.StreetMaxLength} caracteres.");

    // Exterior Number Errors
    public static readonly Error ExteriorNumberRequired = Error.Validation(
        "Address.ExteriorNumberRequired",
        "El número exterior es obligatorio.");

    public static readonly Error ExteriorNumberTooLong = Error.Validation(
        "Address.ExteriorNumberTooLong",
        $"El número exterior no debe exceder los {Address.ExteriorNumberMaxLength} caracteres.");

    // Interior Number Error
    public static readonly Error InteriorNumberTooLong = Error.Validation(
        "Address.InteriorNumberTooLong",
        $"El número interior no debe exceder los {Address.InteriorNumberMaxLength} caracteres.");

    // Postal Code
    public static readonly Error PostalCodeInvalid = Error.Validation(
        "Address.PostalCodeInvalid",
        "El código postal es inválido, no debe contener letras ni caracteres especiales.");

    public static readonly Error PostalCodeRequired = Error.Validation(
        "Address.PostalCodeRequired",
        "El código postal es obligatorio.");

    public static readonly Error PostalCodeInvalidLength = Error.Validation(
        "Address.PostalCodeInvalidLength",
        $"El código postal debe tener {Address.PostalCodeLength} caracteres.");

    // Municipality Errors
    public static readonly Error MunicipalityRequired = Error.Validation(
        "Address.MunicipalityRequired",
        "El municipio es obligatorio.");

    public static readonly Error MunicipalityTooLong = Error.Validation(
        "Address.MunicipalityTooLong",
        $"El municipio no debe exceder los {Address.MunicipalityMaxLength} caracteres.");

    // State Errors
    public static readonly Error StateRequired = Error.Validation(
        "Address.StateRequired",
        "El estado es obligatorio.");

    public static readonly Error StateTooLong = Error.Validation(
        "Address.StateTooLong",
        $"El estado no debe exceder los {Address.StateMaxLength} caracteres.");
}
