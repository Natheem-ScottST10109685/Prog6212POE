using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ST10109685_PROGPOEPART2.Controllers
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }
    }


    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _configuration;

        public LoginController(ILogger<LoginController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate user credentials
                    bool isValidUser = await ValidateUser(model.Username, model.Password);

                    if (isValidUser)
                    {
                        // Check if the selected role matches the username
                        string expectedRole = GetUserRole(model.Username);
                        if (!string.Equals(expectedRole, model.Role, StringComparison.OrdinalIgnoreCase))
                        {
                            ModelState.AddModelError(string.Empty, $"The role '{model.Role}' is not associated with the username '{model.Username}'.");
                            return View(model);
                        }

                        // Create claims and sign in
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(ClaimTypes.Role, model.Role)
                };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        _logger.LogInformation($"User {model.Username} logged in as {model.Role} at {DateTime.UtcNow}");

                        // Redirect based on role
                        return model.Role switch
                        {
                            "Lecturer" => RedirectToAction("lecturerDashBoard", "Home"),
                            "ProgrammeCoordinator" => RedirectToAction("CoordDashboard", "Home"),
                            "AcademicManager" => RedirectToAction("CoordDashboard", "Home"),
                            "HR" => RedirectToAction("HRDashboard", "Home"),
                            _ => RedirectToAction("Index", "Home")
                        };

                    }

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    _logger.LogWarning($"Failed login attempt for user {model.Username}");
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login process");
                    ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                    return View(model);
                }
            }

            return View(model);
        }

        private async Task<bool> ValidateUser(string username, string password)
        {
            var validUsers = new Dictionary<string, (string password, string role)>
    {
        { "lecturer", ("lecturer123", "Lecturer") },
        { "coordinator", ("coordinator123", "ProgrammeCoordinator") },
        { "manager", ("manager123", "AcademicManager") },
        { "hr", ("hr123", "HR") }
    };

            return validUsers.ContainsKey(username.ToLower()) &&
                   validUsers[username.ToLower()].password == password;
        }

        private string GetUserRole(string username)
        {
            var validUsers = new Dictionary<string, string>
    {
        { "lecturer", "Lecturer" },
        { "coordinator", "ProgrammeCoordinator" },
        { "manager", "AcademicManager" },
        { "hr", "HR" }
    };

            return validUsers.TryGetValue(username.ToLower(), out string role) ? role : "User";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation($"User logged out at {DateTime.UtcNow}");
            return RedirectToAction("Login");
        }
    }
}
