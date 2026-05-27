using OpenCBT.Domain.Entities;
using System.Linq.Expressions;

namespace OpenCBT.Domain.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}
