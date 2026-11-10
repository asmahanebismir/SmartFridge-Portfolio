using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SmartFridgeBackend.Database.Models;
using SmartFridgeBackend.Database.Repositories;

namespace BackendTests;


public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestDatabaseName = "SmartFridgeTestDb";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            var mongoDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IMongoDatabase));

            if (mongoDescriptor != null)
            {
                services.Remove(mongoDescriptor);
            }

            var mongoClientDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IMongoClient));

            if (mongoClientDescriptor != null)
            {
                services.Remove(mongoClientDescriptor);
            }

            var client = new MongoClient("mongodb://localhost:27017");
            services.AddSingleton<IMongoClient>(client);
            services.AddSingleton(sp => client.GetDatabase(TestDatabaseName));

            services.AddScoped<IRepository<User>, MongoRepository<User>>(sp =>
                new MongoRepository<User>(
                    sp.GetRequiredService<IMongoDatabase>(),
                    "Users"));

            services.AddScoped<IRepository<Item>, MongoRepository<Item>>(sp =>
                new MongoRepository<Item>(
                    sp.GetRequiredService<IMongoDatabase>(),
                    "Items"));

            services.AddScoped<IRepository<Recipe>, MongoRepository<Recipe>>(sp =>
                new MongoRepository<Recipe>(
                    sp.GetRequiredService<IMongoDatabase>(),
                    "Recipes"));
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

        await db.DropCollectionAsync("Users");
        await db.DropCollectionAsync("Items");
        await db.DropCollectionAsync("Recipes");
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim("uid", "test-firebase-uid"),
            new Claim("email_verified", "true"),
            new Claim(ClaimTypes.Name, "integration-test-user")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}