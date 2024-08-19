using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Models.ViewModels
{
    public class RevenueViewModel
    {
        public DateTime Date { get; set; }
        public double TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public List<OrderHeader> PaidOrders { get; set; }  // Danh sách đơn hàng đã thanh toán
    }
}
