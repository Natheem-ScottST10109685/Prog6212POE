using Microsoft.AspNetCore.Http;
using ST10109685_PROGPOEPART2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ST10109685_PROGPOEPART2.Services
{
    public class ClaimService : IClaimService
    {
        private readonly string _claimDirectory;

        public ClaimService(string claimDirectory)
        {
            _claimDirectory = claimDirectory;
            Directory.CreateDirectory(_claimDirectory); // Ensure directory exists
        }

        public async Task<List<ClaimModel>> GetAllClaimsAsync()
        {
            var claims = new List<ClaimModel>();
            var files = Directory.GetFiles(_claimDirectory, "*.txt");

            foreach (var filePath in files)
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                var claim = await ParseClaimFromFileAsync(lines, filePath);
                claims.Add(claim);
            }

            return claims;
        }

        private async Task<ClaimModel> ParseClaimFromFileAsync(string[] lines, string filePath)
        {
            var claim = new ClaimModel
            {
                FileName = Path.GetFileName(filePath),
                Status = "Pending" // Default status
            };

            foreach (var line in lines)
            {
                var parts = line.Split(':', 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "Lecturer Name":
                        claim.LecturerName = value;
                        break;
                    case "Hours Worked":
                        if (int.TryParse(value, out int hours))
                            claim.HoursWorked = hours;
                        break;
                    case "Hourly Rate":
                        if (decimal.TryParse(value, out decimal rate))
                            claim.HourlyRate = rate;
                        break;
                    case "Additional Notes":
                        claim.AdditionalNotes = value;
                        break;
                    case "Status":
                        claim.Status = value;
                        break;
                }
            }

            return claim;
        }

        public async Task<ClaimModel> GetClaimByIdAsync(int claimId)
        {
            var claims = await GetAllClaimsAsync();
            return claims.Find(c => c.Id == claimId) ??
                   throw new KeyNotFoundException($"Claim with ID {claimId} not found.");
        }

        public async Task SubmitClaimAsync(ClaimModel claim, IFormFile supportingDocument)
        {
            // Generate unique filename with timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"Claim_{timestamp}.txt";
            string filePath = Path.Combine(_claimDirectory, fileName);

            // Prepare claim content
            var content = new[]
            {
                $"Lecturer Name: {claim.LecturerName}",
                $"Hours Worked: {claim.HoursWorked}",
                $"Hourly Rate: {claim.HourlyRate}",
                $"Additional Notes: {claim.AdditionalNotes}",
                $"Status: Pending",
                $"Submitted On: {DateTime.Now}"
            };

            // Save claim details
            await File.WriteAllLinesAsync(filePath, content);

            // Handle supporting document
            if (supportingDocument != null && supportingDocument.Length > 0)
            {
                if (supportingDocument.Length > 5 * 1024 * 1024)
                    throw new InvalidOperationException("File size exceeds 5MB limit.");

                string docFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_support{Path.GetExtension(supportingDocument.FileName)}";
                string docPath = Path.Combine(_claimDirectory, docFileName);

                using (var stream = new FileStream(docPath, FileMode.Create))
                {
                    await supportingDocument.CopyToAsync(stream);
                }

                // Add document reference to claim file
                await File.AppendAllTextAsync(filePath, $"\nSupporting Document: {docFileName}");
            }
        }

        public async Task ApproveClaimAsync(int claimId)
        {
            await UpdateClaimStatusAsync(claimId, "Approved");
        }

        public async Task RejectClaimAsync(int claimId)
        {
            await UpdateClaimStatusAsync(claimId, "Rejected");
        }

        private async Task UpdateClaimStatusAsync(int claimId, string newStatus)
        {
            var claim = await GetClaimByIdAsync(claimId);
            string filePath = Path.Combine(_claimDirectory, claim.FileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Claim file not found: {claim.FileName}");

            var lines = await File.ReadAllLinesAsync(filePath);
            var updatedLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.StartsWith("Status:"))
                    updatedLines.Add($"Status: {newStatus}");
                else
                    updatedLines.Add(line);
            }

            await File.WriteAllLinesAsync(filePath, updatedLines);
        }
    }
}