using Ecom_Book_main_DataAccess.Repository.IRepository;
using Ecom_Book_main_Models;
using Ecom_Book_main_Models.ViewModels;
using Ecom_Book_main_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace Ecom_Book_main.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claims == null)
            {
                ShoppingCartVM = new ShoppingCartVM()
                {
                    ListCart = new List<ShoppingCart>()
                };
                return View(ShoppingCartVM);
            }
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            foreach(var item in ShoppingCartVM.ListCart)
            {
                item.Price = SD.GetPriceBasedOnQuantiy(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);

                if (item.Product.Description.Length > 100)
                {
                    item.Product.Description = item.Product.Description.Substring(0, 99) + "...";
                }
            }
            return View(ShoppingCartVM);
        }
        public IActionResult Plus(int id)
        {
            var cart= _unitOfWork.ShoppingCart.FirstOrDefault(sc=>sc.Id== id);
            cart.Count += 1;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int id)
        {
            var cart = _unitOfWork.ShoppingCart.FirstOrDefault(sc => sc.Id == id);
            if (cart.Count == 1)
                cart.Count = 1;
            else
            cart.Count -= 1;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }
        public IActionResult Delete(int id)
        {
            var cart = _unitOfWork.ShoppingCart.FirstOrDefault(sc => sc.Id == id);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            var claimIdentity=(ClaimsIdentity)User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount,count);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            foreach (var item in ShoppingCartVM.ListCart)
            {
                item.Price = SD.GetPriceBasedOnQuantiy(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            ShoppingCartVM.OrderHeader.Name=ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.StreetAddress=ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City=ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State=ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode=ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            ShoppingCartVM.OrderHeader.PhoneNumber=ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPost(string stripeToken)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product");
            ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;
            _unitOfWork.OrderHeader.add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach(var item in ShoppingCartVM.ListCart)
            {
                item.Price= SD.GetPriceBasedOnQuantiy(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);
                OrderDetail orderDetail = new OrderDetail()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = item.Price,
                    Count = item.Count
                };
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
                _unitOfWork.OrderDetail.add(orderDetail);
                _unitOfWork.Save();
            }
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, 0);
            #region Stripe
            if(stripeToken == null)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
            }
            else
            {
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal),
                    Currency = "usd",
                    Description = "Order ID : " + ShoppingCartVM.OrderHeader.Id,
                    Source = stripeToken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.BalanceTransactionId == null)
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                else
                    ShoppingCartVM.OrderHeader.TransactionId=charge.BalanceTransactionId;
                if (charge.Status.ToLower() == "succeeded")
                {
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                }
                _unitOfWork.Save();
            }
            #endregion
            return RedirectToAction("OrderConfirmation", "Cart", new {id=ShoppingCartVM.OrderHeader.Id});
        }
        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }
    }
}
