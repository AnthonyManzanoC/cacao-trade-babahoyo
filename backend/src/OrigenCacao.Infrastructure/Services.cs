using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrigenCacao.Application;
using OrigenCacao.Domain;

namespace OrigenCacao.Infrastructure;

internal static class Maps
{
    public static ProducerDto Producer(Producer x, int count = 0, decimal quintals = 0, decimal paid = 0) =>
        new(x.Id, x.FullName, x.DocumentNumber, x.FarmLocation, x.Phone, x.Email, x.PreferredPaymentMethod,
            x.Notes, x.IsActive, count, quintals, paid, x.CreatedAtUtc);

    public static PurchaseDto Purchase(Purchase x, string? producerName = null) => new(x.Id, x.Code, x.ProducerId,
        producerName ?? x.Producer?.FullName ?? string.Empty, x.Producer?.Email, x.Variety, x.State, x.GrossWeightLbs, x.TareLbs,
        x.HumidityPercent, x.ShrinkagePercent, x.NetWeightLbs, x.PayableQuintals, x.UnitPrice, x.TotalPaid,
        x.PaymentMethod, x.PurchasedAtUtc, x.Notes, x.IsVoided);

    public static SaleDto Sale(Sale x) => new(x.Id, x.Code, x.CustomerName, x.CustomerTaxId, x.CustomerEmail, x.Variety, x.State,
        x.QuantityQuintals, x.UnitPrice, x.CostBasisPerQuintal, x.Total, x.GrossProfit, x.PaymentMethod,
        x.SoldAtUtc, x.Notes, x.IsVoided);

    public static SettingsDto Settings(BusinessSettings x) => new(x.BusinessName, x.MarginPerQuintal,
        x.WetPriceFactor, x.UseManualPrice, x.ManualDryPricePerQuintal, x.CurrentMarketPricePerMetricTon,
        x.CurrentDryPricePerQuintal, x.CurrentWetPricePerQuintal, x.CurrentPriceUpdatedAtUtc,
        x.ApiLastSuccessAtUtc, x.ApiLastError, x.PriceSource, x.ContactWhatsApp, x.ContactAddress,
        x.ContactPhone, x.ContactEmail, x.GoogleMapsEmbedUrl, x.Location, x.PickupEnabled,
        x.EmailSendingEnabled, x.SmtpHost, x.SmtpPort, x.SmtpEmail,
        !string.IsNullOrWhiteSpace(x.SmtpPassword), x.SmtpUseSsl);
}

public sealed class AuthService(AppDbContext db, IPasswordHasher<AdminUser> hasher, IConfiguration configuration) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.AdminUsers.SingleOrDefaultAsync(x => x.Email == email && x.IsActive, ct);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return null;

        user.LastLoginAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        var expires = DateTime.UtcNow.AddHours(10);
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurado.");
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"] ?? "OrigenCacao",
            configuration["Jwt:Audience"] ?? "OrigenCacao.Admin",
            [new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(JwtRegisteredClaimNames.Email, user.Email), new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.Role, "Admin")],
            expires: expires, signingCredentials: credentials);
        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expires, user.FullName, user.Email);
    }
}

public sealed class ProducerService(AppDbContext db) : IProducerService
{
    public async Task<IReadOnlyList<ProducerDto>> ListAsync(string? search, CancellationToken ct)
    {
        var query = db.Producers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x => x.FullName.ToLower().Contains(term) || x.DocumentNumber.Contains(term) || x.FarmLocation.ToLower().Contains(term));
        }
        return await query.OrderBy(x => x.FullName).Select(x => new ProducerDto(x.Id, x.FullName, x.DocumentNumber,
            x.FarmLocation, x.Phone, x.Email, x.PreferredPaymentMethod, x.Notes, x.IsActive,
            x.Purchases.Count(p => !p.IsVoided), x.Purchases.Where(p => !p.IsVoided).Sum(p => (decimal?)p.PayableQuintals) ?? 0,
            x.Purchases.Where(p => !p.IsVoided).Sum(p => (decimal?)p.TotalPaid) ?? 0, x.CreatedAtUtc)).ToListAsync(ct);
    }

    public async Task<ProducerDto?> GetAsync(Guid id, CancellationToken ct) =>
        await db.Producers.AsNoTracking().Where(x => x.Id == id).Select(x => new ProducerDto(x.Id, x.FullName,
            x.DocumentNumber, x.FarmLocation, x.Phone, x.Email, x.PreferredPaymentMethod, x.Notes, x.IsActive,
            x.Purchases.Count(p => !p.IsVoided), x.Purchases.Where(p => !p.IsVoided).Sum(p => (decimal?)p.PayableQuintals) ?? 0,
            x.Purchases.Where(p => !p.IsVoided).Sum(p => (decimal?)p.TotalPaid) ?? 0, x.CreatedAtUtc)).SingleOrDefaultAsync(ct);

    public async Task<ProducerDto> CreateAsync(UpsertProducerRequest request, CancellationToken ct)
    {
        Validate(request);
        if (await db.Producers.AnyAsync(x => x.DocumentNumber == request.DocumentNumber.Trim(), ct)) throw new InvalidOperationException("Ya existe un productor con esa cédula/RUC.");
        var entity = new Producer(); Apply(entity, request); db.Producers.Add(entity); await db.SaveChangesAsync(ct); return Maps.Producer(entity);
    }

    public async Task<ProducerDto?> UpdateAsync(Guid id, UpsertProducerRequest request, CancellationToken ct)
    {
        Validate(request); var entity = await db.Producers.FindAsync([id], ct); if (entity is null) return null;
        if (await db.Producers.AnyAsync(x => x.Id != id && x.DocumentNumber == request.DocumentNumber.Trim(), ct)) throw new InvalidOperationException("Ya existe un productor con esa cédula/RUC.");
        Apply(entity, request); entity.UpdatedAtUtc = DateTime.UtcNow; await db.SaveChangesAsync(ct); return await GetAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.Producers.FindAsync([id], ct); if (entity is null) return false;
        if (await db.Purchases.AnyAsync(x => x.ProducerId == id, ct)) { entity.IsActive = false; entity.UpdatedAtUtc = DateTime.UtcNow; }
        else db.Producers.Remove(entity);
        await db.SaveChangesAsync(ct); return true;
    }

    private static void Validate(UpsertProducerRequest x)
    {
        if (string.IsNullOrWhiteSpace(x.FullName) || string.IsNullOrWhiteSpace(x.DocumentNumber)) throw new ArgumentException("Nombre y cédula/RUC son obligatorios.");
        if (!string.IsNullOrWhiteSpace(x.Email) && !System.Net.Mail.MailAddress.TryCreate(x.Email, out _))
            throw new ArgumentException("El correo del productor no es válido.");
    }
    private static void Apply(Producer e, UpsertProducerRequest x)
    {
        e.FullName = x.FullName.Trim(); e.DocumentNumber = x.DocumentNumber.Trim(); e.FarmLocation = x.FarmLocation.Trim();
        e.Phone = x.Phone.Trim(); e.Email = string.IsNullOrWhiteSpace(x.Email) ? null : x.Email.Trim().ToLowerInvariant();
        e.PreferredPaymentMethod = x.PreferredPaymentMethod; e.Notes = x.Notes?.Trim(); e.IsActive = x.IsActive;
    }
}

public sealed class PurchaseService(AppDbContext db) : IPurchaseService
{
    public async Task<IReadOnlyList<PurchaseDto>> ListAsync(int take, CancellationToken ct) => await db.Purchases.AsNoTracking()
        .Include(x => x.Producer).OrderByDescending(x => x.PurchasedAtUtc).Take(Math.Clamp(take, 1, 500))
        .Select(x => Maps.Purchase(x, x.Producer.FullName)).ToListAsync(ct);

    public async Task<PurchaseDto?> GetAsync(Guid id, CancellationToken ct) => await db.Purchases.AsNoTracking()
        .Include(x => x.Producer).Where(x => x.Id == id).Select(x => Maps.Purchase(x, x.Producer.FullName)).SingleOrDefaultAsync(ct);

    public async Task<PurchaseDto> CreateAsync(CreatePurchaseRequest request, CancellationToken ct)
    {
        var producer = await db.Producers.FindAsync([request.ProducerId], ct) ?? throw new KeyNotFoundException("Productor no encontrado.");
        if (!producer.IsActive) throw new InvalidOperationException("El productor está inactivo.");
        var settings = await db.BusinessSettings.SingleAsync(x => x.Id == 1, ct);
        var unitPrice = request.UnitPrice ?? (request.State == CocoaState.Seco ? settings.CurrentDryPricePerQuintal : settings.CurrentWetPricePerQuintal);
        var calc = PricingCalculator.CalculatePurchase(request.GrossWeightLbs, request.TareLbs, request.HumidityPercent, request.ShrinkagePercent, unitPrice);
        var now = DateTime.UtcNow;
        var entity = new Purchase
        {
            Code = await NextCode("COM", db.Purchases.Select(x => x.Code), ct), ProducerId = producer.Id, Producer = producer,
            Variety = request.Variety, State = request.State, GrossWeightLbs = request.GrossWeightLbs,
            TareLbs = request.TareLbs, HumidityPercent = request.HumidityPercent, ShrinkagePercent = request.ShrinkagePercent,
            NetWeightLbs = calc.PhysicalNetWeightLbs, PayableQuintals = calc.PayableQuintals, UnitPrice = unitPrice,
            TotalPaid = calc.Total, PaymentMethod = request.PaymentMethod, PurchasedAtUtc = ToUtc(request.PurchasedAtUtc ?? now), Notes = request.Notes?.Trim()
        };
        db.Purchases.Add(entity);
        db.InventoryLots.Add(new InventoryLot
        {
            Code = $"LOT-{entity.Code}", PurchaseId = entity.Id, Purchase = entity,
            Variety = entity.Variety, State = entity.State,
            InitialQuantityQuintals = entity.PayableQuintals, AvailableQuantityQuintals = entity.PayableQuintals,
            UnitCost = entity.UnitPrice, HumidityPercent = entity.HumidityPercent,
            Status = InventoryLotStatus.Disponible, ReceivedAtUtc = entity.PurchasedAtUtc,
            Notes = $"Lote creado por la compra {entity.Code}"
        });
        db.InventoryMovements.Add(new InventoryMovement { Type = MovementType.Compra, Variety = entity.Variety, State = entity.State,
            QuantityQuintals = entity.PayableQuintals, UnitAmount = entity.UnitPrice, ReferenceId = entity.Id,
            ReferenceCode = entity.Code, OccurredAtUtc = entity.PurchasedAtUtc, Notes = "Entrada automática por compra" });
        if (entity.PaymentMethod == PaymentMethod.Efectivo)
            await CashOperations.AddAutomaticAsync(db, CashMovementDirection.Egreso, CashMovementCategory.CompraCacao,
                entity.TotalPaid, $"Compra de cacao a {producer.FullName}", entity.Id, entity.Code,
                entity.PaymentMethod, entity.PurchasedAtUtc, requireSufficientCash: true, ct);
        await db.SaveChangesAsync(ct); return Maps.Purchase(entity, producer.FullName);
    }

    public async Task<bool> VoidAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.Purchases.Include(x => x.Producer).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false; if (entity.IsVoided) return true;
        var lot = await db.InventoryLots.SingleOrDefaultAsync(x => x.PurchaseId == entity.Id, ct);
        if (lot is null || lot.AvailableQuantityQuintals != lot.InitialQuantityQuintals)
            throw new InvalidOperationException("No se puede anular: una parte de este lote ya fue vendida o enviada a secado.");
        entity.IsVoided = true; entity.UpdatedAtUtc = DateTime.UtcNow;
        lot.AvailableQuantityQuintals = 0; lot.Status = InventoryLotStatus.Anulado; lot.UpdatedAtUtc = DateTime.UtcNow;
        db.InventoryMovements.Add(new InventoryMovement { Type = MovementType.Reversion, Variety = entity.Variety, State = entity.State,
            QuantityQuintals = -entity.PayableQuintals, UnitAmount = entity.UnitPrice, ReferenceId = entity.Id,
            ReferenceCode = entity.Code, OccurredAtUtc = DateTime.UtcNow, Notes = "Reversión por anulación de compra" });
        if (entity.PaymentMethod == PaymentMethod.Efectivo)
            await CashOperations.AddAutomaticAsync(db, CashMovementDirection.Ingreso, CashMovementCategory.Ajuste,
                entity.TotalPaid, $"Reversión de compra {entity.Code}", entity.Id, entity.Code,
                entity.PaymentMethod, DateTime.UtcNow, false, ct);
        await db.SaveChangesAsync(ct); return true;
    }

    internal static async Task<string> NextCode(string prefix, IQueryable<string> codes, CancellationToken ct)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var count = await codes.CountAsync(x => x.StartsWith($"{prefix}-{date}"), ct) + 1;
        return $"{prefix}-{date}-{count:0000}";
    }
    internal static DateTime ToUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}

public sealed class SaleService(AppDbContext db) : ISaleService
{
    public async Task<IReadOnlyList<SaleDto>> ListAsync(int take, CancellationToken ct) => await db.Sales.AsNoTracking()
        .OrderByDescending(x => x.SoldAtUtc).Take(Math.Clamp(take, 1, 500)).Select(x => Maps.Sale(x)).ToListAsync(ct);

    public async Task<SaleDto?> GetAsync(Guid id, CancellationToken ct) =>
        await db.Sales.AsNoTracking().Where(x => x.Id == id).Select(x => Maps.Sale(x)).SingleOrDefaultAsync(ct);

    public async Task<SaleDto> CreateAsync(CreateSaleRequest request, CancellationToken ct)
    {
        if (request.QuantityQuintals <= 0 || request.UnitPrice <= 0 || string.IsNullOrWhiteSpace(request.CustomerName)) throw new ArgumentException("Cliente, cantidad y precio son obligatorios.");
        if (!string.IsNullOrWhiteSpace(request.CustomerEmail) && !System.Net.Mail.MailAddress.TryCreate(request.CustomerEmail, out _))
            throw new ArgumentException("El correo del cliente no es válido.");
        var lots = await db.InventoryLots
            .Where(x => x.Variety == request.Variety && x.State == request.State &&
                        x.Status == InventoryLotStatus.Disponible && x.AvailableQuantityQuintals > 0)
            .OrderBy(x => x.ReceivedAtUtc).ThenBy(x => x.Code).ToListAsync(ct);
        var available = lots.Sum(x => x.AvailableQuantityQuintals);
        if (available < request.QuantityQuintals) throw new InvalidOperationException($"Inventario insuficiente. Disponible: {available:N2} qq.");
        var remaining = request.QuantityQuintals;
        var allocationValues = new List<(InventoryLot Lot, decimal Quantity)>();
        foreach (var lot in lots)
        {
            if (remaining <= 0) break;
            var quantity = Math.Min(lot.AvailableQuantityQuintals, remaining);
            allocationValues.Add((lot, quantity));
            remaining -= quantity;
        }
        var costBasis = allocationValues.Sum(x => x.Quantity * x.Lot.UnitCost) / request.QuantityQuintals;
        var entity = new Sale { Code = await PurchaseService.NextCode("VEN", db.Sales.Select(x => x.Code), ct),
            CustomerName = request.CustomerName.Trim(), CustomerTaxId = request.CustomerTaxId?.Trim(),
            CustomerEmail = string.IsNullOrWhiteSpace(request.CustomerEmail) ? null : request.CustomerEmail.Trim().ToLowerInvariant(), Variety = request.Variety,
            State = request.State, QuantityQuintals = request.QuantityQuintals, UnitPrice = request.UnitPrice,
            CostBasisPerQuintal = decimal.Round(costBasis, 2), Total = decimal.Round(request.QuantityQuintals * request.UnitPrice, 2),
            GrossProfit = decimal.Round(request.QuantityQuintals * (request.UnitPrice - costBasis), 2),
            PaymentMethod = request.PaymentMethod,
            SoldAtUtc = PurchaseService.ToUtc(request.SoldAtUtc ?? DateTime.UtcNow), Notes = request.Notes?.Trim() };
        db.Sales.Add(entity);
        foreach (var (lot, quantity) in allocationValues)
        {
            lot.AvailableQuantityQuintals -= quantity;
            lot.Status = lot.AvailableQuantityQuintals == 0 ? InventoryLotStatus.Agotado : InventoryLotStatus.Disponible;
            lot.UpdatedAtUtc = DateTime.UtcNow;
            db.SaleLotAllocations.Add(new SaleLotAllocation { Sale = entity, SaleId = entity.Id,
                InventoryLot = lot, InventoryLotId = lot.Id, QuantityQuintals = quantity, UnitCost = lot.UnitCost });
        }
        db.InventoryMovements.Add(new InventoryMovement { Type = MovementType.Venta, Variety = entity.Variety,
            State = entity.State, QuantityQuintals = -entity.QuantityQuintals, UnitAmount = entity.UnitPrice,
            ReferenceId = entity.Id, ReferenceCode = entity.Code, OccurredAtUtc = entity.SoldAtUtc, Notes = "Salida automática por venta" });
        if (entity.PaymentMethod == PaymentMethod.Efectivo)
            await CashOperations.AddAutomaticAsync(db, CashMovementDirection.Ingreso, CashMovementCategory.VentaCacao,
                entity.Total, $"Venta de cacao a {entity.CustomerName}", entity.Id, entity.Code,
                entity.PaymentMethod, entity.SoldAtUtc, false, ct);
        await db.SaveChangesAsync(ct); return Maps.Sale(entity);
    }

    public async Task<bool> VoidAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.Sales.Include(x => x.LotAllocations).ThenInclude(x => x.InventoryLot)
            .SingleOrDefaultAsync(x => x.Id == id, ct); if (entity is null) return false; if (entity.IsVoided) return true;
        entity.IsVoided = true; entity.UpdatedAtUtc = DateTime.UtcNow;
        foreach (var allocation in entity.LotAllocations)
        {
            allocation.InventoryLot.AvailableQuantityQuintals += allocation.QuantityQuintals;
            allocation.InventoryLot.Status = InventoryLotStatus.Disponible;
            allocation.InventoryLot.UpdatedAtUtc = DateTime.UtcNow;
        }
        db.InventoryMovements.Add(new InventoryMovement { Type = MovementType.Reversion, Variety = entity.Variety,
            State = entity.State, QuantityQuintals = entity.QuantityQuintals, UnitAmount = entity.UnitPrice,
            ReferenceId = entity.Id, ReferenceCode = entity.Code, OccurredAtUtc = DateTime.UtcNow, Notes = "Reversión por anulación de venta" });
        if (entity.PaymentMethod == PaymentMethod.Efectivo)
            await CashOperations.AddAutomaticAsync(db, CashMovementDirection.Egreso, CashMovementCategory.Ajuste,
                entity.Total, $"Reversión de venta {entity.Code}", entity.Id, entity.Code,
                entity.PaymentMethod, DateTime.UtcNow, true, ct);
        await db.SaveChangesAsync(ct); return true;
    }
}

public sealed class InventoryService(AppDbContext db) : IInventoryService
{
    public async Task<IReadOnlyList<InventoryItemDto>> GetAsync(CancellationToken ct)
    {
        var lots = await db.InventoryLots.AsNoTracking().Where(x => x.AvailableQuantityQuintals > 0 && x.Status == InventoryLotStatus.Disponible).ToListAsync(ct);
        return lots.GroupBy(x => new { x.Variety, x.State }).Select(g =>
        {
            var qty = g.Sum(x => x.AvailableQuantityQuintals);
            var avg = qty > 0 ? g.Sum(x => x.AvailableQuantityQuintals * x.UnitCost) / qty : 0;
            return new InventoryItemDto(g.Key.Variety, g.Key.State, decimal.Round(qty, 4), decimal.Round(avg, 2), decimal.Round(qty * avg, 2));
        }).OrderBy(x => x.State).ThenBy(x => x.Variety).ToList();
    }

    public async Task<IReadOnlyList<InventoryLotDto>> GetLotsAsync(CocoaState? state, CancellationToken ct)
    {
        var query = db.InventoryLots.AsNoTracking().AsQueryable();
        if (state.HasValue) query = query.Where(x => x.State == state.Value);
        return await query.OrderByDescending(x => x.ReceivedAtUtc).Select(x => new InventoryLotDto(x.Id, x.Code,
            x.PurchaseId, x.ProcessingBatchId, x.Variety, x.State, x.InitialQuantityQuintals,
            x.AvailableQuantityQuintals, x.UnitCost, x.HumidityPercent, x.Status, x.ReceivedAtUtc, x.Notes)).ToListAsync(ct);
    }
}

public sealed class SettingsService(AppDbContext db, IOptions<ApiNinjasOptions> apiOptions) : ISettingsService
{
    public async Task<PublicPriceDto> GetPublicPriceAsync(CancellationToken ct)
    {
        var x = await db.BusinessSettings.AsNoTracking().SingleAsync(s => s.Id == 1, ct);
        return new PublicPriceDto(x.BusinessName, x.CurrentDryPricePerQuintal, x.CurrentWetPricePerQuintal,
            x.CurrentMarketPricePerMetricTon, x.CurrentPriceUpdatedAtUtc, x.PriceSource, x.UseManualPrice,
            x.ContactWhatsApp, x.ContactAddress, x.ContactPhone, x.ContactEmail, x.GoogleMapsEmbedUrl,
            x.Location, x.PickupEnabled,
            (x.ApiLastSuccessAtUtc ?? x.CurrentPriceUpdatedAtUtc).AddMinutes(Math.Clamp(apiOptions.Value.RefreshIntervalMinutes, 5, 1440)));
    }
    public async Task<IReadOnlyList<PricePointDto>> GetHistoryAsync(int days, CancellationToken ct) => await db.PriceHistory.AsNoTracking()
        .Where(x => x.QuotedAtUtc >= DateTime.UtcNow.AddDays(-Math.Clamp(days, 1, 90))).OrderBy(x => x.QuotedAtUtc)
        .Select(x => new PricePointDto(x.QuotedAtUtc, x.DryPricePerQuintal, x.WetPricePerQuintal, x.MarketPricePerMetricTon, x.Source)).ToListAsync(ct);
    public async Task<SettingsDto> GetAsync(CancellationToken ct) => Maps.Settings(await db.BusinessSettings.AsNoTracking().SingleAsync(x => x.Id == 1, ct));
    public async Task<SettingsDto> UpdateAsync(UpdateSettingsRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.BusinessName)) throw new ArgumentException("El nombre del negocio es obligatorio.");
        if (request.MarginPerQuintal < 0 || request.WetPriceFactor is <= 0 or > 1) throw new ArgumentException("Margen o factor de cacao en baba inválido.");
        if (request.UseManualPrice && (!request.ManualDryPricePerQuintal.HasValue || request.ManualDryPricePerQuintal <= 0)) throw new ArgumentException("Ingresa un precio manual válido.");
        if (!string.IsNullOrWhiteSpace(request.ContactEmail) && !System.Net.Mail.MailAddress.TryCreate(request.ContactEmail, out _))
            throw new ArgumentException("El correo público no es válido.");
        if (request.EmailSendingEnabled)
        {
            if (string.IsNullOrWhiteSpace(request.SmtpHost) || request.SmtpPort is <= 0 or > 65535 ||
                !System.Net.Mail.MailAddress.TryCreate(request.SmtpEmail, out _))
                throw new ArgumentException("Completa correctamente host, puerto y correo SMTP.");
        }
        var x = await db.BusinessSettings.SingleAsync(s => s.Id == 1, ct);
        x.BusinessName = request.BusinessName.Trim(); x.MarginPerQuintal = request.MarginPerQuintal; x.WetPriceFactor = request.WetPriceFactor;
        x.UseManualPrice = request.UseManualPrice; x.ManualDryPricePerQuintal = request.ManualDryPricePerQuintal;
        x.ContactWhatsApp = request.ContactWhatsApp.Trim(); x.ContactAddress = request.ContactAddress.Trim();
        x.ContactPhone = request.ContactPhone.Trim(); x.ContactEmail = request.ContactEmail.Trim().ToLowerInvariant();
        x.GoogleMapsEmbedUrl = request.GoogleMapsEmbedUrl.Trim(); x.Location = request.Location.Trim(); x.PickupEnabled = request.PickupEnabled;
        x.EmailSendingEnabled = request.EmailSendingEnabled; x.SmtpHost = request.SmtpHost.Trim();
        x.SmtpPort = request.SmtpPort; x.SmtpEmail = request.SmtpEmail.Trim().ToLowerInvariant(); x.SmtpUseSsl = request.SmtpUseSsl;
        if (!string.IsNullOrWhiteSpace(request.SmtpPassword)) x.SmtpPassword = request.SmtpPassword;
        if (x.EmailSendingEnabled && string.IsNullOrWhiteSpace(x.SmtpPassword))
            throw new ArgumentException("Configura la contraseña SMTP antes de habilitar el envío.");
        if (x.UseManualPrice)
        {
            x.CurrentDryPricePerQuintal = decimal.Round(x.ManualDryPricePerQuintal!.Value, 2);
            x.CurrentWetPricePerQuintal = decimal.Round(x.CurrentDryPricePerQuintal * x.WetPriceFactor, 2);
            x.CurrentPriceUpdatedAtUtc = DateTime.UtcNow; x.PriceSource = "Precio manual"; x.ApiLastError = null;
            db.PriceHistory.Add(new PriceHistory { MarketPricePerMetricTon = x.CurrentMarketPricePerMetricTon,
                DryPricePerQuintal = x.CurrentDryPricePerQuintal, WetPricePerQuintal = x.CurrentWetPricePerQuintal,
                MarginPerQuintal = x.MarginPerQuintal, Source = x.PriceSource, QuotedAtUtc = x.CurrentPriceUpdatedAtUtc });
        }
        else if (x.CurrentMarketPricePerMetricTon > 0)
        {
            x.CurrentDryPricePerQuintal = PricingCalculator.CalculateDryPrice(x.CurrentMarketPricePerMetricTon, x.MarginPerQuintal);
            x.CurrentWetPricePerQuintal = decimal.Round(x.CurrentDryPricePerQuintal * x.WetPriceFactor, 2);
        }
        x.UpdatedAtUtc = DateTime.UtcNow; await db.SaveChangesAsync(ct); return Maps.Settings(x);
    }
}

public sealed class DashboardService(AppDbContext db, IInventoryService inventoryService) : IDashboardService
{
    public async Task<DashboardDto> GetAsync(CancellationToken ct)
    {
        var start = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var purchases = db.Purchases.AsNoTracking().Where(x => !x.IsVoided && x.PurchasedAtUtc >= start);
        var sales = db.Sales.AsNoTracking().Where(x => !x.IsVoided && x.SoldAtUtc >= start);
        var inventory = await inventoryService.GetAsync(ct);
        var recent = await db.Purchases.AsNoTracking().Include(x => x.Producer).Where(x => !x.IsVoided)
            .OrderByDescending(x => x.PurchasedAtUtc).Take(5).Select(x => Maps.Purchase(x, x.Producer.FullName)).ToListAsync(ct);
        var openRegister = await db.CashRegisters.AsNoTracking().Include(x => x.Movements)
            .SingleOrDefaultAsync(x => x.Status == CashRegisterStatus.Abierta, ct);
        decimal? currentCash = openRegister is null ? null : CashOperations.ExpectedBalance(openRegister);
        return new DashboardDto(await purchases.SumAsync(x => (decimal?)x.TotalPaid, ct) ?? 0,
            await sales.SumAsync(x => (decimal?)x.Total, ct) ?? 0, await sales.SumAsync(x => (decimal?)x.GrossProfit, ct) ?? 0,
            inventory.Sum(x => x.QuantityQuintals), await db.Producers.CountAsync(x => x.IsActive, ct), await purchases.CountAsync(ct),
            currentCash, recent, inventory);
    }
}
