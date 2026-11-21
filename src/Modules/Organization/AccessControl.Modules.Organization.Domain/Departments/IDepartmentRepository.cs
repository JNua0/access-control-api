namespace AccessControl.Modules.Organization.Domain.Departments;

public interface IDepartmentRepository
{
    Task AddAsync(Department department);
    Task<Department?> GetByIdAsync(int id);
    Task<bool> ExistsByNameAsync(string name);
    Task<IReadOnlyList<Department>> GetActiveAsync();
}
