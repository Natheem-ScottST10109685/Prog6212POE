using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ST10109685_PROGPOEPART2.Models;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ST10109685_PROGPOEPART2.Controllers
{
    [Authorize]
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

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Simulate authentication logic
            if ((username == "lecturer" && password == "lecturer123") ||
                (username == "coordinator" && password == "coordinator123") ||
                (username == "manager" && password == "manager123"))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, username == "lecturer" ? "Lecturer" :
                                              username == "coordinator" ? "ProgrammeCoordinator" :
                                                                          "AcademicManager")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Invalid username or password";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Lecturer")]
        public IActionResult lecturerDashBoard()
        {
            return View();
        }

        [Authorize(Roles = "Lecturer")]
        public IActionResult ViewAll()
        {
            string claimsDirectory = @"C:\Users\Natheem Scott\Desktop\2ndyear\New Content\PROG2B\LecturerClaims";
            var claims = new List<ClaimModel>();

            string[] claimFiles = Directory.GetFiles(claimsDirectory, "*.txt");

            foreach (var filePath in claimFiles)
            {
                string[] claimDetails = System.IO.File.ReadAllLines(filePath);

                try
                {
                    var claim = new ClaimModel
                    {
                        LecturerName = claimDetails[0].Split(':')[1].Trim(),
                        HoursWorked = ParseIntField(claimDetails, "Hours Worked"),
                        HourlyRate = ParseDecimalField(claimDetails, "Hourly Rate"),
                        AdditionalNotes = ParseStringField(claimDetails, "Additional Notes"),
                        SupportingDocument = ParseStringField(claimDetails, "Supporting Document"),
                        Status = ParseStringField(claimDetails, "Status") ?? "Pending",
                        FileName = Path.GetFileName(filePath)
                    };

                    claims.Add(claim);
                }
                catch (FormatException ex)
                {
                    Console.WriteLine($"Error parsing file {filePath}: {ex.Message}");
                }
            }

            return View(claims);
        }


        [Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
        public IActionResult CoordDashboard()
        {
            return View();
        }

        [Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
        public IActionResult CoordMange()
        {
            string claimsDirectory = @"C:\Users\Natheem Scott\Desktop\2ndyear\New Content\PROG2B\LecturerClaims";
            var claims = new List<ClaimModel>();

            // Get all text files in the directory
            string[] claimFiles = Directory.GetFiles(claimsDirectory, "*.txt");
            int lecturerCounter = 1;

            foreach (var filePath in claimFiles)
            {
                try
                {
                    string[] claimDetails = System.IO.File.ReadAllLines(filePath);

                    var claim = new ClaimModel
                    {
                        Id = lecturerCounter, // Use counter as ID
                        LecturerName = $"Lecturer {lecturerCounter}",
                        HoursWorked = ParseIntField(claimDetails, "Hours Worked"),
                        HourlyRate = ParseDecimalField(claimDetails, "Hourly Rate"),
                        AdditionalNotes = ParseStringField(claimDetails, "Additional Notes"),
                        SupportingDocument = ParseStringField(claimDetails, "Supporting Document"),
                        FileName = Path.GetFileName(filePath),
                        Status = ParseStringField(claimDetails, "Status") ?? "Pending"
                    };

                    claims.Add(claim);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {filePath}: {ex.Message}");
                }
                lecturerCounter++;
            }

            return View(claims);
        }

        // Helper methods to parse fields
        private int ParseIntField(string[] details, string fieldName)
        {
            var field = details.FirstOrDefault(line => line.StartsWith($"{fieldName}:"));
            return field != null ? int.Parse(field.Split(':')[1].Trim()) : 0;
        }

        private decimal ParseDecimalField(string[] details, string fieldName)
        {
            var field = details.FirstOrDefault(line => line.StartsWith($"{fieldName}:"));
            return field != null ? decimal.Parse(field.Split(':')[1].Trim()) : 0;
        }

        private string ParseStringField(string[] details, string fieldName)
        {
            var field = details.FirstOrDefault(line => line.StartsWith($"{fieldName}:"));
            return field != null ? field.Split(':')[1].Trim() : null;
        }


        [Authorize(Roles = "Lecturer")]
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
            return RedirectToAction("lecturerDashBoard");
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

            // Check if status is already set
            bool statusExists = claimDetails.Any(line => line.StartsWith("Status:"));

            // Update the status if not already set
            if (!statusExists)
            {
                string status = action.Equals("approve", StringComparison.OrdinalIgnoreCase) ? "Approved" : "Rejected";
                claimDetails.Add($"Status: {status}");

                // Write the updated content back to the file
                System.IO.File.WriteAllLines(filePath, claimDetails);
            }

            // Redirect to CoordMange to show updated claims
            return RedirectToAction("CoordMange");
        }

        [HttpPost]
        public IActionResult ApproveClaim(int id)
        {
            var claim = _claimService.GetClaimById(id);
            if (claim != null)
            {
                claim.Status = "Approved";
                _claimService.UpdateClaim(claim);
            }

            return Ok(); // Return 200 status to indicate success
        }

        [HttpPost]
        public IActionResult RejectClaim(int id)
        {
            var claim = _claimService.GetClaimById(id);
            if (claim != null)
            {
                claim.Status = "Rejected";
                _claimService.UpdateClaim(claim);
            }

            return Ok(); // Return 200 status to indicate success
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}