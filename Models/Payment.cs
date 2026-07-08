using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FutsalBooking.Models
{
    // stores payment records for each booking
    public class Payment
    {
        public int Id { get; set; }

        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        // amount in paisa (khalti uses paisa, so Rs.100 = 10000 paisa)
        public long AmountInPaisa { get; set; }

        // token from khalti after user pays
        public string? KhaltiToken { get; set; }

        // transaction id from khalti after verification
        public string? TransactionId { get; set; }

        // idx returned by khalti
        public string? KhaltiIdx { get; set; }

        public PaymentState State { get; set; } = PaymentState.Initiated;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PaidAt { get; set; }
    }

    public enum PaymentState
    {
        Initiated,
        Completed,
        Failed,
        Refunded
    }
}
