using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FutsalBooking.Models
{
    public class Booking
    {
        public int Id { get; set; }

        // who made this booking
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public int CourtId { get; set; }

        [ForeignKey("CourtId")]
        public Court? Court { get; set; }

        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        // storing time as TimeSpan because DateTime was giving problems
        // TimeSpan just stores the time part like 06:00:00
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // this is calculated automatically in the controller
        // price per hour * total hours
        public decimal TotalAmount { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        // when was this record created
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // optional notes from customer
        public string? Notes { get; set; }
    }

    // possible status for booking
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    // payment status update  after receiving payment
    public enum PaymentStatus
    {
        Unpaid,
        Paid,
        Refunded
    }
}