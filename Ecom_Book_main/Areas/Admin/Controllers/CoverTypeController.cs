using Ecom_Book_main_DataAccess.Repository.IRepository;
using Ecom_Book_main_Models;
using Ecom_Book_main_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecom_Book_main.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
    
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _iunitOfWork;
        public CoverTypeController(IUnitOfWork iunitOfWork)
        {
            _iunitOfWork= iunitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region API's
        public IActionResult GetAll()
        {
            return Json(new {data=_iunitOfWork.CoverType.GetAll()});    
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var covertypeInDb = _iunitOfWork.CoverType.Get(id);
            if (covertypeInDb == null) return Json(new { success = false, message = "Something Went Wrong" });
            _iunitOfWork.CoverType.Remove(covertypeInDb);
            _iunitOfWork.Save();
            return Json(new { success = true, message = "Deleted Successfully" });
        }
        #endregion
        public IActionResult Upsert(int? id)
        {
            CoverType coverType=new CoverType();
            if (id == null) return View(coverType);

            coverType = _iunitOfWork.CoverType.Get(id.GetValueOrDefault());
            if(coverType == null) return NotFound();
            return View(coverType);
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            if (coverType == null) return NotFound();
            if (!ModelState.IsValid) return View(coverType);
            if (coverType.Id == 0)
                _iunitOfWork.CoverType.add(coverType);
            else
                _iunitOfWork.CoverType.update(coverType);
            _iunitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
    }
}
