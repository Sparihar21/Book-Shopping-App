    using Ecom_Book_main_DataAccess;
using Ecom_Book_main_DataAccess.Repository.IRepository;
using Ecom_Book_main_Models;
using Ecom_Book_main_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;

namespace Ecom_Book_main.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public UserController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region API's
        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _context.ApplicationUsers.ToList();
            var roles = _context.Roles.ToList();
            var userRoles = _context.UserRoles.ToList();
            foreach(var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(r => r.Id == roleId).Name;
                if (user.CompanyId != null)
                {
                    user.Company = new Company()
                    {
                        Name = _unitOfWork.Company.Get(Convert.ToInt32(user.CompanyId)).Name
                    };
                }
                if (user.CompanyId == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            var adminUsers = userList.FirstOrDefault(u => u.Role == SD.Role_Admin);
            userList.Remove(adminUsers);
            return Json(new {Data= userList});
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            bool isLocked = false;
            var userInDb=_context.ApplicationUsers.FirstOrDefault(u=>u.Id== id);
            if (userInDb == null) return Json(new { success=false, message = "Something Went Wrong" });
            if(userInDb!=null && userInDb.LockoutEnd > DateTime.Now)
            {
                userInDb.LockoutEnd=DateTime.Now;
                isLocked = false;
            }
            else
            {
                userInDb.LockoutEnd=DateTime.Now.AddYears(100);
                isLocked = true;
            }
            _context.SaveChanges();
            return Json(new { success = true, message = isLocked == true ? "User Successfuly Locked" : "User Successfully Unlocked" });
        }
        #endregion
    }
}
