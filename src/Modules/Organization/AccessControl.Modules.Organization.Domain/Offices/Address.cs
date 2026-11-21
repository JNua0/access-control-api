using System.Text.RegularExpressions;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.Offices;

public sealed record Address
{
    public const int StreetMaxLength = 80;
    public const int ExteriorNumberMaxLength = 10;
    public const int InteriorNumberMaxLength = 20;
    public const int PostalCodeLength = 5;
    public const int MunicipalityMaxLength = 40;
    public const int StateMaxLength = 40;

    private static readonly Regex _replaceSpaces = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex _onlyNumbers = new(@"^[0-9]+$", RegexOptions.Compiled);

    public string Street { get; }
    public string ExteriorNumber { get; }
    public string? InteriorNumber { get; }
    public string PostalCode { get; }
    public string Municipality { get; }
    public string State { get; }

    private Address(
        string street,
        string exteriorNumber,
        string? interiorNumber,
        string postalCode,
        string municipality,
        string state)
    {
        Street = street;
        ExteriorNumber = exteriorNumber;
        InteriorNumber = interiorNumber;
        PostalCode = postalCode;
        Municipality = municipality;
        State = state;
    }

    public static Result<Address> Create(
        string? street,
        string? exteriorNumber,
        string? interiorNumber,
        string? postalCode,
        string? municipality,
        string? state)
    {
        Result<string> streetResult = ValidateRequiredProperty(street, StreetMaxLength, AddressErrors.StreetRequired, AddressErrors.StreetTooLong);
        Result<string> exteriorNumberResult = ValidateRequiredProperty(exteriorNumber, ExteriorNumberMaxLength, AddressErrors.ExteriorNumberRequired, AddressErrors.ExteriorNumberTooLong);
        Result<string?> interiorNumberResult = ValidateNotRequiredProperty(interiorNumber, InteriorNumberMaxLength, AddressErrors.InteriorNumberTooLong);
        Result<string> postalCodeResult = ValidatePostalCode(postalCode);
        Result<string> municipalityResult = ValidateRequiredProperty(municipality, MunicipalityMaxLength, AddressErrors.MunicipalityRequired, AddressErrors.MunicipalityTooLong);
        Result<string> stateResult = ValidateRequiredProperty(state, StateMaxLength, AddressErrors.StateRequired, AddressErrors.StateTooLong);

        Result[] results = [streetResult, exteriorNumberResult, interiorNumberResult, postalCodeResult, municipalityResult, stateResult];

        foreach (var result in results)
        {
            if (result.IsFailure)
                return Result.Failure<Address>(result.Error);
        }

        return Result.Success(
            new Address(
                streetResult.Value,
                exteriorNumberResult.Value,
                interiorNumberResult.Value,
                postalCodeResult.Value,
                municipalityResult.Value,
                stateResult.Value
            ));
    }

    private static Result<string> ValidateRequiredProperty(
        string? property,
        int maxLength,
        Error errorRequired,
        Error errorTooLong)
    {
        if (string.IsNullOrWhiteSpace(property))
            return Result.Failure<string>(errorRequired);

        string cleanProperty = property.Normalize().Trim();
        cleanProperty = _replaceSpaces.Replace(cleanProperty, " ");

        if (cleanProperty.Length > maxLength)
            return Result.Failure<string>(errorTooLong);

        return Result.Success(cleanProperty);
    }

    private static Result<string?> ValidateNotRequiredProperty(string? property, int maxLength, Error errorTooLong)
    {
        if (string.IsNullOrWhiteSpace(property))
            return Result.Success<string?>(null);

        string? cleanProperty = property!.Normalize().Trim();
        cleanProperty = _replaceSpaces.Replace(cleanProperty, " ");

        if (cleanProperty.Length > maxLength)
            return Result.Failure<string?>(errorTooLong);

        return Result.Success<string?>(cleanProperty);
    }

    private static Result<string> ValidatePostalCode(string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return Result.Failure<string>(AddressErrors.PostalCodeRequired);

        string postalCodeClean = _replaceSpaces.Replace(postalCode, "");

        if (!_onlyNumbers.IsMatch(postalCodeClean))
            return Result.Failure<string>(AddressErrors.PostalCodeInvalid);

        if (postalCodeClean.Length != PostalCodeLength)
            return Result.Failure<string>(AddressErrors.PostalCodeInvalidLength);

        return Result.Success<string>(postalCodeClean);
    }
}
