using Ecom_Book_main_DataAccess;
using Ecom_Book_main_DataAccess.Repository.IRepository;
using Ecom_Book_main_Models;
using Ecom_Book_main_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecom_Book_main.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]


    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork= unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region API's
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.Company.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var companyInDb = _unitOfWork.Company.Get(id);
            if (companyInDb == null) return Json(new { success = false, message = "Something Went Wrong" });
            _unitOfWork.Company.Remove(companyInDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted Successfully" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            Company company = new Company();
            if (id == null) return View(company);

            company = _unitOfWork.Company.Get(id.GetValueOrDefault());
            if (company == null) return NotFound();
            return View(company);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (company == null) return BadRequest();
            if (!ModelState.IsValid) return View(company);
            if (company.Id == 0)
                _unitOfWork.Company.add(company);
            else
                _unitOfWork.Company.update(company);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
