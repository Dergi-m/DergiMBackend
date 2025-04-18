// Infrastructure/Startup/SecretsValidator.cs
public static class SecretsValidator
{
    public static void Validate(IConfiguration configuration, IWebHostEnvironment env)
    {
        string[] requiredKeys =
        [
            "ApiSettings:Issuer",
            "ApiSettings:Audience",
            "ApiSettings:SecretKey",
            "AzureSql:Server",
            "AzureSql:Database",
            "Clients:ClientOne:ClientId",
            "Clients:ClientOne:ClientSecret"
        ];

        List<string> missing = requiredKeys
            .Where(key => string.IsNullOrWhiteSpace(configuration[key]))
            .ToList();

        if (missing.Any())
        {
            throw new InvalidOperationException($"""
                ❌ Missing required configuration keys in {env.EnvironmentName} environment:
                {string.Join(Environment.NewLine, missing)}
                Make sure to set these in your appsettings.{env.EnvironmentName}.json or Azure App Service settings.
            """);
        }
    }
}
