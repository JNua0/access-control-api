using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.Employees;

public sealed class Employee
{
    public const int PhotoPathMaxLength = 200;

    public int Id { get; private set; }
    public EmployeeNumber Number { get; private set; }
    public int OfficeId { get; private set; }
    public int DepartmentId { get; private set; }
    public int? WorkScheduleId { get; private set; }
    public PersonName Name { get; private set; }
    public string? PhotoPath { get; private set; }
    public bool IsActive { get; private set; }
    public DateOnly? HiredAt { get; private set; }

    private Employee(
        EmployeeNumber number,
        int officeId,
        int departmentId,
        PersonName name,
        DateOnly? hiredAt)
    {
        Number = number;
        OfficeId = officeId;
        DepartmentId = departmentId;
        Name = name;
        IsActive = true;
        HiredAt = hiredAt;
    }

    public static Result<Employee> Create(
        EmployeeNumber number,
        int officeId,
        int departmentId,
        PersonName name,
        DateOnly? hiredAt)
    {
        ArgumentNullException.ThrowIfNull(number);
        ArgumentNullException.ThrowIfNull(name);

        Result<int> officeIdResult = ValidateOfficeId(officeId);

        if (officeIdResult.IsFailure)
            return Result.Failure<Employee>(officeIdResult.Error);

        Result<int> departmentIdResult = ValidateDepartmentId(departmentId);

        if (departmentIdResult.IsFailure)
            return Result.Failure<Employee>(departmentIdResult.Error);

        return Result.Success(
                new Employee(
                    number,
                    officeIdResult.Value,
                    departmentIdResult.Value,
                    name,
                    hiredAt)
            );
    }

    public void Rename(PersonName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
    }

    public Result ChangeOffice(int officeId)
    {
        Result<int> officeIdResult = ValidateOfficeId(officeId);

        if (officeIdResult.IsFailure)
            return Result.Failure(officeIdResult.Error);

        OfficeId = officeIdResult.Value;

        return Result.Success();
    }

    public Result ChangeDepartment(int departmentId)
    {
        Result<int> departmentIdResult = ValidateDepartmentId(departmentId);

        if (departmentIdResult.IsFailure)
            return Result.Failure(departmentIdResult.Error);

        DepartmentId = departmentIdResult.Value;

        return Result.Success();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public Result ChangePhoto(string photoPath)
    {
        if (string.IsNullOrWhiteSpace(photoPath))
            return Result.Failure(EmployeeErrors.PhotoPathRequired);

        Result<string> photoPathResult = ValidatePhotoPath(photoPath);

        if (photoPathResult.IsFailure)
            return Result.Failure(photoPathResult.Error);

        PhotoPath = photoPathResult.Value;

        return Result.Success();
    }

    public void RemovePhoto()
    {
        PhotoPath = null;
    }

    public void ChangeHiredAt(DateOnly? hiredAt)
    {
        HiredAt = hiredAt;
    }

    public Result AssignWorkSchedule(int workScheduleId)
    {
        Result<int> workScheduleIdResult = ValidateWorkScheduleId(workScheduleId);

        if (workScheduleIdResult.IsFailure)
            return Result.Failure(workScheduleIdResult.Error);

        WorkScheduleId = workScheduleIdResult.Value;

        return Result.Success();
    }

    private static Result<int> ValidateOfficeId(int officeId)
    {
        if (officeId <= 0)
            return Result.Failure<int>(EmployeeErrors.OfficeIdInvalid);

        return Result.Success(officeId);
    }

    private static Result<int> ValidateDepartmentId(int departmentId)
    {
        if (departmentId <= 0)
            return Result.Failure<int>(EmployeeErrors.DepartmentIdInvalid);

        return Result.Success(departmentId);
    }

    private static Result<int> ValidateWorkScheduleId(int workScheduleId)
    {
        if (workScheduleId <= 0)
            return Result.Failure<int>(EmployeeErrors.WorkScheduleIdInvalid);

        return Result.Success(workScheduleId);
    }

    private static Result<string> ValidatePhotoPath(string photoPath)
    {
        if (photoPath.Length > PhotoPathMaxLength)
            return Result.Failure<string>(EmployeeErrors.PhotoPathTooLong);

        return Result.Success(photoPath);
    }
}