using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
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
            
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new() {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages"),
                Count = 1,
                ProductId = productId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart) 
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            // Giả sử bạn có một thuộc tính trong đối tượng shoppingCart để lấy số lượng tồn kho hiện tại
            var productFromDb = _unitOfWork.Product.Get(u => u.Id == shoppingCart.ProductId);

            if (productFromDb == null || productFromDb.StockQuantity < shoppingCart.Count)
            {
                // Nếu không tìm thấy sản phẩm hoặc số lượng yêu cầu vượt quá số lượng tồn kho
                TempData["error"] = "Không đủ số lượng hàng tồn kho.";
                return RedirectToAction(nameof(Index));
            }


            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart
                .Get(u=>u.ApplicationUserId == userId &&
                        u.ProductId==shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                // Kiểm tra tổng số lượng trong giỏ và số lượng mới có vượt quá hàng tồn kho không
                if (cartFromDb.Count + shoppingCart.Count > productFromDb.StockQuantity)
                {
                    TempData["error"] = "Không đủ số lượng hàng tồn kho.";
                    return RedirectToAction(nameof(Index));
                }

                //shopping cart exists
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                // Kiểm tra số lượng mới có vượt quá hàng tồn kho không
                if (shoppingCart.Count > productFromDb.StockQuantity)
                {
                    TempData["error"] = "Không đủ số lượng hàng tồn kho.";
                    return RedirectToAction(nameof(Index));
                }

                //add cart record
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            }

            TempData["success"] = "Cart updated successfully";
            return RedirectToAction(nameof(Index));
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
    }
}