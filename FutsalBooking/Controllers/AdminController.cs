using FutsalBooking.Data;
using FutsalBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutsalBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var vm = new AdminViewModel();

            vm.TotalBookings = await _context.Bookings.CountAsync();
            vm.TotalUsers = _userManager.Users.Count();
            vm.PendingBookings = await _context.Bookings
                .CountAsync(b => b.Status == BookingStatus.Pending);
            vm.TodayBookings = await _context.Bookings
                .CountAsync(b => b.BookingDate.Date == DateTime.Today);
            vm.TotalRevenue = await _context.Bookings
                .Where(b => b.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(b => b.TotalAmount);
            vm.RecentBookings = await _context.Bookings
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .ToListAsync();

            // pass court status to view
            var court = await _context.Courts.FirstOrDefaultAsync();
            ViewBag.CourtIsActive = court?.IsActive ?? false;

            return View(vm);
        }

        // edit court info - name, price etc
        [HttpGet]
        public async Task<IActionResult> EditCourt()
        {
            var court = await _context.Courts.FirstOrDefaultAsync();
            if (court == null)
                return NotFound();

            return View(court);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourt(Court court)
        {
            if (!ModelState.IsValid)
                return View(court);

            _context.Courts.Update(court);
            await _context.SaveChangesAsync();

            TempData["Success"] = "court updated";
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Bookings(string? status)
        {
            var query = _context.Bookings
                .Include(b => b.User)
                .AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<BookingStatus>(status, out var parsedStatus))
                    query = query.Where(b => b.Status == parsedStatus);
            }

            var allBookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            return View(allBookings);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int id, BookingStatus status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                booking.Status = status;
                await _context.SaveChangesAsync();
            }
            // go back to all bookings not filtered
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
            }
            // go back to all bookings not filtered
            return RedirectToAction("Bookings");
        }

        // close or reopen court temporarily
        [HttpPost]
        public async Task<IActionResult> ToggleCourtStatus()
        {
            var court = await _context.Courts.FirstOrDefaultAsync();
            if (court != null)
            {
                court.IsActive = !court.IsActive;
                await _context.SaveChangesAsync();

                string msg = court.IsActive ? "court is now open" : "court is now closed";
                TempData["Success"] = msg;
            }
            return RedirectToAction("Dashboard");
        }
        public async Task<IActionResult> Users()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            return View(allUsers);
        }
    }
}