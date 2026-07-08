using FutsalBooking.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutsalBooking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var courts = await _context.Courts.Where(c => c.IsActive).ToListAsync();
            return View(courts);
        }

        public async Task<IActionResult> Courts()
        {
            var courts = await _context.Courts.Where(c => c.IsActive).ToListAsync();
            return View(courts);
        }

        public IActionResult About() => View();
        public IActionResult Contact() => View();
    }
}
