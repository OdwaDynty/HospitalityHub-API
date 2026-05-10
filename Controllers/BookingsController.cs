using HospitalityHub.API.Data;
using HospitalityHub.API.DTOs.Bookings;
using HospitalityHub.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalityHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/bookings
        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetAllBookings(
            [FromQuery] string? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var query = _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Guest)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(b => b.Status == status);

            if (fromDate.HasValue)
                query = query.Where(b => b.CheckInDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(b => b.CheckOutDate <= toDate.Value);

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    RoomId = b.RoomId,
                    RoomNumber = b.Room.RoomNumber,
                    RoomType = b.Room.RoomType,
                    GuestId = b.GuestId,
                    GuestName = b.Guest.FullName,
                    GuestEmail = b.Guest.Email,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    Nights = (int)(b.CheckOutDate - b.CheckInDate).TotalDays,
                    Status = b.Status,
                    TotalAmount = b.TotalAmount,
                    CreatedAt = b.CreatedAt
                }).ToListAsync();

            return Ok(bookings);
        }

        // GET: api/bookings/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Guest)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound(new { message = $"Booking with ID {id} not found." });

            return Ok(new BookingResponseDto
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                RoomNumber = booking.Room.RoomNumber,
                RoomType = booking.Room.RoomType,
                GuestId = booking.GuestId,
                GuestName = booking.Guest.FullName,
                GuestEmail = booking.Guest.Email,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                Nights = (int)(booking.CheckOutDate - booking.CheckInDate).TotalDays,
                Status = booking.Status,
                TotalAmount = booking.TotalAmount,
                CreatedAt = booking.CreatedAt
            });
        }

        // POST: api/bookings
        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            // Validate dates
            if (dto.CheckInDate.Date < DateTime.UtcNow.Date)
                return BadRequest(new { message = "Check-in date cannot be in the past." });

            if (dto.CheckOutDate.Date <= dto.CheckInDate.Date)
                return BadRequest(new { message = "Check-out date must be after check-in date." });

            // Check room exists
            var room = await _context.Rooms.FindAsync(dto.RoomId);
            if (room == null)
                return NotFound(new { message = "Room not found." });

            // Check room is available
            if (room.Status == "Maintenance")
                return BadRequest(new { message = "Room is currently under maintenance." });

            // Check guest exists
            var guest = await _context.Guests.FindAsync(dto.GuestId);
            if (guest == null)
                return NotFound(new { message = "Guest not found." });

            // Double-booking prevention — core business logic
            var isDoubleBooked = await _context.Bookings.AnyAsync(b =>
                b.RoomId == dto.RoomId &&
                (b.Status == "Confirmed" || b.Status == "CheckedIn") &&
                b.CheckInDate.Date < dto.CheckOutDate.Date &&
                b.CheckOutDate.Date > dto.CheckInDate.Date);

            if (isDoubleBooked)
                return BadRequest(new { message = "Room is already booked for the selected dates." });

            // Calculate total amount
            var nights = (int)(dto.CheckOutDate.Date - dto.CheckInDate.Date).TotalDays;
            var totalAmount = nights * room.PricePerNight;

            var booking = new Booking
            {
                RoomId = dto.RoomId,
                GuestId = dto.GuestId,
                CheckInDate = dto.CheckInDate.Date,
                CheckOutDate = dto.CheckOutDate.Date,
                TotalAmount = totalAmount,
                Status = "Confirmed"
            };

            // Update room status
            room.Status = "Occupied";

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, new BookingResponseDto
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                GuestId = booking.GuestId,
                GuestName = guest.FullName,
                GuestEmail = guest.Email,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                Nights = nights,
                Status = booking.Status,
                TotalAmount = booking.TotalAmount,
                CreatedAt = booking.CreatedAt
            });
        }

        // PUT: api/bookings/5/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto dto)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound(new { message = $"Booking with ID {id} not found." });

            var validStatuses = new[] { "Confirmed", "CheckedIn", "CheckedOut", "Cancelled" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { message = "Invalid status. Use: Confirmed, CheckedIn, CheckedOut, Cancelled" });

            booking.Status = dto.Status;

            // Update room status based on booking status
            if (dto.Status == "CheckedIn")
                booking.Room.Status = "Occupied";
            else if (dto.Status == "CheckedOut" || dto.Status == "Cancelled")
                booking.Room.Status = "Available";

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Booking status updated to {dto.Status}." });
        }

        // DELETE: api/bookings/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
                return NotFound(new { message = $"Booking with ID {id} not found." });

            if (booking.Status == "CheckedIn")
                return BadRequest(new { message = "Cannot cancel a booking that is currently checked in." });

            booking.Status = "Cancelled";
            booking.Room.Status = "Available";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking cancelled successfully." });
        }
    }
}