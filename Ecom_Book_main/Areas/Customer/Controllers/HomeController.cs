using Ecom_Book_main_DataAccess.Repository.IRepository;
using Ecom_Book_main_Models;
using Ecom_Book_main_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Ecom_Book_main.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimIdentity=(ClaimsIdentity)User.Identity;
            var claims=claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }
            var productList=_unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return View(productList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Details(int id)
        {
            var productsInDb = _unitOfWork.Product.FirstOrDefault(u => u.Id == id, includeProperties: "Category,CoverType");
            if (productsInDb == null) return NotFound();
            var shoppingCart = new ShoppingCart()
            {
                Product=productsInDb,
                ProductId=productsInDb.Id
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                if (claims == null) return NotFound();
                shoppingCart.ApplicationUserId = claims.Value;
                var shoppingCartInDb=_unitOfWork.ShoppingCart.FirstOrDefault
                    (sc=>sc.ApplicationUserId==claims.Value && sc.ProductId==shoppingCart.ProductId);
                if (shoppingCartInDb == null)
                    _unitOfWork.ShoppingCart.add(shoppingCart);
                else
                    shoppingCartInDb.Count += shoppingCart.Count;
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));


            }
            else
            {
                var productsInDb = _unitOfWork.Product.FirstOrDefault(u => u.Id == shoppingCart.Id, includeProperties: "Category,CoverType");
                if (productsInDb == null) return NotFound();
                var shoppingCartEdit = new ShoppingCart()
                {
                    Product = productsInDb,
                    ProductId = productsInDb.Id
                };
                return View(shoppingCartEdit);
            }
        }
    }
}