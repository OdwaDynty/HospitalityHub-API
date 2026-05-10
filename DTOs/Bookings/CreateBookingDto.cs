namespace HospitalityHub.API.DTOs.Bookings
{
    public class CreateBookingDto
    {
        public int RoomId { get; set; }
        public int GuestId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}