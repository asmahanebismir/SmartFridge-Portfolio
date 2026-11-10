using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using SmartFridgeBackend.Authentication;
using SmartFridgeBackend.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();


// Firebase initialisering
FirebaseInitializer.Initialize();

// MongoDB
var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"];
var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"];

builder.Services.AddSingleton<IMongoClient>(
    new MongoClient(mongoConnectionString)
);
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

// Authentication med Firebase
builder.Services.AddAuthentication("Firebase")
    .AddScheme<FirebaseAuthenticationOptions, FirebaseAuthenticationHandler>(
        "Firebase", _ => { }
    );

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmailVerified", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
            context.User.HasClaim(claim => 
                claim.Type == "email_verified" &&
                string.Equals(claim.Value, "true", StringComparison.OrdinalIgnoreCase)));
    });
}
);
builder.Services.AddHttpClient<FirebaseAuthService>();
builder.Services.AddScoped<IGroceriesService, GroceriesService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://smartfridge-frontend-1dfw.onrender.com" //Ændres senere til deplyede URL. 
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Controllers og Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartFridge API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your Firebase ID token like this: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// AI recipe generator:
builder.Services.AddHttpClient("openai", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    client.Timeout = TimeSpan.FromMinutes(3); // More groceries makes the response to slow. Time out happened at 100 sec, now increased to 180. HOT FIX for now.
});

builder.Services.AddScoped<IRecipeGenerationService, OpenAiRecipeGenerationService>();
builder.Services.AddScoped<IRecipeFavoritesService, RecipeFavoritesService>();

// Repository registrering (Dependency Injection)
// AddScoped = én instans per HTTP-request
builder.Services.AddScoped<
    SmartFridgeBackend.Database.Repositories.IRepository<SmartFridgeBackend.Database.Models.User>,
    SmartFridgeBackend.Database.Repositories.MongoRepository<SmartFridgeBackend.Database.Models.User>>(
    sp => new SmartFridgeBackend.Database.Repositories.MongoRepository<SmartFridgeBackend.Database.Models.User>(
        sp.GetRequiredService<IMongoDatabase>(), "Users"));

builder.Services.AddScoped<
    SmartFridgeBackend.Database.Repositories.IRepository<SmartFridgeBackend.Database.Models.Item>,
    SmartFridgeBackend.Database.Repositories.MongoRepository<SmartFridgeBackend.Database.Models.Item>>(
    sp => new SmartFridgeBackend.Database.Repositories.MongoRepository<SmartFridgeBackend.Database.Models.Item>(
        sp.GetRequiredService<IMongoDatabase>(), "Items"));

builder.Services.AddScoped<
    SmartFridgeBackend.Database.Repositories.IRepository<SmartFridgeBackend.Database.Models.Recipe>,
    SmartFridgeBackend.Database.Repositories.MongoRepository<SmartFridgeBackend.Database.Models.Recipe>>(
    sp => new SmartFridgeBackend.Database.Repositories.MongoRepository<SmartFridgeBackend.Database.Models.Recipe>(
        sp.GetRequiredService<IMongoDatabase>(), "Recipes"));

// Transaction Manager
builder.Services.AddScoped<
    SmartFridgeBackend.Database.Transactions.ITransactionManager,
    SmartFridgeBackend.Database.Transactions.MongoTransactionManager>();

try
{
    Log.Information("Starting Smartfridge backend");
    var app = builder.Build();

    // Middleware pipeline — rækkefølgen er kritisk
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/", () => Results.Ok("SmartFridge backend is running")); //Healt check for render
    app.MapGet("/healthz", () => Results.Ok("Healthy"));
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }