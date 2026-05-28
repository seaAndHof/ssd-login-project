namespace LoginPortal.Models;

public class AuthResult
{
    public bool Succeeded { get; private init; }
    public string? Token { get; private init; }
    public string[] Errors { get; private init; } = [];

    public static AuthResult Success(string token) => new() { Succeeded = true, Token = token };
    public static AuthResult Failure(string[] errors) => new() { Succeeded = false, Errors = errors };
}
