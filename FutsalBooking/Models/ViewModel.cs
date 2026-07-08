using System.ComponentModel.DataAnnotations;

namespace FutsalBooking.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }

    }
    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be atlease 6 characters")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords doesn't match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    public class BookingViewModel
    {
        [Required]
        public DateTime BookingDate { get; set; } = DateTime.Today;
        [Required]
        public string StartTime { get; set; } = DateTime.Now.AddMinutes(30).ToString("HH:00");
        [Required]
        public string EndTime { get; set; } = DateTime.Now.AddMinutes(90).ToString("HH:00");
    }
    public class AdminViewModel
    {
        public int TotalBookings { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingBookings { get; set; }
        public int TodayBookings { get; set; }
        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
    }
}