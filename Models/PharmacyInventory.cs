using System.ComponentModel.DataAnnotations.Schema; 

namespace HealthAidAPI.Models.Extras
{
    public class PharmacyInventory
    {
        public int Id { get; set; }
        public int MedicalFacilityId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }


        public bool IsAvailable => StockQuantity > 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}