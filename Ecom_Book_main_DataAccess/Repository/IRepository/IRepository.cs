using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ecom_Book_main_DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        void add(T Entity);
        void update(T Entity);
        void Remove(T Entity);
        void Remove(int Id);
        void RemoveRange(IEnumerable<T> values);
        T Get(int Id);
        IEnumerable<T> GetAll(
            Expression<Func<T,bool>> filter=null,
            Func<IQueryable<T>,IOrderedQueryable<T>> orderby=null,
            string includeProperties=null
            );

        T FirstOrDefault(
            Expression<Func<T, bool>> filter = null,
            string includeProperties = null
            );
    }
}
