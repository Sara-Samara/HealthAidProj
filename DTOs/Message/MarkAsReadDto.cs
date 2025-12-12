using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HealthAidAPI.DTOs.Messages
{
    public class MarkAsReadDto
    {
        [JsonIgnore]
        [SwaggerSchema(ReadOnly = true)]
        public int MyId { get; set; }     

        [Required(ErrorMessage = "OtherUserId is required")]
        public int OtherUserId { get; set; }
        [JsonIgnore]
        [SwaggerSchema(ReadOnly = true)]
        public bool IsRead { get; set; }
    }
}
