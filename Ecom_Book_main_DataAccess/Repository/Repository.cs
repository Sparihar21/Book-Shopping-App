using Ecom_Book_main_DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom_Book_main_DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        public readonly ApplicationDbContext _context;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext context)
        {
            _context= context;
            dbSet = _context.Set<T>();
        }
        public void add(T Entity)
        {
            dbSet.Add(Entity);
        }

        public T FirstOrDefault(System.Linq.Expressions.Expression<Func<T, bool>> filter = null, string includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
                query = query.Where(filter);
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new[] { ',' },
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }

        public T Get(int Id)
        {
            return dbSet.Find(Id);
        }

        public IEnumerable<T> GetAll(System.Linq.Expressions.Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderby = null, string includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if(filter!=null)
                query = query.Where(filter);
            if(includeProperties!=null)
            {
                foreach(var includeProp in includeProperties.Split(new[]{','},
                    StringSplitOptions.RemoveEmptyEntries))
                {
                    query=query.Include(includeProp);
                }
            }
            if (orderby != null)
                return orderby(query).ToList();
            return query.ToList();
        }

        public void Remove(T Entity)
        {
            dbSet.Remove(Entity);
        }

        public void Remove(int Id)
        {
            dbSet.Remove(Get(Id));
        }

        public void RemoveRange(IEnumerable<T> values)
        {
            dbSet.RemoveRange(values);
        }

        public void update(T Entity)
        {
            _context.ChangeTracker.Clear();
            dbSet.Update(Entity);
        }
    }
}
