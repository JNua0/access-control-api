using AccessControl.Modules.Organization.Application.Abstractions.Persistence;
using AccessControl.Modules.Organization.Domain.Departments;
using AccessControl.SharedKernel;

namespace AccessControl.Modules.Organization.Application.UseCases.Departments.Create;

public sealed class CreateDepartmentHandler
{
    private readonly IDepartmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentHandler(
        IDepartmentRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> HandleAsync(CreateDepartmentCommand command)
    {
        Result<Department> departmentResult = Department.Create(command.Name);

        if (departmentResult.IsFailure)
            return Result.Failure<int>(departmentResult.Error);

        Department department = departmentResult.Value;

        if (await _repository.ExistsByNameAsync(department.Name))
            return Result.Failure<int>(DepartmentErrors.NameAlreadyExists);

        await _repository.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success(department.Id);
    }
}
