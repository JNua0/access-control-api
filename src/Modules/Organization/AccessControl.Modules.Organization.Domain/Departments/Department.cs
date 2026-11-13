using System.Text.RegularExpressions;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.Departments;

public sealed class Department
{
    public const int NameMaxLength = 60;

    private static readonly Regex _multipleSpaces = new(@"\s+", RegexOptions.Compiled);

    public int Id { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    private Department(string name)
    {
        Name = name;
        IsActive = true;
    }

    public static Result<Department> Create(string? name)
    {
        Result<string> nameResult = ValidateName(name);

        if (nameResult.IsFailure)
            return Result.Failure<Department>(nameResult.Error);

        return Result.Success(new Department(nameResult.Value));
    }

    public Result Rename(string? name)
    {
        Result<string> nameResult = ValidateName(name);

        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Error);

        Name = nameResult.Value;

        return Result.Success();
    }

    private static Result<string> ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<string>(DepartmentErrors.NameRequired);

        string cleanName = _multipleSpaces.Replace(name.Normalize().Trim(), " ");

        if (cleanName.Length > NameMaxLength)
            return Result.Failure<string>(DepartmentErrors.NameTooLong);

        return Result.Success(cleanName);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
