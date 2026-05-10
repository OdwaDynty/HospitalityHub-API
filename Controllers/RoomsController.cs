using HospitalityHub.API.Data;
using HospitalityHub.API.DTOs.Rooms;
using HospitalityHub.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalityHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/rooms
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllRooms([FromQuery] string? status, [FromQuery] string? roomType)
        {
            var query = _context.Rooms.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            if (!string.IsNullOrEmpty(roomType))
                query = query.Where(r => r.RoomType == roomType);

            var rooms = await query.Select(r => new RoomResponseDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                PricePerNight = r.PricePerNight,
                Status = r.Status,
                Description = r.Description
            }).ToListAsync();

            return Ok(rooms);
        }

        // GET: api/rooms/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound(new { message = $"Room with ID {id} not found." });

            return Ok(new RoomResponseDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                PricePerNight = room.PricePerNight,
                Status = room.Status,
                Description = room.Description
            });
        }

        // POST: api/rooms
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto dto)
        {
            // Check if room number already exists
            if (await _context.Rooms.AnyAsync(r => r.RoomNumber == dto.RoomNumber))
                return BadRequest(new { message = $"Room number {dto.RoomNumber} already exists." });

            var room = new Room
            {
                RoomNumber = dto.RoomNumber,
                RoomType = dto.RoomType,
                PricePerNight = dto.PricePerNight,
                Description = dto.Description,
                Status = "Available"
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, new RoomResponseDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                PricePerNight = room.PricePerNight,
                Status = room.Status,
                Description = room.Description
            });
        }

        // PUT: api/rooms/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomDto dto)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound(new { message = $"Room with ID {id} not found." });

            room.RoomType = dto.RoomType;
            room.PricePerNight = dto.PricePerNight;
            room.Status = dto.Status;
            room.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Room updated successfully." });
        }

        // DELETE: api/rooms/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound(new { message = $"Room with ID {id} not found." });

            // Prevent deletion if room has active bookings
            var hasActiveBookings = await _context.Bookings
                .AnyAsync(b => b.RoomId == id &&
                          (b.Status == "Confirmed" || b.Status == "CheckedIn"));

            if (hasActiveBookings)
                return BadRequest(new { message = "Cannot delete room with active bookings." });

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Room deleted successfully." });
        }
    }
}