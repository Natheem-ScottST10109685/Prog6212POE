using Microsoft.AspNetCore.Mvc;
using ST10109685_PROGPOEPART2.Models;
using System.Diagnostics;

namespace ST10109685_PROGPOEPART2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string claimDirectory = @"C:\Users\Natheem Scott\Desktop\2ndyear\New Content\PROG2B\LecturerClaims";
        private readonly IClaimService _claimService;

        public HomeController(ILogger<HomeController> logger, IClaimService claimService)
        {
            _logger = logger;
            _claimService = claimService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Claim()
        {
            return View();
        }

        public IActionResult CoordDashboard()
        {
            return View();
        }

        public IActionResult CoordMange()
        {
            string claimsDirectory = @"C:\Users\Natheem Scott\Desktop\2ndyear\New Content\PROG2B\LecturerClaims";
            var claims = new List<ClaimModel>();

            string[] claimFiles = Directory.GetFiles(claimsDirectory, "*.txt");
            int lecturerCounter = 1;

            foreach (var filePath in claimFiles)
            {
                string[] claimDetails = System.IO.File.ReadAllLines(filePath);

                // Proper parsing of string values to correct types
                int hoursWorked;
                decimal hourlyRate;

                // Use TryParse to safely convert strings to numbers
                int.TryParse(claimDetails[0], out hoursWorked);
                decimal.TryParse(claimDetails[1], out hourlyRate);

                var claim = new ClaimModel
                {
                    LecturerName = "Lecturer " + lecturerCounter,
                    HoursWorked = hoursWorked,
                    HourlyRate = hourlyRate,
                    AdditionalNotes = claimDetails[2],
                    FileName = Path.GetFileName(filePath)
                };

                claims.Add(claim);
                lecturerCounter++;
            }

            return View(claims);
        }

        public IActionResult NewClaim()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitClaim(int hoursWorked, decimal hourlyRate, string notes, IFormFile supportingDocument)
        {
            // Directory path where claims will be saved
            string claimDirectory = "C:\\Users\\Natheem Scott\\Desktop\\2ndyear\\New Content\\PROG2B\\LecturerClaims";

            // Ensure the directory exists
            if (!Directory.Exists(claimDirectory))
            {
                Directory.CreateDirectory(claimDirectory);
            }

            // Create a unique file name with a timestamp for the claim text file
            string fileName = $"Claim_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(claimDirectory, fileName);

            // Prepare claim details content to be written to the text file
            string content = $"Hours Worked: {hoursWorked}\n" +
                             $"Hourly Rate: {hourlyRate}\n" +
                             $"Additional Notes: {notes}\n" +
                             $"Submitted On: {DateTime.Now}\n";

            // Handle file upload if there's a supporting document
            if (supportingDocument != null && supportingDocument.Length > 0)
            {
                // Define the path to save the uploaded document
                var documentPath = Path.Combine(claimDirectory, supportingDocument.FileName);

                // Restrict file size to 5MB
                if (supportingDocument.Length <= 5 * 1024 * 1024)
                {
                    using (var stream = new FileStream(documentPath, FileMode.Create))
                    {
                        supportingDocument.CopyTo(stream);
                    }
                    // Append the file link to the claim content
                    content += $"Supporting Document: {supportingDocument.FileName}\n";
                }
                else
                {
                    ModelState.AddModelError("FileSize", "The file size exceeds the 5MB limit.");
                    return View("NewClaim"); // Return view if the file is too large
                }
            }

            // Write the claim details (and document link if any) to the text file
            System.IO.File.WriteAllText(filePath, content);

            // Redirect to confirmation page
            return RedirectToAction("ClaimConfirmation");
        }

        public IActionResult ClaimConfirmation()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ApproveReject(string fileName, string action)
        {
            string claimsDirectory = @"C:\Users\Natheem Scott\Desktop\2ndyear\New Content\PROG2B\LecturerClaims";
            string filePath = Path.Combine(claimsDirectory, fileName);

            // Read the existing content
            var claimDetails = System.IO.File.ReadAllLines(filePath).ToList();

            // Add approval or rejection status at the end
            claimDetails.Add(action == "approve" ? "Approved" : "Rejected");

            // Write the updated content back to the file
            System.IO.File.WriteAllLines(filePath, claimDetails);

            // Redirect to the confirmation view
            return View("CoordDashboard");
        }

        [HttpPost]
        public IActionResult ApproveClaim(int id)
        {
            // Update the claim status to 'Approved'
            var claim = _claimService.GetClaimById(id);
            claim.Status = "Approved";
            _claimService.UpdateClaim(claim);

            return Ok();
        }

        [HttpPost]
        public IActionResult RejectClaim(int id)
        {
            // Update the claim status to 'Rejected'
            var claim = _claimService.GetClaimById(id);
            claim.Status = "Rejected";
            _claimService.UpdateClaim(claim);

            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
