using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrigenCacao.Domain;

namespace OrigenCacao.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, IConfiguration configuration, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(ct);
        if (!await db.BusinessSettings.AnyAsync(ct))
        {
            var settings = new BusinessSettings();
            db.BusinessSettings.Add(settings);
            db.PriceHistory.Add(new PriceHistory { MarketPricePerMetricTon = 0, DryPricePerQuintal = settings.CurrentDryPricePerQuintal,
                WetPricePerQuintal = settings.CurrentWetPricePerQuintal, MarginPerQuintal = settings.MarginPerQuintal,
                Source = settings.PriceSource, QuotedAtUtc = settings.CurrentPriceUpdatedAtUtc });
        }
        if (!await db.AdminUsers.AnyAsync(ct))
        {
            var user = new AdminUser { FullName = configuration["Admin:Name"] ?? "Administrador",
                Email = (configuration["Admin:Email"] ?? "admin@cacao.local").Trim().ToLowerInvariant() };
            var password = configuration["Admin:Password"] ?? "CacaoLocal2026!";
            user.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AdminUser>>().HashPassword(user, password);
            db.AdminUsers.Add(user);
        }
        if (!await db.PublicContents.AnyAsync(ct))
        {
            db.PublicContents.AddRange(
                new PublicContent { ContentKey = "hero-principal", Section = PublicContentSection.Hero,
                    Eyebrow = "Centro de acopio · Ecuador", Title = "Tu cacao vale. Aquí lo demostramos.",
                    Subtitle = "Precio transparente, peso exacto y pago responsable para cada productor.",
                    Body = "Consulta el precio vigente, calcula tu venta y coordina la entrega o recolección de tu cacao.",
                    PrimaryCtaLabel = "Calcular mi venta", PrimaryCtaUrl = "/precios",
                    SecondaryCtaLabel = "Hablar por WhatsApp", SecondaryCtaUrl = "#whatsapp", DisplayOrder = 0 },
                new PublicContent { ContentKey = "nosotros-historia", Section = PublicContentSection.Nosotros,
                    Eyebrow = "Nuestra razón de ser", Title = "Comercio justo que fortalece el campo",
                    Subtitle = "Construimos relaciones duraderas con pequeños productores.",
                    Body = "Compramos cacao en baba y seco con trazabilidad por lote, criterios claros y atención cercana. Nuestro compromiso es crecer junto a las familias cacaoteras.", DisplayOrder = 0 },
                new PublicContent { ContentKey = "servicio-compra", Section = PublicContentSection.Servicio,
                    Eyebrow = "Compra directa", Title = "Recepción de cacao en baba y seco",
                    Subtitle = "Pesaje transparente y comprobante detallado.",
                    Body = "Evaluamos humedad y merma, aplicamos el precio vigente y entregamos un comprobante por cada compra.", Icon = "scale", DisplayOrder = 0 },
                new PublicContent { ContentKey = "servicio-secado", Section = PublicContentSection.Servicio,
                    Eyebrow = "Valor agregado", Title = "Secado y manejo por lotes",
                    Subtitle = "Control de rendimiento desde cacao en baba hasta cacao seco.",
                    Body = "Registramos entradas, pérdidas de humedad y rendimiento final para conservar la trazabilidad y el costo real.", Icon = "sun", DisplayOrder = 1 },
                new PublicContent { ContentKey = "servicio-recoleccion", Section = PublicContentSection.Servicio,
                    Eyebrow = "Logística", Title = "Coordinación de recolección",
                    Subtitle = "Consulta disponibilidad de retiro en finca.",
                    Body = "Ayudamos a coordinar el transporte cuando la zona y el volumen lo permiten.", Icon = "truck", DisplayOrder = 2 },
                new PublicContent { ContentKey = "contacto-principal", Section = PublicContentSection.Contacto,
                    Eyebrow = "Conversemos", Title = "Trae tu cacao o coordina una visita",
                    Subtitle = "Atención directa para productores, exportadoras y chocolaterías.",
                    Body = "Escríbenos para conocer el precio del día, horarios de recepción y opciones de transporte.",
                    PrimaryCtaLabel = "Abrir WhatsApp", PrimaryCtaUrl = "#whatsapp", DisplayOrder = 0 });
        }
        await db.SaveChangesAsync(ct);
    }
}
