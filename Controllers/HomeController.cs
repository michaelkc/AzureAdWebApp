using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AzureAdWebapp.Models;
using Microsoft.AspNetCore.Authorization;

namespace AzureAdWebapp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult About()
        {
            return View(this.User);
        }

        public IActionResult UserNotFound()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
