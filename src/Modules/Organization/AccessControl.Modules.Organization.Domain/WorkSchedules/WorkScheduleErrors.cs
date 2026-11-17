using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Domain.WorkSchedules;

public static class WorkScheduleErrors
{
    public static readonly Error NameRequired = Error.Validation(
        "WorkSchedule.NameRequired",
        "El nombre del horario es obligatorio.");

    public static readonly Error NameTooShort = Error.Validation(
        "WorkSchedule.NameTooShort",
        $"El nombre del horario debe tener al menos {WorkSchedule.NameMinLength} caracteres.");

    public static readonly Error NameTooLong = Error.Validation(
        "WorkSchedule.NameTooLong",
        $"El nombre del horario no debe exceder los {WorkSchedule.NameMaxLength} caracteres.");

    public static readonly Error DayNotExist = Error.NotFound(
        "WorkSchedule.DayNotExist",
        "El horario no tiene registrado ese día.");
}
