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

    public async Task<AuthResult> VerifyMfaAsync(string mfaToken, string code)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/mfa/verify", new { mfaToken, code });
        return await ParseResponse(response);
    }

    public async Task<MfaSetupInfo?> GetMfaSetupAsync(string jwt)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/mfa/setup");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<MfaSetupInfo>();
    }

    public async Task<MfaConfirmResult> ConfirmMfaSetupAsync(string jwt, string code)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/mfa/setup")
        {
            Content = JsonContent.Create(new { code }),
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
            return new MfaConfirmResult(true, []);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return new MfaConfirmResult(false, error?.Errors ?? ["Failed to enable MFA."]);
    }

    private static async Task<AuthResult> ParseResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (body is null)
                return AuthResult.Failure(["Empty response from backend."]);

            if (body.MfaRequired && body.MfaToken is not null)
                return AuthResult.MfaChallenge(body.MfaToken);

            if (body.Token is not null)
                return AuthResult.Success(body.Token);

            return AuthResult.Failure(["Unexpected response from backend."]);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return AuthResult.Failure(error?.Errors ?? ["Request failed."]);
    }

    private record LoginResponse(string? Token, bool MfaRequired, string? MfaToken);
    private record ErrorResponse(string[]? Errors);
}

public record MfaSetupInfo(string Secret, string OtpauthUri, bool Enabled);

public record MfaConfirmResult(bool Succeeded, string[] Errors);
