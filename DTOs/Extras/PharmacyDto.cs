using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Extras
{
    public class MedicineSearchDto
    {
        public string MedicineName { get; set; } = string.Empty;
        public string PharmacyName { get; set; } = string.Empty;

        public string PharmacyAddress { get; set; } = string.Empty;

        public int Stock { get; set; }
        public decimal Price { get; set; }
    }

    public class UpdateStockDto
    {
        [Required]
        public int FacilityId { get; set; }

        [Required]
        public string MedicineName { get; set; } = string.Empty;

        [Required]
        public int NewQuantity { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}