using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ST10109685_PROGPOEPART2.Controllers
{
    [Authorize(Policy = "LecturerOnly")]
    public class LecturerController : Controller
    {
        public IActionResult Claim() => View();
        public IActionResult ViewAll() => View();
        public IActionResult ClaimConfirmation() => View();
    }
}
