using System.Linq.Expressions;
using STLMS.Domain.Common;
using STLMS.Domain.Interfaces;

namespace STLMS.Application.Tests.TestDoubles;

/// <summary>In-memory IRepository/IUnitOfWork for handler tests - Moq can't usefully mock
/// Query() (IQueryable) chains, so a small real in-memory backing list is more honest than trying
/// to stub every possible LINQ expression.</summary>
public class FakeRepository<T> : IRepository<T> where T : BaseEntity
{
    public readonly List<T> Items = [];

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Items.SingleOrDefault(x => x.Id == id));

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<T>>(Items.ToList());

    public Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<T>>(Items.AsQueryable().Where(predicate).ToList());

    public Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        Task.FromResult(Items.AsQueryable().Where(predicate).SingleOrDefault());

    public IQueryable<T> Query() => Items.AsQueryable();

    public Task AddAsync(T entity, CancellationToken ct = default)
    {
        Items.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(T entity)
    {
        var index = Items.FindIndex(x => x.Id == entity.Id);
        if (index >= 0) Items[index] = entity;
    }

    public void Remove(T entity) => Items.RemoveAll(x => x.Id == entity.Id);
}

public class FakeUnitOfWork : IUnitOfWork
{
    private readonly Dictionary<Type, object> _repositories = [];

    public FakeRepository<T> FakeRepository<T>() where T : BaseEntity
    {
        if (!_repositories.TryGetValue(typeof(T), out var repo))
        {
            repo = new FakeRepository<T>();
            _repositories[typeof(T)] = repo;
        }
        return (FakeRepository<T>)repo;
    }

    public IRepository<T> Repository<T>() where T : BaseEntity => FakeRepository<T>();

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);
    public Task BeginTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task CommitTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task RollbackTransactionAsync(CancellationToken ct = default) => Task.CompletedTask;
    public void Dispose() { }
}
