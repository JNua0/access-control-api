using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.WorkSchedules;

public sealed record WorkScheduleDay
{
    public static readonly TimeSpan MinDuration = TimeSpan.FromHours(1);
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(16);

    private WorkScheduleDay(
        DayOfWeek day,
        TimeOnly startTime,
        TimeSpan workdayDuration)
    {
        Day = day;
        StartTime = startTime;
        WorkdayDuration = workdayDuration;
    }

    public static Result<WorkScheduleDay> Create(
        DayOfWeek day,
        TimeOnly startTime,
        TimeSpan workdayDuration)
    {
        if (!Enum.IsDefined(day))
            return Result.Failure<WorkScheduleDay>(WorkScheduleDayErrors.DayInvalid);

        if (workdayDuration < MinDuration)
            return Result.Failure<WorkScheduleDay>(WorkScheduleDayErrors.DurationTooShort);

        if (workdayDuration > MaxDuration)
            return Result.Failure<WorkScheduleDay>(WorkScheduleDayErrors.DurationTooLong);

        return Result.Success(
                new WorkScheduleDay(day, startTime, workdayDuration)
            );
    }

    public DayOfWeek Day { get; }
    public TimeOnly StartTime { get; }
    public TimeSpan WorkdayDuration { get; }
    public TimeOnly EndTime => StartTime.Add(WorkdayDuration);
}
