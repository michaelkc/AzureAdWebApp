using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Authentication;

namespace AzureAdWebapp.Models
{
    public class UserViewModel
    {
        public ClaimsPrincipal User { get; set; }
        public AuthenticationProperties AuthenticationProperties { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Payload { get; set; }
    }
}
