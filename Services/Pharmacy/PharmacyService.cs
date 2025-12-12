using HealthAidAPI.Data;
using HealthAidAPI.Models.Extras;
using HealthAidAPI.DTOs.Extras;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Services.Implementations
{
    public class PharmacyService : IPharmacyService
    {
        private readonly ApplicationDbContext _context;

        public PharmacyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MedicineSearchDto>> SearchMedicineAsync(string name)
        {
            var query = from inv in _context.PharmacyInventories
                        join fac in _context.MedicalFacilities on inv.MedicalFacilityId equals fac.Id
                        where inv.MedicineName.Contains(name) && inv.StockQuantity > 0
                        select new MedicineSearchDto
                        {
                            MedicineName = inv.MedicineName,
                            PharmacyName = fac.Name,
                            PharmacyAddress = fac.Address,
                            Stock = inv.StockQuantity,
                            Price = inv.Price
                        };

            return await query.ToListAsync();
        }

        public async Task<bool> UpdateStockAsync(UpdateStockDto dto)
        {
            var inventory = await _context.PharmacyInventories
                .FirstOrDefaultAsync(i => i.MedicalFacilityId == dto.FacilityId && i.MedicineName == dto.MedicineName);

            if (inventory == null)
            {
                inventory = new PharmacyInventory
                {
                    MedicalFacilityId = dto.FacilityId,
                    MedicineName = dto.MedicineName
                };
                _context.PharmacyInventories.Add(inventory);
            }

            inventory.StockQuantity = dto.NewQuantity;
            inventory.Price = dto.Price;
            inventory.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}