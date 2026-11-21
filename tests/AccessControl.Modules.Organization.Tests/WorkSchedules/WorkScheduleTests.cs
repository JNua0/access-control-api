using AccessControl.Modules.Organization.Domain.WorkSchedules;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Tests.WorkSchedules;

public class WorkScheduleTests
{
    private static readonly TimeOnly NineAm = new(9, 0);
    private static readonly TimeSpan NineHours = TimeSpan.FromHours(9);

    private static WorkSchedule CreateSchedule() => WorkSchedule.Create("Oficina 9 a 18").Value;

    // ---------- Create ----------

    [Fact]
    public void Create_WithValidName_ReturnsScheduleWithoutDays()
    {
        // Act
        Result<WorkSchedule> result = WorkSchedule.Create("Oficina 9 a 18");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Oficina 9 a 18", result.Value.Name);
        Assert.Empty(result.Value.Days);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutName_ReturnsNameRequired(string? name)
    {
        // Act
        Result<WorkSchedule> result = WorkSchedule.Create(name);

        // Assert
        Assert.Equal(WorkScheduleErrors.NameRequired, result.Error);
    }

    [Theory]
    [InlineData("  Turno Nocturno  ", "Turno Nocturno")]
    [InlineData("Turno     Nocturno", "Turno Nocturno")]
    [InlineData("Turno\tNocturno\n", "Turno Nocturno")]
    public void Create_WithExtraSpaces_CleansName(string name, string expectedName)
    {
        // Act
        Result<WorkSchedule> result = WorkSchedule.Create(name);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedName, result.Value.Name);
    }

    [Fact]
    public void Create_WithNameAtMaxLength_ReturnsSuccess()
    {
        // Act: exactamente 40 caracteres
        Result<WorkSchedule> result = WorkSchedule.Create(new string('a', WorkSchedule.NameMaxLength));

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithNameOverMaxLength_ReturnsNameTooLong()
    {
        // Act: 41 caracteres
        Result<WorkSchedule> result = WorkSchedule.Create(new string('a', WorkSchedule.NameMaxLength + 1));

        // Assert
        Assert.Equal(WorkScheduleErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Create_WithSurroundingSpacesAndMaxLength_ReturnsSuccess()
    {
        // Arrange: la longitud se mide después de limpiar
        string name = "   " + new string('a', WorkSchedule.NameMaxLength) + "   ";

        // Act
        Result<WorkSchedule> result = WorkSchedule.Create(name);

        // Assert
        Assert.True(result.IsSuccess);
    }

    // ---------- Rename ----------

    [Fact]
    public void Rename_WithValidName_ChangesName()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();

        // Act
        Result result = schedule.Rename("  Oficina   8 a 17  ");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Oficina 8 a 17", schedule.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithoutName_ReturnsNameRequiredAndKeepsName(string? newName)
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();

        // Act
        Result result = schedule.Rename(newName);

        // Assert
        Assert.Equal(WorkScheduleErrors.NameRequired, result.Error);
        Assert.Equal("Oficina 9 a 18", schedule.Name);
    }

    [Fact]
    public void Rename_DoesNotChangeDays()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();
        schedule.SetDay(DayOfWeek.Monday, NineAm, NineHours);

        // Act
        schedule.Rename("Oficina 8 a 17");

        // Assert
        Assert.Single(schedule.Days);
    }

    // ---------- SetDay ----------

    [Fact]
    public void SetDay_NewDay_AddsDay()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();

        // Act
        Result result = schedule.SetDay(DayOfWeek.Monday, NineAm, NineHours);

        // Assert
        Assert.True(result.IsSuccess);
        WorkScheduleDay day = Assert.Single(schedule.Days);   // Revisa que haya uno y lo devuelve
        Assert.Equal(DayOfWeek.Monday, day.Day);
        Assert.Equal(NineAm, day.StartTime);
        Assert.Equal(NineHours, day.WorkdayDuration);
    }

    [Fact]
    public void SetDay_FiveDifferentDays_AddsAll()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();
        DayOfWeek[] weekdays = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

        // Act
        foreach (DayOfWeek weekday in weekdays)
            schedule.SetDay(weekday, NineAm, NineHours);

        // Assert
        Assert.Equal(5, schedule.Days.Count);
    }

    [Fact]
    public void SetDay_ExistingDay_ReplacesItInsteadOfAddingAnother()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();
        schedule.SetDay(DayOfWeek.Friday, NineAm, NineHours);

        // Act: el viernes ahora sale temprano
        Result result = schedule.SetDay(DayOfWeek.Friday, NineAm, TimeSpan.FromHours(5));

        // Assert: sigue habiendo un solo viernes, con la nueva duración
        Assert.True(result.IsSuccess);
        WorkScheduleDay friday = Assert.Single(schedule.Days);
        Assert.Equal(TimeSpan.FromHours(5), friday.WorkdayDuration);
        Assert.Equal(new TimeOnly(14, 0), friday.EndTime);
    }

    [Fact]
    public void SetDay_WithInvalidDuration_ReturnsErrorAndDoesNotAddDay()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();

        // Act
        Result result = schedule.SetDay(DayOfWeek.Monday, NineAm, TimeSpan.FromHours(20));

        // Assert
        Assert.Equal(WorkScheduleDayErrors.DurationTooLong, result.Error);
        Assert.Empty(schedule.Days);
    }

    [Fact]
    public void SetDay_WithInvalidDuration_KeepsPreviousDay()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();
        schedule.SetDay(DayOfWeek.Monday, NineAm, NineHours);

        // Act: intento fallido de reemplazar el lunes
        Result result = schedule.SetDay(DayOfWeek.Monday, NineAm, TimeSpan.Zero);

        // Assert: el lunes anterior se conserva sin cambios
        Assert.Equal(WorkScheduleDayErrors.DurationTooShort, result.Error);
        WorkScheduleDay monday = Assert.Single(schedule.Days);
        Assert.Equal(NineHours, monday.WorkdayDuration);
    }

    [Fact]
    public void SetDay_WithUndefinedDay_ReturnsDayInvalid()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();

        // Act
        Result result = schedule.SetDay((DayOfWeek)9, NineAm, NineHours);

        // Assert
        Assert.Equal(WorkScheduleDayErrors.DayInvalid, result.Error);
        Assert.Empty(schedule.Days);
    }

    // ---------- RemoveDay ----------

    [Fact]
    public void RemoveDay_ExistingDay_RemovesOnlyThatDay()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();
        schedule.SetDay(DayOfWeek.Monday, NineAm, NineHours);
        schedule.SetDay(DayOfWeek.Tuesday, NineAm, NineHours);

        // Act
        Result result = schedule.RemoveDay(DayOfWeek.Monday);

        // Assert
        Assert.True(result.IsSuccess);
        WorkScheduleDay remaining = Assert.Single(schedule.Days);
        Assert.Equal(DayOfWeek.Tuesday, remaining.Day);
    }

    [Fact]
    public void RemoveDay_DayNotInSchedule_ReturnsDayNotExist()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();
        schedule.SetDay(DayOfWeek.Monday, NineAm, NineHours);

        // Act
        Result result = schedule.RemoveDay(DayOfWeek.Sunday);

        // Assert
        Assert.Equal(WorkScheduleErrors.DayNotExist, result.Error);
        Assert.Single(schedule.Days);
    }

    [Fact]
    public void RemoveDay_WithUndefinedDay_ReturnsDayInvalid()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();

        // Act
        Result result = schedule.RemoveDay((DayOfWeek)9);

        // Assert
        Assert.Equal(WorkScheduleDayErrors.DayInvalid, result.Error);
    }

    [Fact]
    public void RemoveDay_LastDay_LeavesScheduleWithoutDays()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();
        schedule.SetDay(DayOfWeek.Monday, NineAm, NineHours);

        // Act: hoy se permite dejar un horario sin días. Si decides lo contrario, esta prueba debe cambiar.
        Result result = schedule.RemoveDay(DayOfWeek.Monday);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(schedule.Days);
    }

    // ---------- La colección está protegida ----------

    [Fact]
    public void Days_CannotBeModifiedFromOutside()
    {
        // Arrange
        WorkSchedule schedule = CreateSchedule();
        WorkScheduleDay day = WorkScheduleDay.Create(DayOfWeek.Monday, NineAm, NineHours).Value;

        // Act: aunque alguien convierta Days a una colección modificable, agregar falla
        var collection = (ICollection<WorkScheduleDay>)schedule.Days;

        // Assert
        Assert.Throws<NotSupportedException>(() => collection.Add(day));
        Assert.Empty(schedule.Days);
    }

    // ---------- Identidad ----------

    [Fact]
    public void Create_TwiceWithSameName_ReturnsDifferentSchedules()
    {
        // Act
        WorkSchedule first = CreateSchedule();
        WorkSchedule second = CreateSchedule();

        // Assert: es una entidad; se distingue por su Id, no por sus datos
        Assert.NotEqual(first, second);
    }
}
