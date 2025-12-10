using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.NGOs
{
    public class UpdateNgoDto
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Organization name must be between 3 and 100 characters")]
        public string? OrganizationName { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Area of work must be between 3 and 100 characters")]
        public string? AreaOfWork { get; set; }

        [StringLength(20, ErrorMessage = "Verified status cannot exceed 20 characters")]
        public string? VerifiedStatus { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "Contact person must be between 3 and 50 characters")]
        public string? ContactedPerson { get; set; }
    }
}