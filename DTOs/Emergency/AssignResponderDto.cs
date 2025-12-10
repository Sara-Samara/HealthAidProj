using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Emergency
{
    public class AssignResponderDto
    {
        [Required(ErrorMessage = "Responder ID is required")]
        public int ResponderId { get; set; }

        [Required(ErrorMessage = "Assigned By User ID is required")]
        public int AssignedByUserId { get; set; }
    }
}