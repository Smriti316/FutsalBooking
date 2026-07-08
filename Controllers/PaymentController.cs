using FutsalBooking.Data;
using FutsalBooking.Models;
using FutsalBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FutsalBooking.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly KhaltiService _khaltiService;
        private readonly IConfiguration _config;

        public PaymentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            KhaltiService khaltiService,
            IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _khaltiService = khaltiService;
            _config = config;
        }

        // step 1: user clicks "Pay with Khalti" button
        // we call khalti API to get a payment URL, then redirect user there
        [HttpPost]
        public async Task<IActionResult> InitiatePayment(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);

            var booking = await _context.Bookings
                .Include(b => b.Court)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == user!.Id);

            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("Index", "Booking");
            }

            if (booking.PaymentStatus == PaymentStatus.Paid)
            {
                TempData["Error"] = "This booking is already paid.";
                return RedirectToAction("Index", "Booking");
            }

            // khalti needs amount in PAISA
            // Rs. 1 = 100 paisa, so Rs. 1500 = 150000 paisa
            long amountInPaisa = (long)(booking.TotalAmount * 100);

            // build the return url - khalti will redirect here after payment
            string returnUrl = Url.Action("PaymentCallback", "Payment",
                new { bookingId = booking.Id }, Request.Scheme) ?? "";

            string websiteUrl = $"{Request.Scheme}://{Request.Host}";

            var initiateRequest = new KhaltiInitiateRequest
            {
                return_url = returnUrl,
                website_url = websiteUrl,
                amount = amountInPaisa,
                purchase_order_id = "BOOKING-" + booking.Id.ToString(),
                purchase_order_name = "PlayZone Futsal - " + booking.Court!.Name,
                customer_info = new KhaltiCustomer
                {
                    name = user!.FullName,
                    email = user.Email ?? "",
                    phone = user.PhoneNumber ?? "9800000000"
                }
            };

            var khaltiResponse = await _khaltiService.InitiatePayment(initiateRequest);

            if (khaltiResponse == null || string.IsNullOrEmpty(khaltiResponse.payment_url))
            {
                TempData["Error"] = "Could not connect to Khalti. Please try again or pay at venue.";
                return RedirectToAction("Details", "Booking", new { id = bookingId });
            }

            // save payment record with pidx so we can verify later
            var payment = new Payment
            {
                BookingId = booking.Id,
                AmountInPaisa = amountInPaisa,
                KhaltiToken = khaltiResponse.pidx,
                State = PaymentState.Initiated,
                CreatedAt = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // redirect user to khalti payment page
            return Redirect(khaltiResponse.payment_url);
        }

        // step 2: khalti redirects back to this URL after user pays
        // we verify the payment with khalti API
        [HttpGet]
        public async Task<IActionResult> PaymentCallback(int bookingId, string pidx, string status,
            string? transaction_id, string? tidx, long? amount, string? mobile)
        {
            // khalti sends status=Completed if paid successfully
            if (status != "Completed")
            {
                TempData["Error"] = "Payment was not completed. You can try again or pay at the venue.";
                return RedirectToAction("Details", "Booking", new { id = bookingId });
            }

            // verify with khalti API to confirm payment is real
            // (important - don't just trust the URL parameters)
            var verifyResponse = await _khaltiService.VerifyPayment(pidx);

            if (verifyResponse == null || verifyResponse.status != "Completed")
            {
                TempData["Error"] = "Payment verification failed. Please contact support.";
                return RedirectToAction("Details", "Booking", new { id = bookingId });
            }

            // payment confirmed - update booking and payment records
            var user = await _userManager.GetUserAsync(User);

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == user!.Id);

            if (booking != null)
            {
                booking.PaymentStatus = PaymentStatus.Paid;
                booking.Status = BookingStatus.Confirmed;
            }

            // update the payment record
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId && p.KhaltiToken == pidx);

            if (payment != null)
            {
                payment.State = PaymentState.Completed;
                payment.TransactionId = verifyResponse.transaction_id;
                payment.KhaltiIdx = tidx;
                payment.PaidAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment successful! Your booking is confirmed. Transaction ID: " + verifyResponse.transaction_id;
            return RedirectToAction("Details", "Booking", new { id = bookingId });
        }

        // show payment history for a booking
        public async Task<IActionResult> PaymentHistory(int bookingId)
        {
            var user = await _userManager.GetUserAsync(User);

            // make sure this booking belongs to the current user
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == user!.Id);

            if (booking == null) return NotFound();

            var payments = await _context.Payments
                .Where(p => p.BookingId == bookingId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.BookingId = bookingId;
            return View(payments);
        }
    }
}
