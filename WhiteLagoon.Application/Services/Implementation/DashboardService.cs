using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Application.Services.Interface;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Application.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month.Equals(1) ? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new DateTime(DateTime.Now.Year, previousMonth, 1);
        readonly DateTime currentMonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PieChartDto> GetBookingPieChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(b => b.BookingDate >= DateTime.Now.AddDays(-30) && !b.Status.Equals(SD.StatusPending) || b.Status.Equals(SD.StatusCancelled));
            var customerWithOneBooking = totalBookings.GroupBy(b => b.UserId).Where(x => x.Count().Equals(1)).Select(y => y.Key).ToList();
            int bookingByNewCustomer = customerWithOneBooking.Count;
            int bookingsByReturningCustomer = totalBookings.Count() - bookingByNewCustomer;

            PieChartDto PieChartDto = new()
            {
                Labels = new string[] { "New Customer Bookings", "Returning Customer Bookings" },
                Series = new decimal[] { bookingByNewCustomer, bookingsByReturningCustomer }
            };

            return PieChartDto;
        }

        public async Task<LineChartDto> GetMemberAndBookingLineChartData()
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

            LineChartDto LineChartDto = new()
            {
                Categories = categories,
                Series = chartDataList
            };

            return LineChartDto;
        }

        public async Task<RadialBarChartDto> GetRegisteredUserChartData()
        {
            var totalUsers = _unitOfWork.User.GetAll();
            var countByCurrentMonth = totalUsers.Count(x => x.CreatedAt >= currentMonthStartDate && x.CreatedAt <= DateTime.Now);
            var countByPreviousMonth = totalUsers.Count(x => x.CreatedAt >= previousMonthStartDate && x.CreatedAt <= currentMonthStartDate);

            return SD.GetRadialChartDataModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth);
        }

        public async Task<RadialBarChartDto> GetRevenueChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(b => !b.Status.Equals(SD.StatusPending) || b.Status.Equals(SD.StatusCancelled));
            var totalRevenue = Convert.ToInt32(totalBookings.Sum(b => b.TotalCost));
            var countByCurrentMonth = totalBookings.Where(x => x.BookingDate >= currentMonthStartDate && x.BookingDate <= DateTime.Now).Sum(b => b.TotalCost);
            var countByPreviousMonth = totalBookings.Where(x => x.BookingDate >= previousMonthStartDate && x.BookingDate <= currentMonthStartDate).Sum(b => b.TotalCost);

            return SD.GetRadialChartDataModel(totalRevenue, countByCurrentMonth, countByPreviousMonth);
        }

        public async Task<RadialBarChartDto> GetTotalBookingRadialChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(b => !b.Status.Equals(SD.StatusPending) || b.Status.Equals(SD.StatusCancelled));
            var countByCurrentMonth = totalBookings.Count(x => x.BookingDate >= currentMonthStartDate && x.BookingDate <= DateTime.Now);
            var countByPreviousMonth = totalBookings.Count(x => x.BookingDate >= previousMonthStartDate && x.BookingDate <= currentMonthStartDate);

            return SD.GetRadialChartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth);
        }
    }
}
