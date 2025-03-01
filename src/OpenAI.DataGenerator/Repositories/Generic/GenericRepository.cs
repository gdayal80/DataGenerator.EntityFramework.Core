namespace OpenAI.DataGenerator.Repositories.Generic
{
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

        public virtual void Insert(K entity)
        {
            _dbSet.Add(entity);
        }
    }
}