using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.WorkSchedules;

public static class WorkScheduleDayErrors
{
    public static readonly Error DurationTooShort = Error.Validation(
        "WorkScheduleDay.DurationInvalid",
        $"La duración de la jornada debe ser al menos {WorkScheduleDay.MinDuration.TotalHours} hora.");

    public static readonly Error DurationTooLong = Error.Validation(
        "WorkScheduleDay.MaxDurationInvalid",
        $"La duración de la jornada debe ser como máximo {WorkScheduleDay.MaxDuration.TotalHours} horas.");

    public static readonly Error DayInvalid = Error.Validation(
        "WorkScheduleDay.DayInvalid",
        "El día de la semana ingresado es inválido.");
}
