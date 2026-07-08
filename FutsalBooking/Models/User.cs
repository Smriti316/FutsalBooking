using Microsoft.AspNetCore.Identity;

namespace FutsalBooking.Models
{
    public class User : IdentityUser //Identity user is built in class which has built in fields like ID, Username etc 
    {
            public string FullName { get; set; } = string.Empty;

            public DateTime CreatedAt { get; set; } = DateTime.Now;

            public ICollection<Booking> Bookings { get; set; } = new List<Booking>(); //User's Booking list
        }
    }
