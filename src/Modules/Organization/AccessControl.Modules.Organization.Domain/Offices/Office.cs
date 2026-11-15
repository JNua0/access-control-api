using AccessControl.SharedKernel;
using System.Text.RegularExpressions;

namespace AccessControl.Modules.Organization.Domain.Offices;

public sealed class Office
{
    public const int NameMaxLength = 60;

    private static readonly Regex _multipleSpaces = new(@"\s+", RegexOptions.Compiled);

    public int Id { get; private set; }
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public bool IsActive { get; private set; }

    private Office(string name, Address address)
    {
        Name = name;
        Address = address;
        IsActive = true;
    }

    public static Result<Office> Create(string? name, Address address)
    {
        ArgumentNullException.ThrowIfNull(address); // En este punto recibir un address null es un bug, ya que no deberia ser null

        Result<string> nameResult = ValidateName(name);

        if (nameResult.IsFailure)
            return Result.Failure<Office>(nameResult.Error);

        return Result.Success(new Office(nameResult.Value, address));
    }

    public Result Rename(string? name)
    {
        Result<string> nameResult = ValidateName(name);

        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Error);

        Name = nameResult.Value;

        return Result.Success();
    }

    public void ChangeAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);

        Address = address;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private static Result<string> ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<string>(OfficeErrors.NameRequired);

        string cleanName = _multipleSpaces.Replace(name.Normalize().Trim(), " ");

        if (cleanName.Length > NameMaxLength)
            return Result.Failure<string>(OfficeErrors.NameTooLong);

        return Result.Success(cleanName);
    }
}
