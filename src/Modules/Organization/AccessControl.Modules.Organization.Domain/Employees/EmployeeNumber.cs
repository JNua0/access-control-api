using System.Globalization;
using System.Text.RegularExpressions;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.Employees;

public sealed record EmployeeNumber
{
    public const int NumberLength = 7;
    public const int MinSequence = 1;
    public const int MaxSequence = 999;

    private static readonly Regex _replaceSpaces = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex _onlyNumbers = new(@"^[0-9]+$", RegexOptions.Compiled);
    private static readonly Regex _validFormatNumber = new(@"^[0-9]{2}(0[1-9]|1[0-2])[0-9]{3}$", RegexOptions.Compiled);

    public string Value { get; }

    private EmployeeNumber(string number)
    {
        Value = number;
    }

    public override string ToString() => Value;

    public static Result<EmployeeNumber> Create(string? number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return Result.Failure<EmployeeNumber>(EmployeeNumberErrors.NumberRequired);

        string normalizedNumber = _replaceSpaces.Replace(number, "");

        if (!_onlyNumbers.IsMatch(normalizedNumber))
            return Result.Failure<EmployeeNumber>(EmployeeNumberErrors.NumberInvalid);

        if (normalizedNumber.Length != NumberLength)
            return Result.Failure<EmployeeNumber>(EmployeeNumberErrors.NumberInvalidLength);

        if (!_validFormatNumber.IsMatch(normalizedNumber))
            return Result.Failure<EmployeeNumber>(EmployeeNumberErrors.NumberInvalidFormat);

        return Result.Success(new EmployeeNumber(normalizedNumber));
    }

    public static Result<EmployeeNumber> Generate(DateOnly date, int sequence)
    {
        if (sequence < MinSequence || sequence > MaxSequence)
            return Result.Failure<EmployeeNumber>(EmployeeNumberErrors.SequenceOutOfRange);

        string period = date.ToString("yyMM", CultureInfo.InvariantCulture);
        string sequenceString = sequence.ToString("D3", CultureInfo.InvariantCulture);

        return Result.Success(new EmployeeNumber(period + sequenceString));
    }
}
