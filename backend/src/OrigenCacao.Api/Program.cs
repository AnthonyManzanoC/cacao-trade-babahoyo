using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrigenCacao.Api;
using OrigenCacao.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Origen Cacao API", Version = "v1", Description = "ERP/CRM para compra y venta justa de cacao." });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = [] });
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "LOCAL-ONLY-change-this-key-before-production-2026!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "OrigenCacao",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "OrigenCacao.Admin",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});
builder.Services.AddAuthorization();

// AQUÍ ESTÁ EL CAMBIO: Se añadió la URL de Vercel a los orígenes permitidos
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy => policy
    .WithOrigins("https://cacao-trade-babahoyo.vercel.app", "http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapControllers();
app.MapHealthChecks("/health");

if (builder.Configuration.GetValue("Database:AutoMigrate", true))
    await app.Services.InitializeDatabaseAsync(builder.Configuration);

app.Run();

public partial class Program;