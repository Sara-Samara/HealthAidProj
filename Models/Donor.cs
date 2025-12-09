using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.Models
{
    public class Donor
    {
        public int DonorId { get; set; }

        [Required]
        public string Organization { get; set; } = string.Empty;

        public decimal TotalDonated { get; set; }

        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}
