using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PawPairs.Web.Auth;

namespace PawPairs.Web.Pages.Auth;

public class LogoutModel : PageModel
{
    public IActionResult OnPost()
    {
        SessionAuth.SignOut(HttpContext);
        return RedirectToPage("/Index");
    }
}