using FutsalBooking.Data;
using FutsalBooking.Models;
using FutsalBooking.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutsalBooking.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // show all bookings of currently logged in user
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var bookings = await _context.Bookings
                .Include(b => b.Court)
                .Where(b => b.UserId == user!.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courtId)
        {
            var court = await _context.Courts.FindAsync(courtId);
            if (court == null)
            {
                return NotFound();
            }

            var model = new BookingViewModel
            {
                CourtId = courtId,
                Court = court,
                BookingDate = DateTime.Today.AddDays(1) // default tomorrow
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Court = await _context.Courts.FindAsync(model.CourtId);
                return View(model);
            }

            var court = await _context.Courts.FindAsync(model.CourtId);
            if (court == null) return NotFound();

            var startTime = TimeSpan.Parse(model.StartTime);
            var endTime = TimeSpan.Parse(model.EndTime);

            // make sure end time is after start time
            if (endTime <= startTime)
            {
                ModelState.AddModelError("", "End time must be after start time.");
                model.Court = court;
                return View(model);
            }

            // check if this time slot is already taken
            // this was tricky to figure out - had to think about overlap logic
            bool alreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.CourtId == model.CourtId &&
                b.BookingDate.Date == model.BookingDate.Date &&
                b.Status != BookingStatus.Cancelled &&
                b.StartTime < endTime &&
                b.EndTime > startTime);

            if (alreadyBooked)
            {
                ModelState.AddModelError("", "Sorry, this time slot is already booked. Please choose a different time.");
                model.Court = court;
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            // calculate total price
            double hours = (endTime - startTime).TotalHours;
            decimal totalAmount = court.PricePerHour * (decimal)hours;

            var booking = new Booking
            {
                UserId = user!.Id,
                CourtId = model.CourtId,
                BookingDate = model.BookingDate,
                StartTime = startTime,
                EndTime = endTime,
                TotalAmount = totalAmount,
                Notes = model.Notes,
                Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Unpaid
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // TODO: send email/SMS confirmation here later
            // TODO: integrate Khalti payment gateway

            TempData["Success"] = "Booking successful! Total amount: Rs. " + totalAmount.ToString("N0");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var booking = await _context.Bookings
                .Include(b => b.Court)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user!.Id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user!.Id);

            if (booking == null) return NotFound();

            // only allow cancel if pending or confirmed
            if (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed)
            {
                booking.Status = BookingStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking has been cancelled.";
            }
            else
            {
                TempData["Error"] = "Cannot cancel this booking.";
            }

            return RedirectToAction("Index");
        }

        // ajax endpoint to check slot availability - used in booking form
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int courtId, string date, string start, string end)
        {
            try
            {
                var bookingDate = DateTime.Parse(date);
                var startTime = TimeSpan.Parse(start);
                var endTime = TimeSpan.Parse(end);

                bool conflict = await _context.Bookings.AnyAsync(b =>
                    b.CourtId == courtId &&
                    b.BookingDate.Date == bookingDate.Date &&
                    b.Status != BookingStatus.Cancelled &&
                    b.StartTime < endTime &&
                    b.EndTime > startTime);

                return Json(new { available = !conflict });
            }
            catch
            {
                // if something goes wrong just return available = false to be safe
                return Json(new { available = false });
            }
        }
    }
}
