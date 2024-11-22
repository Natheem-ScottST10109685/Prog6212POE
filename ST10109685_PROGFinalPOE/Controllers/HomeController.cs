using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ST10109685_PROGPOEPART2.Models;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace ST10109685_PROGPOEPART2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string claimDirectory = @"Your own Local path";
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
                (username == "manager" && password == "manager123") ||
                (username == "hr" && password == "hr123")) // Add HR credentials
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, username == "lecturer" ? "Lecturer" :
                                      username == "coordinator" ? "ProgrammeCoordinator" :
                                      username == "manager" ? "AcademicManager" : "HR") 
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // Redirect to the correct dashboard based on role
                return claimsIdentity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value switch
                {
                    "Lecturer" => RedirectToAction("lecturerDashBoard", "Home"),
                    "ProgrammeCoordinator" => RedirectToAction("CoordDashboard", "Home"),
                    "AcademicManager" => RedirectToAction("CoordDashboard", "Home"),
                    "HR" => RedirectToAction("HRDashboard", "Home"), 
                    _ => RedirectToAction("Index", "Home")
                };
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

        /*--------------------------------------------------End of Home Page-------------------------------------------------------------------------------*/

        [Authorize(Roles = "Lecturer")]
        public IActionResult lecturerDashBoard()
        {
            return View();
        }

        [Authorize(Roles = "Lecturer")]
        public IActionResult ViewAll()
        {
            string claimsDirectory = @"Your own Local path";
            var claims = new List<ClaimModel>();

            string[] claimFiles = Directory.GetFiles(claimsDirectory, "*.txt");

            foreach (var filePath in claimFiles)
            {
                string[] claimDetails = System.IO.File.ReadAllLines(filePath);

                try
                {
                    var claim = new ClaimModel
                    {
                        LecturerName = ParseStringField(claimDetails, "Lecturer Name"),
                        HoursWorked = ParseIntField(claimDetails, "Hours Worked"),
                        HourlyRate = ParseDecimalField(claimDetails, "Hourly Rate"),
                        AdditionalNotes = ParseStringField(claimDetails, "Additional Notes"),
                        SupportingDocument = ParseStringField(claimDetails, "Supporting Document"),
                        Status = ParseStringField(claimDetails, "Status") ?? "Pending",
                        FileName = Path.GetFileName(filePath),
                        TotalAmount = (double)(ParseIntField(claimDetails, "Hours Worked") * ParseDecimalField(claimDetails, "Hourly Rate"))
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

        [Authorize(Roles = "Lecturer")]
        public IActionResult NewClaim()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitClaim(int hoursWorked, string hourlyRate, string notes, IFormFile supportingDocument)
        {
            // Parse the hourlyRate using InvariantCulture
            if (!decimal.TryParse(hourlyRate, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal hourlyRateValue) || hourlyRateValue <= 0)
            {
                ModelState.AddModelError("hourlyRate", "Invalid hourly rate. Please enter a valid decimal value.");
                return View("NewClaim");
            }

            // Validate hoursWorked
            if (hoursWorked <= 0)
            {
                ModelState.AddModelError("hoursWorked", "Hours worked must be greater than 0.");
                return View("NewClaim");
            }

            // Calculate the total amount
            decimal totalAmount = hoursWorked * hourlyRateValue;

            // Directory path where claims will be saved
            string claimDirectory = "Your own Local path";

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
                             $"Hourly Rate: {hourlyRateValue:F2}\n" +
                             $"Total Amount: {totalAmount:F2}\n" +
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
                    // Append the file name to the claim content
                    content += $"Supporting Document: {supportingDocument.FileName}\n";
                }
                else
                {
                    ModelState.AddModelError("FileSize", "The file size exceeds the 5MB limit.");
                    return View("NewClaim"); // Return view if the file is too large
                }
            }


            // Write the claim details to the text file
            System.IO.File.WriteAllText(filePath, content);

            // Redirect to the lecturer dashboard after successful submission
            return RedirectToAction("lecturerDashBoard");
        }

        /*--------------------------------------------------End of Lecturer-------------------------------------------------------------------------------*/

        [Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
        public IActionResult CoordDashboard()
        {
            return View();
        }

        [Authorize(Roles = "ProgrammeCoordinator,AcademicManager")]
        public IActionResult CoordMange()
        {
            string claimsDirectory = @"Your own Local path";
            var claims = new List<ClaimModel>();

            // Define verification rules
            const int maxHoursWorked = 40;
            const decimal minHourlyRate = 20.00m;
            const decimal maxHourlyRate = 200.00m;

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
                        LecturerName = ParseStringField(claimDetails, "Lecturer Name") ?? $"Lecturer {lecturerCounter}",
                        HoursWorked = ParseIntField(claimDetails, "Hours Worked"),
                        HourlyRate = ParseDecimalField(claimDetails, "Hourly Rate"),
                        AdditionalNotes = ParseStringField(claimDetails, "Additional Notes"),
                        SupportingDocument = ParseStringField(claimDetails, "Supporting Document"),
                        Status = ParseStringField(claimDetails, "Status") ?? "Pending",
                        FileName = Path.GetFileName(filePath),
                        TotalAmount = (double)(ParseIntField(claimDetails, "Hours Worked") * ParseDecimalField(claimDetails, "Hourly Rate")),
                        ContactInfo = ""
                    };

                    // Apply automation rules
                    if (claim.HoursWorked <= maxHoursWorked &&
                        claim.HourlyRate >= minHourlyRate &&
                        claim.HourlyRate <= maxHourlyRate)
                    {
                        claim.Status = "Auto-Approved";
                        UpdateClaimFileStatus(filePath, "Auto-Approved");
                    }
                    else
                    {
                        claim.Status = "Needs Review";
                    }

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

        // Helper method to update status in the claim file
        private void UpdateClaimFileStatus(string filePath, string newStatus)
        {
            var claimDetails = System.IO.File.ReadAllLines(filePath).ToList();
            var statusLineIndex = claimDetails.FindIndex(line => line.StartsWith("Status:"));

            if (statusLineIndex >= 0)
            {
                claimDetails[statusLineIndex] = $"Status: {newStatus}";
            }
            else
            {
                claimDetails.Add($"Status: {newStatus}");
            }

            System.IO.File.WriteAllLines(filePath, claimDetails);
        }

        // Helper methods to parse fields
        private int ParseIntField(string[] details, string fieldName)
        {
            var field = details.FirstOrDefault(line => line.StartsWith($"{fieldName}:"));
            if (field != null && int.TryParse(field.Split(':')[1].Trim(), out int result))
                return result;

            Console.WriteLine($"Failed to parse integer for field: {fieldName}");
            return 0;
        }

        private decimal ParseDecimalField(string[] details, string fieldName)
        {
            var field = details.FirstOrDefault(line => line.StartsWith($"{fieldName}:"));
            if (field != null && decimal.TryParse(field.Split(':')[1].Trim(), out decimal result))
                return result;

            Console.WriteLine($"Failed to parse decimal for field: {fieldName}");
            return 0;
        }

        private string ParseStringField(string[] details, string fieldName)
        {
            var field = details.FirstOrDefault(line => line.StartsWith($"{fieldName}:", StringComparison.OrdinalIgnoreCase));
            return field != null ? field.Split(':')[1].Trim() : null;
        }


        [HttpPost]
        public IActionResult ApproveReject(string fileName, string action)
        {
            string claimsDirectory = @"Your own Local path";
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

        /*--------------------------------------------------End of Manager and Coordinator-------------------------------------------------------------------------------*/

        [Authorize(Roles = "HR")]
        public IActionResult HRDashboard()
        {
            return View();
        }

        [Authorize(Roles = "HR")]
        public IActionResult ManageLecturerData()
        {
            try
            {
                var claimsDirectory = @"Your own Local path";
                var claims = new List<ClaimModel>();
                int lecturerCounter = 1;

                if (Directory.Exists(claimsDirectory))
                {
                    var claimFiles = Directory.GetFiles(claimsDirectory, "*.txt");

                    foreach (var filePath in claimFiles)
                    {
                        var claimDetails = System.IO.File.ReadAllLines(filePath);
                        var status = ParseStringField(claimDetails, "Status");

                        if (!string.IsNullOrEmpty(status) && (status.Equals("Auto-Approved", StringComparison.OrdinalIgnoreCase) ||
                                                            status.Equals("Approved", StringComparison.OrdinalIgnoreCase)))
                        {
                            var claim = new ClaimModel
                            {
                                Id = lecturerCounter++,
                                FileName = Path.GetFileName(filePath),
                                LecturerName = ParseStringField(claimDetails, "Lecturer Name") ?? $"Lecturer {lecturerCounter}",
                                HoursWorked = ParseIntField(claimDetails, "Hours Worked"),
                                HourlyRate = ParseDecimalField(claimDetails, "Hourly Rate"),
                                ContactInfo = ParseStringField(claimDetails, "Contact Info") ?? "Not provided",
                                TotalAmount = CalculateTotalAmount(
                                    ParseIntField(claimDetails, "Hours Worked"),
                                    ParseDecimalField(claimDetails, "Hourly Rate")
                                ),
                                Status = status
                            };

                            claims.Add(claim);
                        }
                    }
                }
                else
                {
                    _logger.LogError($"Claims directory not found: {claimsDirectory}");
                    ViewBag.ErrorMessage = "Claims directory not found. Please check the system configuration.";
                }

                return View(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in ManageLecturerData: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while processing the claims data.";
                return View(new List<ClaimModel>());
            }
        }

        // Add this helper method to calculate total amount
        private double CalculateTotalAmount(int hoursWorked, decimal hourlyRate)
        {
            return (double)(hoursWorked * hourlyRate);
        }

        // Update the UpdateLecturerData action to handle the updates properly
        [HttpPost]
        [Authorize(Roles = "HR")]
        public IActionResult UpdateLecturerData(string fileName, Dictionary<string, string> field)
        {
            try
            {
                string filePath = Path.Combine(@"Your own Local path", fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Claim file not found.");
                }

                var claimDetails = System.IO.File.ReadAllLines(filePath).ToList();

                // Update the fields in the file
                foreach (var key in field.Keys)
                {
                    var fieldIndex = claimDetails.FindIndex(line => line.StartsWith($"{key}:", StringComparison.OrdinalIgnoreCase));

                    if (fieldIndex != -1)
                    {
                        claimDetails[fieldIndex] = $"{key}: {field[key]}";
                    }
                    else
                    {
                        claimDetails.Add($"{key}: {field[key]}");
                    }
                }

                System.IO.File.WriteAllLines(filePath, claimDetails);
                return Ok(new { message = "Data updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating lecturer data: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the data.");
            }
        }


        [Authorize(Roles = "HR")]
        public IActionResult GenerateReports()
        {
            try
            {
                var claimsDirectory = @"Your own Local path";
                var claims = new List<ClaimModel>();
                int lecturerCounter = 1;

                if (Directory.Exists(claimsDirectory))
                {
                    var claimFiles = Directory.GetFiles(claimsDirectory, "*.txt");

                    foreach (var filePath in claimFiles)
                    {
                        var claimDetails = System.IO.File.ReadAllLines(filePath);
                        var status = ParseStringField(claimDetails, "Status");

                        // Include all approved claims in the report
                        if (!string.IsNullOrEmpty(status) && (status.Equals("Auto-Approved", StringComparison.OrdinalIgnoreCase) ||
                                                            status.Equals("Approved", StringComparison.OrdinalIgnoreCase)))
                        {
                            var claim = new ClaimModel
                            {
                                Id = lecturerCounter++,
                                FileName = Path.GetFileName(filePath),
                                LecturerName = ParseStringField(claimDetails, "Lecturer Name") ?? $"Lecturer {lecturerCounter}",
                                HoursWorked = ParseIntField(claimDetails, "Hours Worked"),
                                HourlyRate = ParseDecimalField(claimDetails, "Hourly Rate"),
                                ContactInfo = ParseStringField(claimDetails, "Contact Info") ?? "Not provided",
                                TotalAmount = CalculateTotalAmount(
                                    ParseIntField(claimDetails, "Hours Worked"),
                                    ParseDecimalField(claimDetails, "Hourly Rate")
                                ),
                                Status = status
                            };

                            claims.Add(claim);
                        }
                    }
                }
                else
                {
                    _logger.LogError($"Claims directory not found: {claimsDirectory}");
                    ViewBag.ErrorMessage = "Claims directory not found. Please check the system configuration.";
                }

                return View(claims);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating reports: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred while generating the reports.";
                return View(new List<ClaimModel>());
            }
        }


        /*--------------------------------------------------End of Human Resources-------------------------------------------------------------------------------*/

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
