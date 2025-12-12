namespace HealthAidAPI.DTOs.Doctors
{
    public class DoctorStatsDto
    {
        public int TotalDoctors { get; set; }
        public int AvailableDoctors { get; set; }
        public Dictionary<string, int> SpecializationsCount { get; set; } = new();
        public double AverageExperience { get; set; }
        public int NewDoctorsThisMonth { get; set; }
    }
}