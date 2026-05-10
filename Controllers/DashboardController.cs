using HospitalityHub.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalityHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Receptionist")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboard/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;

            var totalRooms = await _context.Rooms.CountAsync();
            var occupiedRooms = await _context.Rooms.CountAsync(r => r.Status == "Occupied");
            var availableRooms = await _context.Rooms.CountAsync(r => r.Status == "Available");
            var maintenanceRooms = await _context.Rooms.CountAsync(r => r.Status == "Maintenance");

            var totalGuests = await _context.Guests.CountAsync();
            var totalBookings = await _context.Bookings.CountAsync();

            var activeBookings = await _context.Bookings
                .CountAsync(b => b.Status == "Confirmed" || b.Status == "CheckedIn");

            var todayCheckIns = await _context.Bookings
                .CountAsync(b => b.CheckInDate.Date == today && b.Status == "Confirmed");

            var todayCheckOuts = await _context.Bookings
                .CountAsync(b => b.CheckOutDate.Date == today && b.Status == "CheckedIn");

            var totalRevenue = await _context.Bookings
                .Where(b => b.Status == "CheckedOut")
                .SumAsync(b => b.TotalAmount);

            var occupancyRate = totalRooms > 0
                ? Math.Round((double)occupiedRooms / totalRooms * 100, 1)
                : 0;

            return Ok(new
            {
                Rooms = new
                {
                    Total = totalRooms,
                    Occupied = occupiedRooms,
                    Available = availableRooms,
                    Maintenance = maintenanceRooms,
                    OccupancyRate = $"{occupancyRate}%"
                },
                Guests = new
                {
                    Total = totalGuests
                },
                Bookings = new
                {
                    Total = totalBookings,
                    Active = activeBookings,
                    TodayCheckIns = todayCheckIns,
                    TodayCheckOuts = todayCheckOuts
                },
                Revenue = new
                {
                    Total = totalRevenue,
                    Currency = "ZAR"
                }
            });
        }

        // GET: api/dashboard/today
        [HttpGet("today")]
        public async Task<IActionResult> GetTodayActivity()
        {
            var today = DateTime.UtcNow.Date;

            var checkIns = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Guest)
                .Where(b => b.CheckInDate.Date == today && b.Status == "Confirmed")
                .Select(b => new
                {
                    b.Id,
                    GuestName = b.Guest.FullName,
                    RoomNumber = b.Room.RoomNumber,
                    RoomType = b.Room.RoomType,
                    b.CheckInDate,
                    b.CheckOutDate,
                    b.TotalAmount
                }).ToListAsync();

            var checkOuts = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Guest)
                .Where(b => b.CheckOutDate.Date == today && b.Status == "CheckedIn")
                .Select(b => new
                {
                    b.Id,
                    GuestName = b.Guest.FullName,
                    RoomNumber = b.Room.RoomNumber,
                    RoomType = b.Room.RoomType,
                    b.CheckInDate,
                    b.CheckOutDate,
                    b.TotalAmount
                }).ToListAsync();

            return Ok(new
            {
                Date = today.ToString("yyyy-MM-dd"),
                CheckIns = checkIns,
                CheckOuts = checkOuts
            });
        }

        // GET: api/dashboard/revenue
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueReport()
        {
            var revenueByRoomType = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.Status == "CheckedOut")
                .GroupBy(b => b.Room.RoomType)
                .Select(g => new
                {
                    RoomType = g.Key,
                    TotalBookings = g.Count(),
                    TotalRevenue = g.Sum(b => b.TotalAmount)
                }).ToListAsync();

            var monthlyRevenue = await _context.Bookings
                .Where(b => b.Status == "CheckedOut" &&
                       b.CheckOutDate.Year == DateTime.UtcNow.Year)
                .GroupBy(b => b.CheckOutDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalRevenue = g.Sum(b => b.TotalAmount),
                    TotalBookings = g.Count()
                })
                .OrderBy(g => g.Month)
                .ToListAsync();

            return Ok(new
            {
                RevenueByRoomType = revenueByRoomType,
                MonthlyRevenue = monthlyRevenue
            });
        }
    }
}