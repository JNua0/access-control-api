using AccessControl.Modules.Organization.Application.Abstractions.Persistence;

namespace AccessControl.Modules.Organization.Tests.Fakes;

// No guarda nada: solo cuenta cuántas veces se llamó a SaveChangesAsync
internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCount++;
        return Task.FromResult(1);
    }
}
