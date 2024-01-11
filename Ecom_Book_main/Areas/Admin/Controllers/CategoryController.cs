using Ecom_Book_main_DataAccess.Repository.IRepository;
using Ecom_Book_main_Models;
using Ecom_Book_main_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Ecom_Book_main.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]


    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region 
        [HttpGet]
        public IActionResult GetAll()
        {
            var categoryList = _unitOfWork.Category.GetAll();
            return Json(new { data = categoryList });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var categoryInDb = _unitOfWork.Category.Get(id);
            if (categoryInDb == null) return Json(new { success = false, message = "Something Went Wrong" });
            _unitOfWork.Category.Remove(categoryInDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted Successfully" });

        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            Category category=new Category();
            if (id == null) return View(category);

            category = _unitOfWork.Category.Get(id.GetValueOrDefault());
            if (category == null) return NotFound();
            return View(category);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(Category category)
        {
            if(category== null) return NotFound();
            if (!ModelState.IsValid) return View(category);
            if (category.Id == 0)
                _unitOfWork.Category.add(category);
            else
                _unitOfWork.Category.update(category);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
