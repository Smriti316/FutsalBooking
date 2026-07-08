using FutsalBooking.Data;
using FutsalBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutsalBooking.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public BookingController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var myBookings = await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.UserId == currentUser!.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(myBookings);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var court = await _context.Courts
                .FirstOrDefaultAsync(c => c.IsActive == true);

            if (court == null)
            {
                TempData["Error"] = "court not available";
                return RedirectToAction("Index", "Home");
            }
            ViewBag.PricePerHour = court.PricePerHour;

            var model = new BookingViewModel
            {
                BookingDate = DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            if (model.BookingDate.Date > DateTime.Today.AddDays(7))
            {
                ModelState.AddModelError("", "You can only book up to 7 days in advance");
                return View(model);
            }
            var court = await _context.Courts.FirstOrDefaultAsync(c => c.IsActive == true);
            if (court == null)
                return NotFound();

            var startTime = TimeSpan.Parse(model.StartTime);
            var endTime = TimeSpan.Parse(model.EndTime);

            if (endTime <= startTime)
            {
                ModelState.AddModelError("", "End time must be after start time");
                return View(model);
            }

            // must book at least 30 mins in advance
            var bookingDateTime = model.BookingDate.Date + startTime;
            if (bookingDateTime < DateTime.Now.AddMinutes(15))
            {
                ModelState.AddModelError("", "Please book at least 15 minutes in advance");
                return View(model);
            }
            // conflict check 
            // existing booking overlaps if it starts before our end AND ends after our start
            bool isSlotTaken = await _context.Bookings.AnyAsync(b =>
                b.BookingDate.Date == model.BookingDate.Date &&
                b.Status != BookingStatus.Cancelled &&
                b.StartTime < endTime &&
                b.EndTime > startTime);
            if (isSlotTaken)
            {
                ModelState.AddModelError("", "This slot is already booked, please choose another time");
                return View(model);
            }
            var currentUser = await _userManager.GetUserAsync(User);
            double hours = (endTime - startTime).TotalHours;
            decimal totalPrice = court.PricePerHour * (decimal)hours;
            var newBooking = new Booking
            {
                UserId = currentUser!.Id,
                BookingDate = model.BookingDate,
                StartTime = startTime,
                EndTime = endTime,
                TotalAmount = totalPrice,
                Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Unpaid
            };
            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();
            TempData["Success"] = "booked! Rs. " + totalPrice.ToString("N0");
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == currentUser!.Id);
            if (booking == null)
                return NotFound();
            return View(booking);
        }
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == currentUser!.Id);
            if (booking == null)
                return NotFound();
            if (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed)
            {
                booking.Status = BookingStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking cancelled";
            }
            return RedirectToAction("Index");
        }
    }
}