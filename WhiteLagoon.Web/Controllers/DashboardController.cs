using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month.Equals(1) ? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new DateTime(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(b => !b.Status.Equals(SD.StatusPending) || b.Status.Equals(SD.StatusCancelled));
            var countByCurrentMonth = totalBookings.Count(x => x.BookingDate >= currentMonthStartDate && x.BookingDate <= DateTime.Now);
            var countByPreviousMonth = totalBookings.Count(x => x.BookingDate >= previousMonthStartDate && x.BookingDate <= currentMonthStartDate);

            return Json(GetRadialChartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            var totalUsers = _unitOfWork.User.GetAll();
            var countByCurrentMonth = totalUsers.Count(x => x.CreatedAt >= currentMonthStartDate && x.CreatedAt <= DateTime.Now);
            var countByPreviousMonth = totalUsers.Count(x => x.CreatedAt >= previousMonthStartDate && x.CreatedAt <= currentMonthStartDate);

            return Json(GetRadialChartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetRevenueChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(b => !b.Status.Equals(SD.StatusPending) || b.Status.Equals(SD.StatusCancelled));
            var totalRevenue = Convert.ToInt32(totalBookings.Sum(b => b.TotalCost));
            var countByCurrentMonth = totalBookings.Where(x => x.BookingDate >= currentMonthStartDate && x.BookingDate <= DateTime.Now).Sum(b => b.TotalCost);
            var countByPreviousMonth = totalBookings.Where(x => x.BookingDate >= previousMonthStartDate && x.BookingDate <= currentMonthStartDate).Sum(b => b.TotalCost);

            return Json(GetRadialChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth));
        }

        public async Task<IActionResult> GetBookingPieChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(b => b.BookingDate >= DateTime.Now.AddDays(-30) && !b.Status.Equals(SD.StatusPending) || b.Status.Equals(SD.StatusCancelled));
            var customerWithOneBooking = totalBookings.GroupBy(b => b.UserId).Where(x => x.Count().Equals(1)).Select(y => y.Key).ToList();
            int bookingByNewCustomer = customerWithOneBooking.Count;
            int bookingsByReturningCustomer = totalBookings.Count() - bookingByNewCustomer;

            PieChartVM pieChartVM = new()
            {
                Labels = new string[] { "New Customer Bookings", "Returning Customer Bookings" },
                Series = new decimal[] { bookingByNewCustomer, bookingsByReturningCustomer }
            };

            return Json(pieChartVM);
        }

        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            var bookingData = _unitOfWork.Booking.GetAll(b => b.BookingDate >= DateTime.Now.AddDays(-30) && b.BookingDate.Date <= DateTime.Now).GroupBy(x => x.BookingDate.Date).Select(y => new
            {
                DateTime = y.Key,
                NewBookingCount = y.Count()
            });

            var customerData = _unitOfWork.User.GetAll(b => b.CreatedAt >= DateTime.Now.AddDays(-30) && b.CreatedAt.Date <= DateTime.Now).GroupBy(x => x.CreatedAt.Date).Select(y => new
            {
                DateTime = y.Key,
                NewCustomerCount = y.Count()
            });

            var leftJoin = bookingData.GroupJoin(customerData, booking => booking.DateTime, customer => customer.DateTime, (booking, customer) => new
            {
                booking.DateTime,
                booking.NewBookingCount,
                NewCustomerCount = customer.Select(c => c.NewCustomerCount).FirstOrDefault()
            });

            var rightJoin = customerData.GroupJoin(bookingData, customer => customer.DateTime, booking => booking.DateTime, (customer, booking) => new
            {
                customer.DateTime,
                NewBookingCount = booking.Select(b => b.NewBookingCount).FirstOrDefault(),
                customer.NewCustomerCount
            });

            var mergedData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime).ToList();

            var newBookingData = mergedData.Select(m => m.NewBookingCount).ToArray();
            var newCustomerData = mergedData.Select(m => m.NewCustomerCount).ToArray();
            var categories = mergedData.Select(m => m.DateTime.ToString("MM/dd/yyyy")).ToArray();

            List<ChartData> chartDataList = new()
            {
                new ChartData
                {
                    Name = "New Bookings",
                    Data = newBookingData
                },
                new ChartData
                {
                    Name = "New Members",
                    Data = newCustomerData
                }
            };

            LineChartVM lineChartVM = new()
            {
                Categories = categories,
                Series = chartDataList
            };

            return Json(lineChartVM);
        }

        private static RadialBarChartVM GetRadialChartDataModel(int totalCount, double countInCurrentMonth, double countInPreviousMonth)
        {
            RadialBarChartVM radialBarChartVM = new();

            int increaseDecreaseRatio = 100;

            if (!countInPreviousMonth.Equals(0))
            {
                increaseDecreaseRatio = Convert.ToInt32((countInCurrentMonth - countInPreviousMonth) / countInPreviousMonth * 100);
            }

            radialBarChartVM.TotalCount = totalCount;
            radialBarChartVM.CountInCurrentMonth = Convert.ToInt32(countInCurrentMonth);
            radialBarChartVM.HasRatioIncreased = countInCurrentMonth > countInPreviousMonth;
            radialBarChartVM.Series = new int[] { increaseDecreaseRatio };

            return radialBarChartVM;
        }
    }
}
