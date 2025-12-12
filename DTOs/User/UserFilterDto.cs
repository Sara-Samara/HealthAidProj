namespace HealthAidAPI.DTOs.Users
{
    public class UserFilterDto
    {
        public string? Search { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public string? CurrentUserName { get; set; }
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}