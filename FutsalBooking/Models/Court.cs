using System.ComponentModel.DataAnnotations;

namespace FutsalBooking.Models
{
    public class Court
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "please enter court name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal PricePerHour { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}