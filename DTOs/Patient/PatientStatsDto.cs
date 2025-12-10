namespace HealthAidAPI.DTOs.Patients
{
    public class PatientStatsDto
    {
        public int TotalPatients { get; set; }
        public Dictionary<string, int> GenderDistribution { get; set; } = new();
        public Dictionary<string, int> BloodTypeDistribution { get; set; } = new();
        public double AverageAge { get; set; }
        public int PatientsWithMedicalHistory { get; set; }
        public int NewPatientsThisMonth { get; set; }
    }
}