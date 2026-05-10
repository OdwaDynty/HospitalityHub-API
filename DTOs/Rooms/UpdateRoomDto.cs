namespace HospitalityHub.API.DTOs.Rooms
{
    public class UpdateRoomDto
    {
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public string Status { get; set; } = string.Empty; // Available, Occupied, Maintenance
        public string? Description { get; set; }
    }
}