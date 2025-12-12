namespace HealthAidAPI.DTOs.Extras
{
    public class LogVitalDto
    {
        public double HeartRate { get; set; }
        public double OxygenLevel { get; set; }
        public double BodyTemperature { get; set; }
        public string DeviceId { get; set; } = "Unknown";
    }
}