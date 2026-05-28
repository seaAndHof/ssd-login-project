using System.Net.Http.Json;
using LoginPortal.Models;

namespace LoginPortal.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { username, password });
        return await ParseResponse(response);
    }

    public async Task<AuthResult> SignupAsync(string email, string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/signup", new { email, username, password });
        return await ParseResponse(response);
    }

    private static async Task<AuthResult> ParseResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var success = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return AuthResult.Success(success!.Token);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return AuthResult.Failure(error?.Errors ?? ["Request failed."]);
    }

    private record TokenResponse(string Token);
    private record ErrorResponse(string[]? Errors);
}
