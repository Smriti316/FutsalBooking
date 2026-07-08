using FutsalBooking.Data;
using FutsalBooking.Models;
using FutsalBooking.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutsalBooking.Controllers
{
    // only admin can access these pages
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            // gather all stats for the dashboard
            var vm = new AdminDashboardViewModel();

            vm.TotalBookings = await _context.Bookings.CountAsync();
            vm.TotalCourts = await _context.Courts.CountAsync();
            vm.TotalUsers = _userManager.Users.Count();
            vm.PendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);
            vm.TodayBookings = await _context.Bookings.CountAsync(b => b.BookingDate.Date == DateTime.Today);

            // only count revenue from paid bookings
            vm.TotalRevenue = await _context.Bookings
                .Where(b => b.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(b => b.TotalAmount);

            // get latest 10 bookings for the table
            vm.RecentBookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Court)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(vm);
        }

        // ============ COURT MANAGEMENT ============

        public async Task<IActionResult> Courts()
        {
            var courts = await _context.Courts.ToListAsync();
            return View(courts);
        }

        [HttpGet]
        public IActionResult CreateCourt()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourt(Court court)
        {
            if (!ModelState.IsValid)
            {
                return View(court);
            }

            _context.Courts.Add(court);
            await _context.SaveChangesAsync();

            TempData["Success"] = "New court added successfully!";
            return RedirectToAction("Courts");
        }

        [HttpGet]
        public async Task<IActionResult> EditCourt(int id)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court == null)
            {
                return NotFound();
            }
            return View(court);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourt(Court court)
        {
            if (!ModelState.IsValid)
            {
                return View(court);
            }

            _context.Courts.Update(court);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Court updated.";
            return RedirectToAction("Courts");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourt(int id)
        {
            var court = await _context.Courts.FindAsync(id);
            if (court != null)
            {
                // soft delete - just set inactive, don't actually delete
                // because existing bookings still reference this court
                court.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Court deactivated.";
            }
            return RedirectToAction("Courts");
        }

        // ============ BOOKING MANAGEMENT ============

        public async Task<IActionResult> Bookings(string? status)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Court)
                .AsQueryable();

            // filter by status if provided
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<BookingStatus>(status, out var bookingStatus))
                {
                    query = query.Where(b => b.Status == bookingStatus);
                }
            }

            var bookings = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();

            ViewBag.CurrentStatus = status;
            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int id, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Booking status updated.";
            }
            return RedirectToAction("Bookings");
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePaymentStatus(int id, PaymentStatus status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.PaymentStatus = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Payment status updated.";
            }
            return RedirectToAction("Bookings");
        }

        // ============ USER MANAGEMENT ============

        public async Task<IActionResult> Users()
        {
            // TODO: add pagination here - will be slow if many users
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }
    }
}
