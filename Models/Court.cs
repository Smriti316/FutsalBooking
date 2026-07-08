using System.ComponentModel.DataAnnotations;

namespace FutsalBooking.Models
{
    // Court model - represents each futsal court in the system
    public class Court
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Court name is required")]
        [Display(Name = "Court Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Price Per Hour (Rs.)")]
        public decimal PricePerHour { get; set; }

        // Indoor or Outdoor - default is Indoor
        [Display(Name = "Court Type")]
        public string CourtType { get; set; } = "Indoor";

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // TODO: add image upload feature later (currently not implemented)
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        // navigation property for bookings
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
