using HospitalityHub.API.Data;
using HospitalityHub.API.DTOs.Guests;
using HospitalityHub.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalityHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GuestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GuestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/guests
        [HttpGet]
        public async Task<IActionResult> GetAllGuests([FromQuery] string? search)
        {
            var query = _context.Guests.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(g =>
                    g.FullName.Contains(search) ||
                    g.Email.Contains(search) ||
                    g.PhoneNumber.Contains(search));

            var guests = await query.Select(g => new GuestResponseDto
            {
                Id = g.Id,
                FullName = g.FullName,
                Email = g.Email,
                PhoneNumber = g.PhoneNumber,
                IdNumber = g.IdNumber,
                Address = g.Address,
                CreatedAt = g.CreatedAt
            }).ToListAsync();

            return Ok(guests);
        }

        // GET: api/guests/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGuest(int id)
        {
            var guest = await _context.Guests
                .Include(g => g.Bookings)
                .ThenInclude(b => b.Room)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guest == null)
                return NotFound(new { message = $"Guest with ID {id} not found." });

            return Ok(new
            {
                guest.Id,
                guest.FullName,
                guest.Email,
                guest.PhoneNumber,
                guest.IdNumber,
                guest.Address,
                guest.CreatedAt,
                BookingHistory = guest.Bookings.Select(b => new
                {
                    b.Id,
                    RoomNumber = b.Room.RoomNumber,
                    b.CheckInDate,
                    b.CheckOutDate,
                    b.Status,
                    b.TotalAmount
                })
            });
        }

        // POST: api/guests
        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> CreateGuest([FromBody] CreateGuestDto dto)
        {
            if (await _context.Guests.AnyAsync(g => g.Email == dto.Email))
                return BadRequest(new { message = "A guest with this email already exists." });

            var guest = new Guest
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IdNumber = dto.IdNumber,
                Address = dto.Address
            };

            _context.Guests.Add(guest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGuest), new { id = guest.Id }, new GuestResponseDto
            {
                Id = guest.Id,
                FullName = guest.FullName,
                Email = guest.Email,
                PhoneNumber = guest.PhoneNumber,
                IdNumber = guest.IdNumber,
                Address = guest.Address,
                CreatedAt = guest.CreatedAt
            });
        }

        // PUT: api/guests/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<IActionResult> UpdateGuest(int id, [FromBody] UpdateGuestDto dto)
        {
            var guest = await _context.Guests.FindAsync(id);

            if (guest == null)
                return NotFound(new { message = $"Guest with ID {id} not found." });

            guest.FullName = dto.FullName;
            guest.PhoneNumber = dto.PhoneNumber;
            guest.IdNumber = dto.IdNumber;
            guest.Address = dto.Address;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Guest updated successfully." });
        }

        // DELETE: api/guests/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGuest(int id)
        {
            var guest = await _context.Guests.FindAsync(id);

            if (guest == null)
                return NotFound(new { message = $"Guest with ID {id} not found." });

            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => b.GuestId == id &&
                          (b.Status == "Confirmed" || b.Status == "CheckedIn"));

            if (hasActiveBookings)
                return BadRequest(new { message = "Cannot delete guest with active bookings." });

            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Guest deleted successfully." });
        }
    }
}