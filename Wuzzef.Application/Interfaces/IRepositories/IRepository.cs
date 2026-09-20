using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Wuzzef.Application.Interfaces.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task CreateAsync(T entity, CancellationToken cancellationToken = default);
        void Update(T entity);
        void Delete(T entity);
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? expression = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? page = null,
            int? pageSize = null,
            bool tracked = true,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            Expression<Func<T, bool>>? expression = null,
            CancellationToken cancellationToken = default);


        Task<T?> GetOneAsync(
       Expression<Func<T, bool>>? expression = null,
       Func<IQueryable<T>, IQueryable<T>>? include = null,
       bool tracked = true,
       CancellationToken cancellationToken = default);
    }
}
