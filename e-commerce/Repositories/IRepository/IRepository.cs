using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Cinema.Repositories.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<T> CreateAsync(T entity, CancellationToken cancellationToken);


        Task<IEnumerable<T>> GetAsync(
        Expression<Func<T, bool>>? expression = null,
        Expression<Func<T, object>>[]? include = null,
        bool tracked = true,
        CancellationToken cancellationToken = default);


        void Update(T entity);


        void Delete(T entity);

        Task<T?> GetOne(
     Expression<Func<T, bool>>? expression = null,
     Expression<Func<T, object>>[]? include = null,
     bool tracked = true,
     CancellationToken cancellationToken = default);





        Task<int> CommitAsync(CancellationToken cancellationToken = default);

        Task<T?> GetOneAsync(
          Expression<Func<T, bool>>? expression = null,
          Expression<Func<T, object>>[]? includes = null,
          bool tracked = true,
          CancellationToken cancellationToken = default);
    }
}
