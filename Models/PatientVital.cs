namespace HealthAidAPI.Models.Extras
{
    public class PatientVital
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public double HeartRate { get; set; }
        public double OxygenLevel { get; set; }
        public double BodyTemperature { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public bool IsCritical { get; set; } = false;
    }
}