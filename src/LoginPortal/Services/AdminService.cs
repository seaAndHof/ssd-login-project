using System.Net.Http.Headers;
using System.Net.Http.Json;
using LoginPortal.Models;

namespace LoginPortal.Services;

public class AdminService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<UserInfo>> GetUsersAsync()
    {
        var jwt = _httpContextAccessor.HttpContext?.Request.Cookies["jwt"];
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        var users = await _httpClient.GetFromJsonAsync<List<UserInfo>>("api/auth/admin/users");
        return users ?? [];
    }
}
