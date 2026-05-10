namespace HospitalityHub.API.DTOs.Guests
{
    public class UpdateGuestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? IdNumber { get; set; }
        public string? Address { get; set; }
    }
}