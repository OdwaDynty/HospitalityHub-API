namespace HospitalityHub.API.DTOs.Rooms
{
    public class RoomResponseDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}