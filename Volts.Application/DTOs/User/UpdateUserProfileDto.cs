namespace Volts.Application.DTOs.User
{
    public class UpdateUserProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Gender { get; set; } = string.Empty;
    }
}
