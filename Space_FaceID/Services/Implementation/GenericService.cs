using Space_FaceID.Repositories.Interfaces;
using Space_FaceID.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Services.Implementation
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        protected readonly IGenericRepository<T> _repository;

        public GenericService(IGenericRepository<T> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public virtual async Task<T?> GetByIdAsync(object id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _repository.FindAsync(predicate);
        }

        public virtual async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _repository.SingleOrDefaultAsync(predicate);
        }

        public virtual async Task<int> AddAsync(T entity)
        {
            return await _repository.AddAsync(entity);
        }

        public virtual async Task<int> AddRangeAsync(IEnumerable<T> entities)
        {
            return await _repository.AddRangeAsync(entities);
        }

        public virtual async Task<int> UpdateAsync(T entity)
        {
            return await _repository.UpdateAsync(entity);
        }

        public virtual async Task<int> UpdateRangeAsync(T entities)
        {
            return await _repository.UpdateRangeAsync(entities);
        }

        public virtual async Task<int> RemoveAsync(T entity)
        {
            return await _repository.RemoveAsync(entity);
        }

        public virtual async Task<int> RemoveRangeAsync(T entities)
        {
            return await _repository.RemoveRangeAsync(entities);
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _repository.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync()
        {
            return await _repository.CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _repository.CountAsync(predicate);
        }
    }
}
