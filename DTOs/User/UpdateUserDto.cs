namespace HealthAidAPI.DTOs.Users
{
    public class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
    }
}