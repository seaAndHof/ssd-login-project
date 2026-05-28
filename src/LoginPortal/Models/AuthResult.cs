namespace LoginPortal.Models;

public class AuthResult
{
    public bool Succeeded { get; private init; }
    public string? Token { get; private init; }
    public bool MfaRequired { get; private init; }
    public string? MfaToken { get; private init; }
    public string[] Errors { get; private init; } = [];

    public static AuthResult Success(string token) =>
        new() { Succeeded = true, Token = token };

    public static AuthResult MfaChallenge(string mfaToken) =>
        new() { Succeeded = false, MfaRequired = true, MfaToken = mfaToken };

    public static AuthResult Failure(string[] errors) =>
        new() { Succeeded = false, Errors = errors };
}
