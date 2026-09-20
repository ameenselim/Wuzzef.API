using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Wuzzef.Application.Interfaces.IRepositories;
using Wuzzef.Infrastructure.Persistence;

namespace Wuzzef.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task CreateAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? page = null,
            int? pageSize = null,
            bool tracked = false,
            CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(expression, include, tracked);

            if (orderBy is not null)
                query = orderBy(query);

            if (page.HasValue && pageSize.HasValue)
            {
                query = query.Skip((page.Value - 1) * pageSize.Value)
                             .Take(pageSize.Value);
            }
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<T?> GetOneAsync(
           Expression<Func<T, bool>>? expression = null,
           Func<IQueryable<T>, IQueryable<T>>? include = null,
           bool tracked = false,
           CancellationToken cancellationToken = default)
        {
            return await BuildQuery(expression, include, tracked)
                .FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<int> CountAsync(
            Expression<Func<T, bool>>? expression = null,
            CancellationToken cancellationToken = default
            )
        {
            return await BuildQuery(expression).CountAsync(cancellationToken);
        }

        private IQueryable<T> BuildQuery(
            Expression<Func<T, bool>>? expression = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            bool tracked = false)
        {
            var query = _dbSet.AsQueryable();

            if (expression is not null)
            {
                query = query.Where(expression);
            }
            if (include != null)
            {
                query = include(query);
            }
            // tracking
            if (!tracked)
                query = query.AsNoTracking();

            return query;
        }
    }
}
