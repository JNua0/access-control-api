using AccessControl.Modules.Organization.Domain.WorkSchedules;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Tests.WorkSchedules;

public class WorkScheduleDayTests
{
    // ---------- Casos válidos ----------

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Act
        Result<WorkScheduleDay> result = WorkScheduleDay.Create(
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            TimeSpan.FromHours(9));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DayOfWeek.Monday, result.Value.Day);
        Assert.Equal(new TimeOnly(9, 0), result.Value.StartTime);
        Assert.Equal(TimeSpan.FromHours(9), result.Value.WorkdayDuration);
        Assert.Equal(new TimeOnly(18, 0), result.Value.EndTime);
    }

    [Theory]
    [InlineData(DayOfWeek.Sunday)]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    public void Create_WithEachDayOfWeek_ReturnsSuccess(DayOfWeek day)
    {
        // Act
        Result<WorkScheduleDay> result = WorkScheduleDay.Create(day, new TimeOnly(9, 0), TimeSpan.FromHours(8));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(day, result.Value.Day);
    }

    // ---------- Hora de salida (EndTime) ----------

    [Theory]
    [InlineData(9, 0, 9, 18, 0)]     // 09:00 + 9 h = 18:00
    [InlineData(22, 0, 8, 6, 0)]     // Turno nocturno: 22:00 + 8 h = 06:00 del día siguiente
    [InlineData(23, 30, 1, 0, 30)]   // Cruza la medianoche: 23:30 + 1 h = 00:30
    [InlineData(0, 0, 16, 16, 0)]    // Desde medianoche, duración máxima
    public void EndTime_IsStartTimePlusDuration(
        int startHour,
        int startMinute,
        int durationHours,
        int expectedEndHour,
        int expectedEndMinute)
    {
        // Act
        WorkScheduleDay day = WorkScheduleDay.Create(
            DayOfWeek.Monday,
            new TimeOnly(startHour, startMinute),
            TimeSpan.FromHours(durationHours)).Value;

        // Assert
        Assert.Equal(new TimeOnly(expectedEndHour, expectedEndMinute), day.EndTime);
    }

    // ---------- Duración (pruebas de límite) ----------

    [Theory]
    [InlineData(60)]    // Exactamente 1 hora (mínimo)
    [InlineData(960)]   // Exactamente 16 horas (máximo)
    public void Create_WithDurationAtLimits_ReturnsSuccess(int durationMinutes)
    {
        // Act
        Result<WorkScheduleDay> result = WorkScheduleDay.Create(
            DayOfWeek.Monday,
            new TimeOnly(6, 0),
            TimeSpan.FromMinutes(durationMinutes));

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(59)]    // Un minuto menos del mínimo
    [InlineData(0)]
    [InlineData(-60)]   // Duración negativa
    public void Create_WithDurationTooShort_ReturnsDurationTooShort(int durationMinutes)
    {
        // Act
        Result<WorkScheduleDay> result = WorkScheduleDay.Create(
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            TimeSpan.FromMinutes(durationMinutes));

        // Assert
        Assert.Equal(WorkScheduleDayErrors.DurationTooShort, result.Error);
    }

    [Theory]
    [InlineData(961)]    // Un minuto más del máximo
    [InlineData(1440)]   // 24 horas
    public void Create_WithDurationTooLong_ReturnsDurationTooLong(int durationMinutes)
    {
        // Act
        Result<WorkScheduleDay> result = WorkScheduleDay.Create(
            DayOfWeek.Monday,
            new TimeOnly(9, 0),
            TimeSpan.FromMinutes(durationMinutes));

        // Assert
        Assert.Equal(WorkScheduleDayErrors.DurationTooLong, result.Error);
    }

    // ---------- Día inválido ----------

    [Theory]
    [InlineData(7)]
    [InlineData(-1)]
    [InlineData(42)]
    public void Create_WithUndefinedDay_ReturnsDayInvalid(int dayValue)
    {
        // Arrange: un enum acepta números que no están en la lista (ver nota "Enum")
        var day = (DayOfWeek)dayValue;

        // Act
        Result<WorkScheduleDay> result = WorkScheduleDay.Create(day, new TimeOnly(9, 0), TimeSpan.FromHours(8));

        // Assert
        Assert.Equal(WorkScheduleDayErrors.DayInvalid, result.Error);
    }

    // ---------- Igualdad ----------

    [Fact]
    public void Create_WithSameData_ReturnsEqualDays()
    {
        // Act
        WorkScheduleDay first = WorkScheduleDay.Create(DayOfWeek.Monday, new TimeOnly(9, 0), TimeSpan.FromHours(9)).Value;
        WorkScheduleDay second = WorkScheduleDay.Create(DayOfWeek.Monday, new TimeOnly(9, 0), TimeSpan.FromHours(9)).Value;

        // Assert: es un record, así que se compara por valor
        Assert.Equal(first, second);
    }
}
