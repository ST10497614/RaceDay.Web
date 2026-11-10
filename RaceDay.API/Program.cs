using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RaceDay.API.Data;
using RaceDay.API.Services;
using Azure.Storage.Blobs;
using System.Text;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is missing.");

        var jwtIssuer = builder.Configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is missing.");

        var jwtAudience = builder.Configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience is missing.");

        // 1. Register Services
        builder.Services.AddControllers();
        builder.Services.AddScoped<TokenService>();
        builder.Services.AddScoped<IEventBannerStorageService, EventBannerStorageService>();
        var blobConnectionString = builder.Configuration["BlobStorage:ConnectionString"]
            ?? throw new InvalidOperationException("BlobStorage connection string is missing. Set BlobStorage__ConnectionString.");
        builder.Services.AddSingleton(new BlobServiceClient(blobConnectionString));
        builder.Services.AddScoped<PasswordService>();
        builder.Services.AddEndpointsApiExplorer();

        // Swagger with Bearer token support
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token here."
                });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
            });
        });

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession();

        // JWT authentication
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtIssuer,

                        ValidateAudience = true,
                        ValidAudience = jwtAudience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine(
                            "JWT authentication failed: " +
                            context.Exception.GetType().Name + ": " +
                            context.Exception.Message);

                        return Task.CompletedTask;
                    },

                    OnChallenge = context =>
                    {
                        Console.WriteLine(
                            "JWT challenge: " +
                            (context.Error ?? "No error code"));

                        if (!string.IsNullOrWhiteSpace(
                                context.ErrorDescription))
                        {
                            Console.WriteLine(
                                "JWT challenge description: " +
                                context.ErrorDescription);
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        // Preserve the existing RaceDayDB connection.
        builder.Services.AddDbContext<RaceDayDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString(
                    "DefaultConnection")));

        // 2. Build App
        var app = builder.Build();

        // 3. Configure HTTP Request Pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (!string.Equals(Environment.GetEnvironmentVariable("DISABLE_HTTPS_REDIRECT"), "true", StringComparison.OrdinalIgnoreCase))
        {
            app.UseHttpsRedirection();
        }
        app.UseSession();

        // Authentication must run before authorization.
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Existing database diagnostic endpoint
        app.MapGet("/api/diagnostics/database",
            async (RaceDayDbContext db) =>
            {
                try
                {
                    bool connected = await db.Database.CanConnectAsync();

                    if (connected)
                    {
                        return Results.Ok(new
                        {
                            status = "Connected",
                            database = "RaceDayDB"
                        });
                    }

                    return Results.Problem(
                        "Could not connect to RaceDayDB.");
                }
                catch
                {
                    return Results.Problem(
                        "Database connection failed. Check the connection string and SQL Server.");
                }
            });

        // 4. Run App
        app.Run();
    }
}