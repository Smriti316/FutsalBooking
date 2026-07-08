using FutsalBooking.Data;
using FutsalBooking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutsalBooking.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public HomeController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
       public async Task<IActionResult> Index()
{
    var court = await _context.Courts.FirstOrDefaultAsync(c => c.IsActive == true);

    // get today's bookings to show slot availability
    var todayBookings = await _context.Bookings
        .Where(b => b.BookingDate.Date == DateTime.Today &&
                    b.Status != BookingStatus.Cancelled)
        .ToListAsync();

    // get current user's bookings for today
    string? currentUserId = null;
    if (User.Identity!.IsAuthenticated)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        currentUserId = currentUser?.Id;
    }
    ViewBag.TodayBookings = todayBookings;
    ViewBag.CurrentUserId = currentUserId;

    return View(court);
}
    }
}