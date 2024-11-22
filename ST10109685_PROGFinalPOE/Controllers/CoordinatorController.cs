using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ST10109685_PROGPOEPART2.Controllers
{
    [Authorize(Policy = "CoordinatorOnly")]
    public class CoordinatorController : Controller
    {
        public IActionResult CoordDashboard() => View();
        public IActionResult CoordMange() => View();
    }
}
