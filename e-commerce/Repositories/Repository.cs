using Cinema.Data;
using Cinema.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Cinema.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private ApplicationDbContext _context;// = new();

        private DbSet<T> _dbset;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbset = _context.Set<T>();
        }

        

        public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken)
        {
            var entities = await _dbset.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return entities.Entity;
        }

        public async Task<IEnumerable<T>> GetAsync(
          Expression<Func<T, bool>>? expression = null,
          Expression<Func<T, object>>[]? include = null,
          bool tracked = true,
          CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbset;

            if (expression is not null)
                query = query.Where(expression);

            if (include is not null)
            {
                foreach (var includeExpression in include)
                {
                    query = query.Include(includeExpression);
                }
            }

            if (!tracked)
                query = query.AsNoTracking();

            return await query.ToListAsync(cancellationToken);
        }


        public async Task<T?> GetOne(
        Expression<Func<T, bool>>? expression = null,
        Expression<Func<T, object>> []? include = null,
        bool tracked = true,
        CancellationToken cancellationToken = default)
        {
            var result = await GetAsync(expression, include, tracked, cancellationToken);
            return result.FirstOrDefault();
        }


        public void Update(T entity)
        {
            _dbset.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbset.Remove(entity);
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 0;
            }
        }
        public async Task<T?> GetOneAsync(
          Expression<Func<T, bool>>? expression = null,
          Expression<Func<T, object>>[]? includes = null,
          bool tracked = true,
          CancellationToken cancellationToken = default)
        {
            return (await GetAsync(expression, includes, tracked, cancellationToken)).FirstOrDefault();
        }

    }
}
