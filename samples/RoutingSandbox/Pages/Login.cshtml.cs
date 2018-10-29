using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RoutingSandbox.Pages
{
    public class LoginModel : PageModel
    {
        public async Task<IActionResult> OnGet(string name, string isAdmin)
        {
            if (!string.IsNullOrEmpty(name))
            {
                const string Issuer = "https://contoso.com";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, name, ClaimValueTypes.String, Issuer)
                };

                var role = isAdmin == "on" ? "admin" : "member";
                claims.Add(new Claim(ClaimTypes.Role, role, ClaimValueTypes.String, Issuer));

                var identity = new ClaimsIdentity("user");
                identity.AddClaims(claims);

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("Cookies", principal, new AuthenticationProperties()
                {
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                    IsPersistent = false,
                    AllowRefresh = false
                });

                return Redirect("/");
            }

            return Page();
        }
    }
}