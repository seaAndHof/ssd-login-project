using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPortal.Pages;

[Authorize(Roles = "Admin")]
public class AdminModel : PageModel
{
    public void OnGet() { }
}
