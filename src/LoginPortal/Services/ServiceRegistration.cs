namespace LoginPortal.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddBackendServices(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = new Uri(configuration["Backend:BaseUrl"]!);

        services.AddHttpContextAccessor();

        services.AddHttpClient<AuthService>(client =>
        {
            client.BaseAddress = baseUrl;
        });

        services.AddHttpClient<AdminService>(client =>
        {
            client.BaseAddress = baseUrl;
        });

        return services;
    }
}
