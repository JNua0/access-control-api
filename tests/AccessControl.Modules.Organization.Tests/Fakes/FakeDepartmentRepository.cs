using AccessControl.Modules.Organization.Domain.Departments;

namespace AccessControl.Modules.Organization.Tests.Fakes;

// Repositorio en memoria: guarda los departamentos en una lista en lugar de la base de datos
internal sealed class FakeDepartmentRepository : IDepartmentRepository
{
    private readonly List<Department> _departments = new();

    // Permite empezar la prueba con departamentos que "ya existían" en la base
    public FakeDepartmentRepository(params string[] existingNames)
    {
        foreach (string name in existingNames)
            _departments.Add(Department.Create(name).Value);
    }

    // Las pruebas leen esta lista para revisar qué se agregó
    public IReadOnlyList<Department> Departments => _departments;

    public Task AddAsync(Department department)
    {
        _departments.Add(department);
        return Task.CompletedTask;
    }

    public Task<Department?> GetByIdAsync(int id)
        => Task.FromResult(_departments.FirstOrDefault(department => department.Id == id));

    // Comparación exacta, como el índice único de la base
    public Task<bool> ExistsByNameAsync(string name)
        => Task.FromResult(_departments.Any(department => department.Name == name));

    public Task<IReadOnlyList<Department>> GetActiveAsync()
    {
        IReadOnlyList<Department> active = _departments
            .Where(department => department.IsActive)
            .ToList();

        return Task.FromResult(active);
    }
}
