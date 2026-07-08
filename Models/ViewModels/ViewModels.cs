using System.ComponentModel.DataAnnotations;

namespace FutsalBooking.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class BookingViewModel
    {
        [Required]
        public int CourtId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; } = DateTime.Today.AddDays(1);

        [Required]
        [Display(Name = "Start Time")]
        public string StartTime { get; set; } = "06:00";

        [Required]
        [Display(Name = "End Time")]
        public string EndTime { get; set; } = "07:00";

        public string? Notes { get; set; }

        public Court? Court { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalBookings { get; set; }
        public int TotalCourts { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingBookings { get; set; }
        public int TodayBookings { get; set; }
        public List<Booking> RecentBookings { get; set; } = new();
    }
}
