using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
            var idToken = this.User.Claims.Single(c => c.Type == "id_token").Value;
            var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(idToken);
            var payloads =
                parsedToken.Payload.Claims
                    .Select(c => c.Type)
                    .Distinct()
                    .Select(ct => new KeyValuePair<string,string>(ct, parsedToken.Payload[ct].ToString()));
                    


            var model = new UserViewModel
            {
                User = this.User,
                Payload = payloads
            };
            return View(model);
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
