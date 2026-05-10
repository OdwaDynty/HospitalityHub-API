namespace HospitalityHub.API.DTOs.Rooms
{
    public class CreateRoomDto
    {
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty; // Single, Double, Suite
        public decimal PricePerNight { get; set; }
        public string? Description { get; set; }
    }
}