using Ecom_Book_main_DataAccess.Repository.IRepository;
using Ecom_Book_main_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom_Book_main_DataAccess.Repository
{
    public class OrderDetailRepository:Repository<OrderDetail>,IOrderDetailRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderDetailRepository(ApplicationDbContext context):base(context) 
        {
            _context = context;
        }

        public void RemoveRange(IEnumerable<ShoppingCart> listCart)
        {
            throw new NotImplementedException();
        }
    }
}
