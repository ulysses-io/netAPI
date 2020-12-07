using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Breatheasy_API.Configuration;
using Breatheasy_API.Models;

namespace Breatheasy_API.Controllers
{
    
    public class AccountController : Controller
    {
        IOptions<AuthOptions> options;

        public AccountController(IOptions<AuthOptions> options)
        {
            this.options = options;
        }

        [HttpGet("auth")]
        public ActionResult<AuthOptions> GetPublicAuthSettings()
        {
            try
            {
                var dto = new AuthOptions()
                {
                    Audience = this.options.Value.Audience,
                    Domain = this.options.Value.Domain,
                    ClientId = this.options.Value.ClientId
                };
                return dto;
            }
            catch (Exception)
            {
                return new StatusCodeResult(500);
            }
        }
        public async Task Login(string returnUrl = "/")
        {
            await HttpContext.ChallengeAsync("Auth0", new AuthenticationProperties() { RedirectUri = returnUrl });
        }

        [Authorize]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Auth0", new AuthenticationProperties
            {
                // Indicate here where Auth0 should redirect the user after a logout.
                // Note that the resulting absolute Uri must be added to the
                // **Allowed Logout URLs** settings for the app.
                RedirectUri = Url.Action("Index", "Home")
            });
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
