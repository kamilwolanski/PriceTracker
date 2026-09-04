using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PriceTracker.Data;
using PriceTracker.Features.Auth;
using PriceTracker.Features.PriceHistory;
using PriceTracker.Features.PriceChecking;
using PriceTracker.Features.TrackedProductCreation;
using PriceTracker.Features.TrackedProducts;
using Scalar.AspNetCore;
using System.Text;
using PriceTracker.Features.PriceChecking.HtmlAgilityScraper;
using PriceTracker.Features.PriceChecking.PlaywrightScraper;
using PriceTracker.Features.PriceChecking.WebsiteScrapers;
using PriceTracker.Features.PriceMonitoring;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Wklej access token JWT."
        };

        return Task.CompletedTask;
    });
});
builder.Services.AddScoped<ITrackedProductService, TrackedProductService>();
builder.Services.AddScoped<IPriceHistoryService, PriceHistoryService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IPriceScraper, PriceScraper>();
builder.Services.AddScoped<IPriceScrapingStrategy, XkomPriceScraper>();
builder.Services.AddScoped<IPriceScrapingStrategy, OlxPriceScraper>();
builder.Services.AddScoped<IPriceScrapingStrategy, HtmlAgilityScraperService>();
builder.Services.AddScoped<IPriceScrapingStrategy, PlaywrightScraperService>();
builder.Services.AddScoped<PriceCheckingService>();
builder.Services.AddScoped<TrackedProductCreationService>();
builder.Services.AddHostedService<PriceMonitoringWorker>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)
            ),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }




