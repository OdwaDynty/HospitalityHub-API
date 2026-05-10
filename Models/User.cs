namespace HospitalityHub.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Guest"; // Admin, Receptionist, Guest
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
