using LoginPortal.Models;
using LoginPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LoginPortal.Pages;

[Authorize(Roles = "Admin")]
public class AdminModel : PageModel
{
    private readonly AdminService _adminService;

    public AdminModel(AdminService adminService)
    {
        _adminService = adminService;
    }

    public List<UserInfo> Users { get; set; } = [];

    public async Task OnGetAsync()
    {
        Users = await _adminService.GetUsersAsync();
    }
}
