using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Utility
{
    public static class SD
    {
        public const string Role_Admin = "Admin";
        public const string Role_Customer = "Customer";
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";
        public static int VillaRoomsAvailable_Count(int villaId, List<VillaNumber> villaNumberList, DateOnly checkInDate, int nights, List<Booking> bookings)
        {
            List<int> bookingInDate = new();
            int finalAvailableRoomsForAllNights = int.MaxValue;
            var roomsInVilla = villaNumberList.Where(v => v.VillaId.Equals(villaId)).Count();

            for (int i = 0; i < nights; i++)
            {
                var villasBooked = bookings.Where(b => b.VillaId.Equals(villaId) && 
                                            b.CheckInDate <= checkInDate.AddDays(i) && 
                                            b.CheckOutDate >= checkInDate.AddDays(i));

                foreach (var booking in villasBooked)
                {
                    if (!bookingInDate.Contains(booking.Id))
                    {
                        bookingInDate.Add(booking.Id);
                    }
                }

                var totalAvailableRooms = roomsInVilla - bookingInDate.Count;
                if (totalAvailableRooms == 0)
                {
                    return 0;
                }
                else
                {
                    if (totalAvailableRooms < finalAvailableRoomsForAllNights)
                    {
                        finalAvailableRoomsForAllNights = totalAvailableRooms;
                    }
                }
            }
            return finalAvailableRoomsForAllNights;
        }
    }
}
