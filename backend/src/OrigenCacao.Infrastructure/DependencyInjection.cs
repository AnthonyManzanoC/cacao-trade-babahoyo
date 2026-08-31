using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrigenCacao.Application;
using OrigenCacao.Domain;

namespace OrigenCacao.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DATABASE_CONNECTION"]
            ?? throw new InvalidOperationException("Configura ConnectionStrings:DefaultConnection o DATABASE_CONNECTION.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                .EnableRetryOnFailure(4, TimeSpan.FromSeconds(5), null)));

        services.AddScoped<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProducerService, ProducerService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IPublicContentService, PublicContentService>();
        services.AddScoped<ICashRegisterService, CashRegisterService>();
        services.AddScoped<IProcessingService, ProcessingService>();
        services.AddScoped<IPurchaseReceiptService, PurchaseReceiptService>();
        services.AddScoped<ISaleReceiptService, SaleReceiptService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ICocoaPriceUpdater, CocoaPriceUpdater>();
        services.Configure<ApiNinjasOptions>(configuration.GetSection("ApiNinjas"));
        services.AddHttpClient("ApiNinjas", client =>
        {
            client.BaseAddress = new Uri("https://api.api-ninjas.com");
            client.Timeout = TimeSpan.FromSeconds(20);
        });
        services.AddHttpClient("YahooFinance", client =>
        {
            client.BaseAddress = new Uri("https://query1.finance.yahoo.com");
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 OrigenCacao/1.0");
        });
        services.AddHostedService<CocoaPriceWorker>();
        return services;
    }
}
