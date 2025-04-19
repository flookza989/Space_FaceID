using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Space_FaceID.Data.Context;
using Space_FaceID.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Space_FaceID.Repositories.Implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly IDbContextFactory<FaceIDDbContext> _contextFactory;

        public GenericRepository(IDbContextFactory<FaceIDDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(object id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().SingleOrDefaultAsync(predicate);
        }

        public virtual async Task<int> AddAsync(T entity)
        {
            using var context = _contextFactory.CreateDbContext();
            await context.Set<T>().AddAsync(entity);
            return await context.SaveChangesAsync();
        }

        public virtual async Task<int> AddRangeAsync(IEnumerable<T> entities)
        {
            using var context = _contextFactory.CreateDbContext();
            await context.Set<T>().AddRangeAsync(entities);
            return await context.SaveChangesAsync();
        }

        public virtual async Task<int> UpdateAsync(T entity)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Set<T>().Update(entity);
            return await context.SaveChangesAsync();
        }

        public virtual async Task<int> UpdateRangeAsync(T entities)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Set<T>().UpdateRange(entities);
            return await context.SaveChangesAsync();
        }

        public virtual async Task<int> RemoveAsync(T entity)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Set<T>().Remove(entity);
            return await context.SaveChangesAsync();
        }

        public virtual async Task<int> RemoveRangeAsync(T entities)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Set<T>().RemoveRange(entities);
            return await context.SaveChangesAsync();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Set<T>().CountAsync(predicate);
        }
    }
}
