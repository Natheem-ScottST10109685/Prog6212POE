using System.ComponentModel.DataAnnotations;

namespace ST10109685_PROGPOEPART2.Models
{
    public class ClaimModel
    {
        public int Id { get; set; }

        [Required]
        public string LecturerName { get; set; }

        [Required]
        [Range(1, 168)] // Max hours in a week
        public int HoursWorked { get; set; }

        [Required]
        [Range(0.01, 10000)] // Reasonable range for hourly rate
        public decimal HourlyRate { get; set; }

        public string AdditionalNotes { get; set; }
        public string Status { get; set; }
        public string FileName { get; set; }
    }

    // Then, let's define the IClaimService interface
    public interface IClaimService
    {
        ClaimModel GetClaimById(int id);
        void UpdateClaim(ClaimModel claim);
    }
}
