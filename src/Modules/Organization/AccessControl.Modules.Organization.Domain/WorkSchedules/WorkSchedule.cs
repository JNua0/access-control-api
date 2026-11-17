using AccessControl.SharedKernel;
using System.Text.RegularExpressions;

namespace AccessControl.Modules.Organization.Domain.WorkSchedules;

public sealed class WorkSchedule
{
    public const int NameMinLength = 1;
    public const int NameMaxLength = 40;

    private static readonly Regex _multipleSpaces = new(@"\s+", RegexOptions.Compiled);
    private readonly List<WorkScheduleDay> _days = new();

    private WorkSchedule(string name)
    {
        Name = name;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public IReadOnlyCollection<WorkScheduleDay> Days => _days.AsReadOnly();

    public static Result<WorkSchedule> Create(string? name)
    {
        Result<string> nameResult = ValidateName(name);

        if (nameResult.IsFailure)
            return Result.Failure<WorkSchedule>(nameResult.Error);

        return Result.Success(new WorkSchedule(nameResult.Value));
    }

    public Result Rename(string? name)
    {
        Result<string> nameResult = ValidateName(name);

        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Error);

        Name = nameResult.Value;

        return Result.Success();
    }

    public Result SetDay(DayOfWeek day, TimeOnly startTime, TimeSpan workdayDuration)
    {
        Result<WorkScheduleDay> dayResult = WorkScheduleDay.Create(day, startTime, workdayDuration);

        if (dayResult.IsFailure)
            return Result.Failure(dayResult.Error);

        int index = _days.FindIndex(d => d.Day == dayResult.Value.Day);

        if (index >= 0)
            _days[index] = dayResult.Value;
        else
            _days.Add(dayResult.Value);

        return Result.Success();
    }

    public Result RemoveDay(DayOfWeek day)
    {
        if (!Enum.IsDefined(day))
            return Result.Failure(WorkScheduleDayErrors.DayInvalid);

        int index = _days.FindIndex(d => d.Day == day);

        if (index >= 0)
            _days.RemoveAt(index);
        else
            return Result.Failure(WorkScheduleErrors.DayNotExist);

        return Result.Success();
    }

    private static Result<string> ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<string>(WorkScheduleErrors.NameRequired);

        string cleanName = name.Normalize().Trim();
        cleanName = _multipleSpaces.Replace(cleanName, " ");

        if (cleanName.Length < NameMinLength)
            return Result.Failure<string>(WorkScheduleErrors.NameTooShort);

        if (cleanName.Length > NameMaxLength)
            return Result.Failure<string>(WorkScheduleErrors.NameTooLong);

        return Result.Success(cleanName);
    }
}
