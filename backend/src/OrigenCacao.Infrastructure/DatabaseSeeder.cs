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
        var settings = await db.BusinessSettings.SingleOrDefaultAsync(x => x.Id == 1, ct);
        if (settings is null)
        {
            settings = new BusinessSettings();
            db.BusinessSettings.Add(settings);
            db.PriceHistory.Add(new PriceHistory { MarketPricePerMetricTon = 0, DryPricePerQuintal = settings.CurrentDryPricePerQuintal,
                WetPricePerQuintal = settings.CurrentWetPricePerQuintal, MarginPerQuintal = settings.MarginPerQuintal,
                Source = settings.PriceSource, QuotedAtUtc = settings.CurrentPriceUpdatedAtUtc });
        }
        else
        {
            if (settings.BusinessName == "Origen Cacao") settings.BusinessName = "Grupo Álvarez";
            if (string.IsNullOrWhiteSpace(settings.LogoUrl)) settings.LogoUrl = "/grupo-alvarez-cacao-logo.png";
            if (string.IsNullOrWhiteSpace(settings.PriceClockLabel)) settings.PriceClockLabel = "Hora Ecuador";
            if (string.IsNullOrWhiteSpace(settings.TimeZone)) settings.TimeZone = "America/Guayaquil";
        }
        if (!await db.AdminUsers.AnyAsync(ct))
        {
            var user = new AdminUser { FullName = configuration["Admin:Name"] ?? "Administrador",
                Email = (configuration["Admin:Email"] ?? "admin@cacao.local").Trim().ToLowerInvariant() };
            var password = configuration["Admin:Password"] ?? "CacaoLocal2026!";
            user.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AdminUser>>().HashPassword(user, password);
            db.AdminUsers.Add(user);
        }
        var defaultContent = new[]
        {
                new PublicContent { ContentKey = "hero-principal", Section = PublicContentSection.Hero,
                    Eyebrow = "El valor nace en el origen", Title = "Tu cacao vale más cuando el trato es claro.",
                    Subtitle = "Pesamos frente a ti, explicamos cada descuento y pagamos con el precio publicado.",
                    Body = "Sin letras pequeñas.", PrimaryCtaLabel = "Calcular mi venta", PrimaryCtaUrl = "#precio",
                    SecondaryCtaLabel = "Hablar por WhatsApp", SecondaryCtaUrl = "#whatsapp",
                    ImageUrl = "/cacao-hero.png", DisplayOrder = 0 },
                new PublicContent { ContentKey = "hero-pesaje", Section = PublicContentSection.Hero,
                    Eyebrow = "Peso transparente", Title = "Cada libra se pesa contigo, cada valor se explica.",
                    Subtitle = "Recibimos cacao en baba y seco con criterios visibles, comprobante y atención directa.",
                    Body = "Tú ves el proceso completo.", PrimaryCtaLabel = "Ver precio de hoy", PrimaryCtaUrl = "#precio",
                    SecondaryCtaLabel = "Cómo trabajamos", SecondaryCtaUrl = "#proceso",
                    ImageUrl = "/cacao-pesaje-transparente.png", DisplayOrder = 1 },
                new PublicContent { ContentKey = "hero-alianza", Section = PublicContentSection.Hero,
                    Eyebrow = "Crecemos desde el campo", Title = "Una alianza que reconoce el trabajo detrás de cada cosecha.",
                    Subtitle = "Compra responsable, logística cercana y relaciones construidas lote a lote.",
                    Body = "Grupo Álvarez conecta origen, confianza y futuro.", PrimaryCtaLabel = "Conócenos", PrimaryCtaUrl = "#nosotros",
                    SecondaryCtaLabel = "Coordinar entrega", SecondaryCtaUrl = "#whatsapp",
                    ImageUrl = "/cacao-productores-alianza.png", DisplayOrder = 2 },
                new PublicContent { ContentKey = "nosotros-historia", Section = PublicContentSection.Nosotros,
                    Eyebrow = "Nuestra razón de ser", Title = "Del campo ecuatoriano a relaciones que perduran",
                    Subtitle = "Somos una empresa familiar que compra cacao con cercanía, respeto y visión de futuro.",
                    Body = "Trabajamos junto a productores de la costa ecuatoriana con pesaje transparente, trazabilidad por lote y pago responsable. Nuestro crecimiento empieza cuando la cosecha de cada familia recibe un trato justo.",
                    ImageUrl = "/cacao-productores-alianza.png", PrimaryCtaLabel = "Conoce nuestra forma de trabajar", PrimaryCtaUrl = "#proceso",
                    SecondaryCtaLabel = "Crecemos cuando el productor también crece.", DisplayOrder = 0 },
                new PublicContent { ContentKey = "nosotros-carrusel-alianza", Section = PublicContentSection.CarruselNosotros,
                    Eyebrow = "Comunidad", Title = "Productores aliados en el campo",
                    Subtitle = "Relaciones que crecen junto a cada cosecha.", ImageUrl = "/cacao-productores-alianza.png", DisplayOrder = 0 },
                new PublicContent { ContentKey = "nosotros-carrusel-pesaje", Section = PublicContentSection.CarruselNosotros,
                    Eyebrow = "Transparencia", Title = "Pesaje claro frente al productor",
                    Subtitle = "Cada valor se comprueba en el centro de acopio.", ImageUrl = "/cacao-pesaje-transparente.png", DisplayOrder = 1 },
                new PublicContent { ContentKey = "nosotros-carrusel-origen", Section = PublicContentSection.CarruselNosotros,
                    Eyebrow = "Origen", Title = "Cacao ecuatoriano seleccionado",
                    Subtitle = "El valor de cada lote comienza en el campo.", ImageUrl = "/cacao-hero.png", DisplayOrder = 2 },
                new PublicContent { ContentKey = "servicio-intro", Section = PublicContentSection.Servicio,
                    Eyebrow = "Más que comprar cacao", Title = "Un aliado para hacer crecer cada cosecha.",
                    Subtitle = "Acompañamos a pequeños productores con procesos claros, logística cercana y una relación que se construye lote a lote.",
                    Body = "", Icon = "intro", DisplayOrder = 0 },
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
                    PrimaryCtaLabel = "Abrir WhatsApp", PrimaryCtaUrl = "#whatsapp", DisplayOrder = 0 },
                new PublicContent { ContentKey = "beneficio-intro", Section = PublicContentSection.Beneficio,
                    Eyebrow = "Lo que puedes esperar", Title = "Claridad desde que llegas hasta que recibes tu pago",
                    Subtitle = "Una compra bien hecha se nota en cada paso.", Body = "", Icon = "intro", DisplayOrder = 0 },
                new PublicContent { ContentKey = "beneficio-precio", Section = PublicContentSection.Beneficio,
                    Eyebrow = "Precio visible", Title = "Sabes cuánto vale antes de vender", Subtitle = "El marcador y la calculadora muestran el valor vigente.",
                    Body = "Actualizamos la referencia y explicamos el cálculo aplicado a tu lote.", Icon = "price", DisplayOrder = 1 },
                new PublicContent { ContentKey = "beneficio-peso", Section = PublicContentSection.Beneficio,
                    Eyebrow = "Proceso abierto", Title = "Pesaje y medición frente a ti", Subtitle = "Sin pasos ocultos ni números difíciles de comprobar.",
                    Body = "Revisamos peso, tara, humedad y merma contigo.", Icon = "scale", DisplayOrder = 2 },
                new PublicContent { ContentKey = "beneficio-pago", Section = PublicContentSection.Beneficio,
                    Eyebrow = "Cierre responsable", Title = "Comprobante y pago acordado", Subtitle = "Cada recepción queda respaldada y trazable.",
                    Body = "Confirmamos el cálculo final y el método de pago antes de cerrar.", Icon = "shield", DisplayOrder = 3 },
                new PublicContent { ContentKey = "proceso-intro", Section = PublicContentSection.Proceso,
                    Eyebrow = "Así trabajamos", Title = "De tu mensaje al pago, sin complicaciones",
                    Subtitle = "Un proceso corto, humano y verificable.", Body = "", Icon = "intro", DisplayOrder = 0 },
                new PublicContent { ContentKey = "proceso-coordina", Section = PublicContentSection.Proceso,
                    Eyebrow = "01", Title = "Coordina tu entrega", Subtitle = "Cuéntanos tipo, estado y cantidad aproximada.", Body = "Confirmamos horario y opción de recolección cuando esté disponible.", Icon = "message", DisplayOrder = 1 },
                new PublicContent { ContentKey = "proceso-mide", Section = PublicContentSection.Proceso,
                    Eyebrow = "02", Title = "Medimos contigo", Subtitle = "Pesamos y comprobamos las condiciones del cacao.", Body = "Revisamos tara, humedad y merma de forma visible.", Icon = "scale", DisplayOrder = 2 },
                new PublicContent { ContentKey = "proceso-recibe", Section = PublicContentSection.Proceso,
                    Eyebrow = "03", Title = "Recibe tu pago", Subtitle = "Te mostramos el cálculo y emitimos el comprobante.", Body = "El pago se realiza según el método acordado.", Icon = "check", DisplayOrder = 3 },
                new PublicContent { ContentKey = "impacto-intro", Section = PublicContentSection.Impacto,
                    Eyebrow = "Impacto con propósito", Title = "Crecer con el campo, cuidar cada relación",
                    Subtitle = "Metas editables que reflejan el avance real de nuestra operación.", Body = "", Icon = "intro", DisplayOrder = 0 },
                new PublicContent { ContentKey = "impacto-productores", Section = PublicContentSection.Impacto,
                    Eyebrow = "+120", Title = "Productores aliados", Subtitle = "Relaciones directas y cercanas.", Body = "", Icon = "sprout", DisplayOrder = 1 },
                new PublicContent { ContentKey = "impacto-trazabilidad", Section = PublicContentSection.Impacto,
                    Eyebrow = "100%", Title = "Compras trazables", Subtitle = "Cada lote conserva su historia.", Body = "", Icon = "shield", DisplayOrder = 2 },
                new PublicContent { ContentKey = "impacto-pago", Section = PublicContentSection.Impacto,
                    Eyebrow = "Mismo día", Title = "Pago responsable", Subtitle = "Cierre claro y comprobante.", Body = "", Icon = "check", DisplayOrder = 3 },
                new PublicContent { ContentKey = "testimonio-ana", Section = PublicContentSection.Testimonio,
                    Eyebrow = "Productora aliada", Title = "Ana M. · Los Ríos", Subtitle = "Ahora conozco el cálculo antes de entregar.",
                    Body = "Me explicaron el peso y la humedad con calma. Salí con mi comprobante y el pago acordado.", DisplayOrder = 0 },
                new PublicContent { ContentKey = "testimonio-intro", Section = PublicContentSection.Testimonio,
                    Eyebrow = "Voces del campo", Title = "La confianza se cultiva en cada compra.",
                    Subtitle = "Historias que muestran cómo se vive nuestro proceso.", Body = "", Icon = "intro", DisplayOrder = 0 },
                new PublicContent { ContentKey = "testimonio-carlos", Section = PublicContentSection.Testimonio,
                    Eyebrow = "Productor aliado", Title = "Carlos V. · Babahoyo", Subtitle = "La transparencia hace que uno vuelva.",
                    Body = "El precio estaba publicado y todo se revisó frente a mí. Así se construye confianza.", DisplayOrder = 1 },
                new PublicContent { ContentKey = "testimonio-luisa", Section = PublicContentSection.Testimonio,
                    Eyebrow = "Familia productora", Title = "Luisa P. · Vinces", Subtitle = "Nos atendieron como socios del proceso.",
                    Body = "Coordinamos la entrega por WhatsApp y al llegar ya sabían qué lote recibiríamos.", DisplayOrder = 2 },
                new PublicContent { ContentKey = "galeria-origen", Section = PublicContentSection.Galeria,
                    Eyebrow = "Origen", Title = "Cacao que nace en nuestra tierra", Subtitle = "Selección de mazorcas frescas.", Body = "",
                    ImageUrl = "/cacao-hero.png", DisplayOrder = 0 },
                new PublicContent { ContentKey = "galeria-intro", Section = PublicContentSection.Galeria,
                    Eyebrow = "Nuestro cacao, nuestra gente", Title = "Historias del origen",
                    Subtitle = "Una mirada al trabajo y las relaciones detrás de cada lote.", Body = "", Icon = "intro", DisplayOrder = 0 },
                new PublicContent { ContentKey = "galeria-pesaje", Section = PublicContentSection.Galeria,
                    Eyebrow = "Transparencia", Title = "Peso que se comprueba", Subtitle = "La medición se realiza contigo.", Body = "",
                    ImageUrl = "/cacao-pesaje-transparente.png", DisplayOrder = 1 },
                new PublicContent { ContentKey = "galeria-alianza", Section = PublicContentSection.Galeria,
                    Eyebrow = "Comunidad", Title = "Cosechas que crean futuro", Subtitle = "Relaciones construidas en el campo.", Body = "",
                    ImageUrl = "/cacao-productores-alianza.png", DisplayOrder = 2 },
                new PublicContent { ContentKey = "footer-principal", Section = PublicContentSection.Footer,
                    Eyebrow = "Grupo Álvarez", Title = "Compra justa de cacao en Ecuador",
                    Subtitle = "Precio claro, peso exacto y relaciones que perduran.",
                    Body = "Trabajamos junto a productores para convertir cada cosecha en una oportunidad de crecimiento compartido.", DisplayOrder = 0 }
        };
        var existingKeys = (await db.PublicContents.Select(x => x.ContentKey).ToListAsync(ct)).ToHashSet();
        db.PublicContents.AddRange(defaultContent.Where(x => !existingKeys.Contains(x.ContentKey)));
        await db.SaveChangesAsync(ct);
    }
}
