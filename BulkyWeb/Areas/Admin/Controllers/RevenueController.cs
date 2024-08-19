using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RevenueController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public RevenueController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            // Lấy dữ liệu đơn hàng từ cơ sở dữ liệu và bao gồm thông tin người dùng
            var orders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

            // Tính toán doanh thu và số lượng đơn hàng
            var totalRevenue = orders.Sum(o => o.OrderTotal);
            var totalOrders = orders.Count();

            // Lấy các đơn hàng đã thanh toán
            var paidOrders = orders.Where(o => o.PaymentStatus == "Paid").ToList();

            var viewModel = new RevenueViewModel
            {
                Date = DateTime.Now,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                PaidOrders = paidOrders
            };

            return View(viewModel);
        }
    }
}
