namespace HospitalityHub.API.DTOs.Bookings
{
    public class UpdateBookingStatusDto
    {
        public string Status { get; set; } = string.Empty; // Confirmed, CheckedIn, CheckedOut, Cancelled
    }
}