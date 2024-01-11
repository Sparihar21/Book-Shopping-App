using Ecom_Book_main_DataAccess.Repository.IRepository;
using Ecom_Book_main_Models;
using Ecom_Book_main_Models.ViewModels;
using Ecom_Book_main_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Ecom_Book_main.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region Api's
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.Product.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var productInDb = _unitOfWork.Product.Get(id);
            if (productInDb == null) return Json(new { success = false, message = "Something Went Wrong!!" });
            _unitOfWork.Product.Remove(productInDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted Successfully !!" });

        }
        #endregion
        //return Json(new { success = false, message = "Something Went Wrong!!" });
        //  
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM();
            {
                productVM.Product = new Product();
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()
                });
                productVM.CoverTypeList = _unitOfWork.CoverType.GetAll().Select(cl1 => new SelectListItem()
                {
                    Text = cl1.Name,
                    Value = cl1.Id.ToString()
                });
                if (id == null) return View(productVM);
                productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
                return View(productVM);

            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var webRoot = _webHostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    var fileName = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(files[0].FileName);
                    var uploads = Path.Combine(webRoot, @"images\products");
                    if (productVM.Product.Id != 0)
                    {
                        var imageExists = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExists;
                    }
                    if (productVM.Product.ImageUrl != null)
                    {
                        var imagePath = Path.Combine(webRoot, productVM.Product.ImageUrl.Trim('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\\images\products\" + fileName + extension;
                }
                else
                {
                    if (productVM.Product.Id != 0)
                    {
                        var imageExist = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExist;
                    }
                }
                if (productVM.Product.Id == 0)
                    _unitOfWork.Product.add(productVM.Product);
                else
                    _unitOfWork.Product.update(productVM.Product);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));


            }
            else
            {
                productVM = new ProductVM();
                {
                    productVM.Product = new Product();
                    productVM.CategoryList = _unitOfWork.Category.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    });
                    productVM.CoverTypeList = _unitOfWork.CoverType.GetAll().Select(cl1 => new SelectListItem()
                    {
                        Text = cl1.Name,
                        Value = cl1.Id.ToString()
                    });
                    if (productVM.Product.Id != 0)
                    {
                        productVM.Product = _unitOfWork.Product.Get(productVM.Product.Id);
                    }
                    return View(productVM);
                }
            }
        }

    }
}
