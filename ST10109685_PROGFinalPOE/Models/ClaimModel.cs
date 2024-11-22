using System.ComponentModel.DataAnnotations;

namespace ST10109685_PROGPOEPART2.Models
{
    public class ClaimModel
    {
        public int Id { get; set; }
        public string LecturerName { get; set; }
        public int HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public double TotalAmount { get; set; }
        public string AdditionalNotes { get; set; }
        public string SupportingDocument { get; set; }
        public string Status { get; set; }
        public string FileName { get; set; }
        public string ContactInfo { get; set; }
    }

    // The IClaimService interface
    public interface IClaimService
    {
        List<ClaimModel> GetAllClaims(); // Fetch all claims synchronously
        Task<List<ClaimModel>> GetAllClaimsAsync(); // Fetch all claims asynchronously
        ClaimModel GetClaimByFileName(string fileName);
        ClaimModel GetClaimById(int id); // Get a specific claim by ID
        void UpdateClaim(ClaimModel claim); // Update claim details
    }
}
