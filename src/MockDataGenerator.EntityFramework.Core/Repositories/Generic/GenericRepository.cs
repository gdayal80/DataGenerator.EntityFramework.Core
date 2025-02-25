namespace Repositories.Generic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data;
    using System.Linq.Expressions;
    using Microsoft.EntityFrameworkCore;

    public class GenericRepository<T, K> where T : DbContext where K : class
    {
        internal T _context;
        internal DbSet<K> _dbSet;

        public GenericRepository(T context)
        {
            _context = context;
            _dbSet = _context.Set<K>();
        }

        public virtual IEnumerable<K> Get(
            Expression<Func<K, bool>>? filter = null,
            Func<IQueryable<K>, IOrderedQueryable<K>>? orderBy = null,
            string includeProperties = "")
        {
            IQueryable<K> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public virtual K GetByID(object id)
        {
            return _dbSet.Find(id)!;
        }

        public virtual void Insert(K entity)
        {
            _dbSet.Add(entity);
        }

        public virtual void Delete(object id)
        {
            K entityToDelete = _dbSet.Find(id)!;
            Delete(entityToDelete);
        }

        public virtual void Delete(K entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
        }

        public virtual void Update(K entityToUpdate)
        {
            _dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
        }
    }
}