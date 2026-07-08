using Microsoft.AspNetCore.Identity;

namespace FutsalBooking.Models
{
    // extending the default IdentityUser to add extra fields
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        // phone already exists in IdentityUser but I'm keeping it here too
        // actually wait - IdentityUser already has PhoneNumber so dont need this
        // keeping it anyway because removing it breaks things
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
