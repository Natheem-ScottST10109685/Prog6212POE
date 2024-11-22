using ST10109685_PROGPOEPART2.Models;

public class ClaimService : IClaimService
{
    private readonly string _claimDirectory;


    public ClaimService(string claimDirectory)
    {
        _claimDirectory = claimDirectory;
        Directory.CreateDirectory(_claimDirectory);
    }

    public List<ClaimModel> GetAllClaims()
    {
        return GetAllClaimsAsync().GetAwaiter().GetResult();
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
        // Logic for parsing claim details from file lines
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

    public ClaimModel GetClaimById(int id)
    {
        var claims = GetAllClaims();
        return claims.Find(c => c.Id == id) ?? throw new KeyNotFoundException($"Claim with ID {id} not found.");
    }

    public async Task<ClaimModel> GetClaimByIdAsync(int claimId)
    {
        var claims = await GetAllClaimsAsync();
        return claims.Find(c => c.Id == claimId) ?? throw new KeyNotFoundException($"Claim with ID {claimId} not found.");
    }

    public void UpdateClaim(ClaimModel claim)
    {
        string filePath = Path.Combine(_claimDirectory, claim.FileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Claim file not found: {claim.FileName}");

        var updatedContent = new[]
        {
            $"Lecturer Name: {claim.LecturerName}",
            $"Hours Worked: {claim.HoursWorked}",
            $"Hourly Rate: {claim.HourlyRate}",
            $"Additional Notes: {claim.AdditionalNotes}",
            $"Status: {claim.Status}"
        };

        File.WriteAllLines(filePath, updatedContent);
    }

    public async Task SubmitClaimAsync(ClaimModel claim, IFormFile supportingDocument)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"Claim_{timestamp}.txt";
        string filePath = Path.Combine(_claimDirectory, fileName);

        var content = new[]
        {
            $"Lecturer Name: {claim.LecturerName}",
            $"Hours Worked: {claim.HoursWorked}",
            $"Hourly Rate: {claim.HourlyRate}",
            $"Additional Notes: {claim.AdditionalNotes}",
            $"Status: Pending",
            $"Submitted On: {DateTime.Now}"
        };

        await File.WriteAllLinesAsync(filePath, content);

        if (supportingDocument != null && supportingDocument.Length > 0)
        {
            if (supportingDocument.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("File size exceeds 5MB limit.");

            string docFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_support{Path.GetExtension(supportingDocument.FileName)}";
            string docPath = Path.Combine(_claimDirectory, docFileName);

            using var stream = new FileStream(docPath, FileMode.Create);
            await supportingDocument.CopyToAsync(stream);

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

    public ClaimModel GetClaimByFileName(string fileName)
    {
        // Path to the claims directory
        string claimsDirectory = @"C:\Users\Natheem Scott\Desktop\2ndyear\New Content\PROG2B\LecturerClaims";

        // Combine the directory path with the file name to get the full file path
        string filePath = Path.Combine(claimsDirectory, fileName);

        // Check if the file exists
        if (!System.IO.File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file '{fileName}' does not exist in the claims directory.");
        }

        // Read all lines from the file
        string[] claimDetails = System.IO.File.ReadAllLines(filePath);

        // Parse the file content and create a ClaimModel object
        var claim = new ClaimModel
        {
            LecturerName = "Lecturer", 
            HoursWorked = int.Parse(claimDetails[0].Split(':')[1].Trim()),
            HourlyRate = decimal.Parse(claimDetails[1].Split(':')[1].Trim()),
            AdditionalNotes = claimDetails[2].Split(':')[1].Trim(),
            Status = claimDetails.Length > 3 ? claimDetails[3].Trim() : "Pending", // Default to 'Pending' if no status is present
            FileName = fileName
        };

        return claim;
    }

}
