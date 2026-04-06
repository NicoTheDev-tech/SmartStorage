using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SmartStorage.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Customer")]
        public IActionResult CustomerDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Staff")]
        public IActionResult StaffDashboard()
        {
            return View();
        }
    }
}