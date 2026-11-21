using System.Text.RegularExpressions;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.Employees;

public sealed record PersonName
{
    public const int MaxLength = 40;

    private static readonly Regex _multipleSpaces = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex _repeatedApostrophes = new("'{2,}", RegexOptions.Compiled);

    public string FirstName { get; }
    public string? PaternalLastName { get; }
    public string? MaternalLastName { get; }

    // Después de Create, cada parte es null o un texto válido: basta con quitar los null.
    public string FullName => string.Join(" ",
        new[] { FirstName, PaternalLastName, MaternalLastName }.Where(part => part is not null));

    private PersonName(string firstName, string? paternalLastName, string? maternalLastName)
    {
        FirstName = firstName;
        PaternalLastName = paternalLastName;
        MaternalLastName = maternalLastName;
    }

    // Responsabilidad de Create: ORQUESTAR. Decide el orden de los pasos y qué error
    // corresponde a cada campo, pero no sabe cómo se limpia ni cómo se valida un texto.
    public static Result<PersonName> Create(string? firstName, string? paternalLastName, string? maternalLastName)
    {
        // 1. Nombre: normalizar, revisar que exista y validar.
        string? normalizedFirstName = NormalizeName(firstName);

        if (normalizedFirstName is null)
            return Result.Failure<PersonName>(PersonNameErrors.FirstNameRequired);

        Result firstNameResult = ValidateNamePart(
            normalizedFirstName,
            PersonNameErrors.FirstNameInvalid,
            PersonNameErrors.FirstNameTooLong);

        if (firstNameResult.IsFailure)
            return Result.Failure<PersonName>(firstNameResult.Error);

        // 2. Apellidos: normalizar y revisar que haya al menos uno (RN-ORG-04).
        string? normalizedPaternalLastName = NormalizeName(paternalLastName);
        string? normalizedMaternalLastName = NormalizeName(maternalLastName);

        if (normalizedPaternalLastName is null && normalizedMaternalLastName is null)
            return Result.Failure<PersonName>(PersonNameErrors.LastNameRequired);

        // 3. Validar cada apellido que exista.
        Result paternalLastNameResult = ValidateOptionalNamePart(
            normalizedPaternalLastName,
            PersonNameErrors.PaternalLastNameInvalid,
            PersonNameErrors.PaternalLastNameTooLong);

        if (paternalLastNameResult.IsFailure)
            return Result.Failure<PersonName>(paternalLastNameResult.Error);

        Result maternalLastNameResult = ValidateOptionalNamePart(
            normalizedMaternalLastName,
            PersonNameErrors.MaternalLastNameInvalid,
            PersonNameErrors.MaternalLastNameTooLong);

        if (maternalLastNameResult.IsFailure)
            return Result.Failure<PersonName>(maternalLastNameResult.Error);

        // 4. Crear con los valores ya normalizados: lo que se validó es lo que se guarda.
        return Result.Success(
            new PersonName(
                normalizedFirstName,
                normalizedPaternalLastName,
                normalizedMaternalLastName));
    }

    // En logs y en el depurador se ve "Juan Pérez López" en lugar de todas las propiedades.
    public override string ToString() => FullName;

    // Responsabilidad: LIMPIAR. No valida nada ni conoce los errores.
    // Devuelve null si no hay texto; si hay, lo devuelve limpio.
    private static string? NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        // Normalize() junta una letra y su acento cuando vienen como dos caracteres
        // (pasa al copiar de algunos programas o PDF), para que "José" no se rechace.
        string cleanName = _multipleSpaces.Replace(name.Normalize().Trim(), " ");

        // Un solo apóstrofo donde se escribieron varios: "O''Brien" -> "O'Brien".
        // Las mayúsculas y minúsculas se respetan tal como se capturaron.
        return _repeatedApostrophes.Replace(cleanName, "'");
    }

    // Responsabilidad: VALIDAR un texto ya limpio. Los errores los decide quien llama,
    // así la misma regla sirve para el nombre y los dos apellidos.
    private static Result ValidateNamePart(string name, Error invalidName, Error nameTooLong)
    {
        // Al separar por espacios y apóstrofos, cada parte debe ser solo letras.
        // Una parte vacía significa un separador al inicio, al final o dos juntos ("'Ana", "Ana'", "O ' Brien").
        string[] parts = name.Split(' ', '\'');

        if (parts.Any(part => part.Length == 0 || !part.All(char.IsLetter)))
            return Result.Failure(invalidName);

        if (name.Length > MaxLength)
            return Result.Failure(nameTooLong);

        return Result.Success();
    }

    // Un apellido que no existe (null) es válido: la regla de "al menos uno" ya se revisó en Create.
    private static Result ValidateOptionalNamePart(string? name, Error invalidName, Error nameTooLong) =>
        name is null
            ? Result.Success()
            : ValidateNamePart(name, invalidName, nameTooLong);
}
