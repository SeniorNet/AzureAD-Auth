using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace AzureADAuth.Controllers
{
    public class AccountController : Controller
    {
        [HttpPost]

        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = "/"
            });
        }

        [HttpPost]
        public IActionResult SignOut()
        {
            return SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = "/Account/SignedOut"
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult SignedOut() => View();
    }
}
