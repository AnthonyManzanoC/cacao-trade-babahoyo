using Microsoft.EntityFrameworkCore;
using OrigenCacao.Application;
using OrigenCacao.Domain;

namespace OrigenCacao.Infrastructure;

internal static class CashOperations
{
    public static decimal ExpectedBalance(CashRegister register) => decimal.Round(register.OpeningBalance +
        register.Movements.Where(x => x.PaymentMethod == PaymentMethod.Efectivo && x.Direction == CashMovementDirection.Ingreso).Sum(x => x.Amount) -
        register.Movements.Where(x => x.PaymentMethod == PaymentMethod.Efectivo && x.Direction == CashMovementDirection.Egreso).Sum(x => x.Amount), 2);

    public static async Task AddAutomaticAsync(AppDbContext db, CashMovementDirection direction,
        CashMovementCategory category, decimal amount, string description, Guid referenceId, string referenceCode,
        PaymentMethod paymentMethod, DateTime occurredAtUtc, bool requireSufficientCash, CancellationToken ct)
    {
        if (amount <= 0) throw new ArgumentException("El movimiento de caja debe ser mayor que cero.");
        var register = await db.CashRegisters.Include(x => x.Movements)
            .SingleOrDefaultAsync(x => x.Status == CashRegisterStatus.Abierta, ct)
            ?? throw new InvalidOperationException("Abre la caja del día antes de registrar operaciones en efectivo.");
        if (requireSufficientCash && paymentMethod == PaymentMethod.Efectivo && ExpectedBalance(register) < amount)
            throw new InvalidOperationException($"Caja insuficiente. Disponible: {ExpectedBalance(register):C2}.");
        var movement = new CashMovement
        {
            CashRegister = register, CashRegisterId = register.Id,
            Direction = direction, Category = category, Amount = decimal.Round(amount, 2),
            Description = description, ReferenceId = referenceId, ReferenceCode = referenceCode,
            PaymentMethod = paymentMethod, OccurredAtUtc = occurredAtUtc
        };
        db.CashMovements.Add(movement);
        register.UpdatedAtUtc = DateTime.UtcNow;
    }
}

public sealed class PublicContentService(AppDbContext db) : IPublicContentService
{
    public async Task<IReadOnlyList<PublicContentDto>> ListPublicAsync(PublicContentSection? section, CancellationToken ct)
    {
        var query = db.PublicContents.AsNoTracking().Where(x => x.IsPublished);
        if (section.HasValue) query = query.Where(x => x.Section == section.Value);
        return await query.OrderBy(x => x.Section).ThenBy(x => x.DisplayOrder).Select(ToDto()).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PublicContentDto>> ListAdminAsync(CancellationToken ct) =>
        await db.PublicContents.AsNoTracking().OrderBy(x => x.Section).ThenBy(x => x.DisplayOrder).Select(ToDto()).ToListAsync(ct);

    public async Task<PublicContentDto?> GetAsync(Guid id, CancellationToken ct) =>
        await db.PublicContents.AsNoTracking().Where(x => x.Id == id).Select(ToDto()).SingleOrDefaultAsync(ct);

    public async Task<PublicContentDto> CreateAsync(UpsertPublicContentRequest request, CancellationToken ct)
    {
        Validate(request);
        var key = NormalizeKey(request.ContentKey);
        if (await db.PublicContents.AnyAsync(x => x.ContentKey == key, ct))
            throw new InvalidOperationException("Ya existe un bloque con esa clave.");
        var entity = new PublicContent();
        Apply(entity, request, key);
        db.PublicContents.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<PublicContentDto?> UpdateAsync(Guid id, UpsertPublicContentRequest request, CancellationToken ct)
    {
        Validate(request);
        var entity = await db.PublicContents.FindAsync([id], ct);
        if (entity is null) return null;
        var key = NormalizeKey(request.ContentKey);
        if (await db.PublicContents.AnyAsync(x => x.Id != id && x.ContentKey == key, ct))
            throw new InvalidOperationException("Ya existe un bloque con esa clave.");
        Apply(entity, request, key);
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.PublicContents.FindAsync([id], ct);
        if (entity is null) return false;
        db.PublicContents.Remove(entity);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static System.Linq.Expressions.Expression<Func<PublicContent, PublicContentDto>> ToDto() => x =>
        new PublicContentDto(x.Id, x.ContentKey, x.Section, x.Eyebrow, x.Title, x.Subtitle, x.Body,
            x.PrimaryCtaLabel, x.PrimaryCtaUrl, x.SecondaryCtaLabel, x.SecondaryCtaUrl,
            x.Icon, x.ImageUrl, x.DisplayOrder, x.IsPublished, x.UpdatedAtUtc);

    private static PublicContentDto Map(PublicContent x) => new(x.Id, x.ContentKey, x.Section, x.Eyebrow,
        x.Title, x.Subtitle, x.Body, x.PrimaryCtaLabel, x.PrimaryCtaUrl, x.SecondaryCtaLabel,
        x.SecondaryCtaUrl, x.Icon, x.ImageUrl, x.DisplayOrder, x.IsPublished, x.UpdatedAtUtc);

    private static string NormalizeKey(string key) => key.Trim().ToLowerInvariant().Replace(' ', '-');
    private static void Validate(UpsertPublicContentRequest x)
    {
        if (string.IsNullOrWhiteSpace(x.ContentKey) || string.IsNullOrWhiteSpace(x.Title))
            throw new ArgumentException("La clave y el título son obligatorios.");
        if (x.DisplayOrder < 0) throw new ArgumentException("El orden no puede ser negativo.");
    }
    private static void Apply(PublicContent e, UpsertPublicContentRequest x, string key)
    {
        e.ContentKey = key; e.Section = x.Section; e.Eyebrow = x.Eyebrow.Trim(); e.Title = x.Title.Trim();
        e.Subtitle = x.Subtitle.Trim(); e.Body = x.Body.Trim(); e.PrimaryCtaLabel = x.PrimaryCtaLabel?.Trim();
        e.PrimaryCtaUrl = x.PrimaryCtaUrl?.Trim(); e.SecondaryCtaLabel = x.SecondaryCtaLabel?.Trim();
        e.SecondaryCtaUrl = x.SecondaryCtaUrl?.Trim(); e.Icon = x.Icon?.Trim(); e.ImageUrl = x.ImageUrl?.Trim();
        e.DisplayOrder = x.DisplayOrder; e.IsPublished = x.IsPublished;
    }
}

public sealed class CashRegisterService(AppDbContext db) : ICashRegisterService
{
    public async Task<CashRegisterDto?> GetCurrentAsync(CancellationToken ct)
    {
        var entity = await db.CashRegisters.AsNoTracking().Include(x => x.Movements)
            .SingleOrDefaultAsync(x => x.Status == CashRegisterStatus.Abierta, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<IReadOnlyList<CashRegisterDto>> ListAsync(int take, CancellationToken ct) =>
        (await db.CashRegisters.AsNoTracking().Include(x => x.Movements).OrderByDescending(x => x.BusinessDate)
            .Take(Math.Clamp(take, 1, 180)).ToListAsync(ct)).Select(Map).ToList();

    public async Task<CashRegisterDto> OpenAsync(OpenCashRegisterRequest request, CancellationToken ct)
    {
        if (request.OpeningBalance < 0) throw new ArgumentException("El saldo de apertura no puede ser negativo.");
        if (await db.CashRegisters.AnyAsync(x => x.Status == CashRegisterStatus.Abierta, ct))
            throw new InvalidOperationException("Ya existe una caja abierta. Ciérrala antes de abrir otra.");
        if (await db.CashRegisters.AnyAsync(x => x.BusinessDate == request.BusinessDate, ct))
            throw new InvalidOperationException("Ya existe una caja para esa fecha.");
        var entity = new CashRegister { BusinessDate = request.BusinessDate, OpeningBalance = request.OpeningBalance,
            OpenedAtUtc = DateTime.UtcNow, Status = CashRegisterStatus.Abierta, Notes = request.Notes?.Trim() };
        db.CashRegisters.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<CashRegisterDto> AddMovementAsync(Guid id, AddCashMovementRequest request, CancellationToken ct)
    {
        var entity = await db.CashRegisters.Include(x => x.Movements).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Caja no encontrada.");
        if (entity.Status != CashRegisterStatus.Abierta) throw new InvalidOperationException("La caja ya está cerrada.");
        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Importe y descripción son obligatorios.");
        if (request.Direction == CashMovementDirection.Egreso && request.PaymentMethod == PaymentMethod.Efectivo &&
            CashOperations.ExpectedBalance(entity) < request.Amount)
            throw new InvalidOperationException($"Caja insuficiente. Disponible: {CashOperations.ExpectedBalance(entity):C2}.");
        var movement = new CashMovement { CashRegister = entity, CashRegisterId = entity.Id,
            Direction = request.Direction, Category = request.Category,
            Amount = decimal.Round(request.Amount, 2), Description = request.Description.Trim(),
            PaymentMethod = request.PaymentMethod, OccurredAtUtc = PurchaseService.ToUtc(request.OccurredAtUtc ?? DateTime.UtcNow) };
        db.CashMovements.Add(movement);
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<CashRegisterDto> CloseAsync(Guid id, CloseCashRegisterRequest request, CancellationToken ct)
    {
        var entity = await db.CashRegisters.Include(x => x.Movements).SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException("Caja no encontrada.");
        if (entity.Status != CashRegisterStatus.Abierta) throw new InvalidOperationException("La caja ya está cerrada.");
        if (request.CountedClosingBalance < 0) throw new ArgumentException("El efectivo contado no puede ser negativo.");
        entity.ExpectedClosingBalance = CashOperations.ExpectedBalance(entity);
        entity.CountedClosingBalance = request.CountedClosingBalance;
        entity.ClosingDifference = decimal.Round(request.CountedClosingBalance - entity.ExpectedClosingBalance.Value, 2);
        entity.Status = CashRegisterStatus.Cerrada; entity.ClosedAtUtc = DateTime.UtcNow;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? entity.Notes : request.Notes.Trim();
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    private static CashRegisterDto Map(CashRegister x)
    {
        var movements = x.Movements.OrderByDescending(m => m.OccurredAtUtc).Select(m => new CashMovementDto(m.Id,
            m.Direction, m.Category, m.Amount, m.Description, m.ReferenceId, m.ReferenceCode,
            m.PaymentMethod, m.OccurredAtUtc)).ToList();
        var cash = x.Movements.Where(m => m.PaymentMethod == PaymentMethod.Efectivo).ToList();
        var income = cash.Where(m => m.Direction == CashMovementDirection.Ingreso).Sum(m => m.Amount);
        var expense = cash.Where(m => m.Direction == CashMovementDirection.Egreso).Sum(m => m.Amount);
        return new CashRegisterDto(x.Id, x.BusinessDate, x.OpeningBalance, income, expense,
            decimal.Round(x.OpeningBalance + income - expense, 2), x.CountedClosingBalance, x.ClosingDifference,
            x.Status, x.OpenedAtUtc, x.ClosedAtUtc, x.Notes, movements);
    }
}

public sealed class ProcessingService(AppDbContext db) : IProcessingService
{
    public async Task<IReadOnlyList<ProcessingBatchDto>> ListAsync(int take, CancellationToken ct) =>
        await db.ProcessingBatches.AsNoTracking().OrderByDescending(x => x.StartedAtUtc)
            .Take(Math.Clamp(take, 1, 500)).Select(x => new ProcessingBatchDto(x.Id, x.Code, x.Variety,
                x.InputWetQuintals, x.ExpectedDryYieldPercent, x.OutputDryQuintals, x.ActualDryYieldPercent,
                x.LossPercent, x.InputUnitCost, x.OutputUnitCost, x.Status, x.StartedAtUtc,
                x.CompletedAtUtc, x.Notes)).ToListAsync(ct);

    public async Task<ProcessingBatchDto> CreateAsync(CreateProcessingBatchRequest request, CancellationToken ct)
    {
        if (request.InputWetQuintals <= 0 || request.ExpectedDryYieldPercent is <= 0 or > 100)
            throw new ArgumentException("Cantidad y rendimiento esperado son inválidos.");
        var lots = await db.InventoryLots.Where(x => x.Variety == request.Variety && x.State == CocoaState.Baba &&
            x.Status == InventoryLotStatus.Disponible && x.AvailableQuantityQuintals > 0)
            .OrderBy(x => x.ReceivedAtUtc).ThenBy(x => x.Code).ToListAsync(ct);
        var available = lots.Sum(x => x.AvailableQuantityQuintals);
        if (available < request.InputWetQuintals)
            throw new InvalidOperationException($"Cacao en baba insuficiente. Disponible: {available:N2} qq.");
        var entity = new ProcessingBatch { Code = await PurchaseService.NextCode("SEC", db.ProcessingBatches.Select(x => x.Code), ct),
            Variety = request.Variety, InputWetQuintals = request.InputWetQuintals,
            ExpectedDryYieldPercent = request.ExpectedDryYieldPercent,
            Status = ProcessingStatus.EnProceso, StartedAtUtc = PurchaseService.ToUtc(request.StartedAtUtc ?? DateTime.UtcNow),
            Notes = request.Notes?.Trim() };
        var remaining = request.InputWetQuintals;
        decimal inputValue = 0;
        foreach (var lot in lots)
        {
            if (remaining <= 0) break;
            var quantity = Math.Min(lot.AvailableQuantityQuintals, remaining);
            lot.AvailableQuantityQuintals -= quantity;
            lot.Status = lot.AvailableQuantityQuintals == 0 ? InventoryLotStatus.EnProceso : InventoryLotStatus.Disponible;
            lot.UpdatedAtUtc = DateTime.UtcNow;
            inputValue += quantity * lot.UnitCost;
            entity.LotAllocations.Add(new ProcessingLotAllocation { InventoryLot = lot,
                InventoryLotId = lot.Id, QuantityQuintals = quantity, UnitCost = lot.UnitCost });
            remaining -= quantity;
        }
        entity.InputUnitCost = decimal.Round(inputValue / request.InputWetQuintals, 2);
        db.ProcessingBatches.Add(entity);
        db.InventoryMovements.Add(new InventoryMovement { Type = MovementType.SecadoSalidaBaba,
            Variety = entity.Variety, State = CocoaState.Baba, QuantityQuintals = -entity.InputWetQuintals,
            UnitAmount = entity.InputUnitCost, ReferenceId = entity.Id, ReferenceCode = entity.Code,
            OccurredAtUtc = entity.StartedAtUtc, Notes = "Cacao en baba enviado al proceso de secado" });
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<ProcessingBatchDto> CompleteAsync(Guid id, CompleteProcessingBatchRequest request, CancellationToken ct)
    {
        if (request.OutputDryQuintals <= 0) throw new ArgumentException("La salida seca debe ser mayor que cero.");
        var entity = await db.ProcessingBatches.Include(x => x.LotAllocations).ThenInclude(x => x.InventoryLot)
            .SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Proceso no encontrado.");
        if (entity.Status != ProcessingStatus.EnProceso) throw new InvalidOperationException("El proceso ya fue finalizado.");
        if (request.OutputDryQuintals > entity.InputWetQuintals)
            throw new ArgumentException("La salida seca no puede superar la entrada en baba.");
        var inputValue = entity.InputWetQuintals * entity.InputUnitCost;
        entity.OutputDryQuintals = request.OutputDryQuintals;
        entity.ActualDryYieldPercent = decimal.Round(request.OutputDryQuintals / entity.InputWetQuintals * 100m, 2);
        entity.LossPercent = decimal.Round(100m - entity.ActualDryYieldPercent.Value, 2);
        entity.OutputUnitCost = decimal.Round(inputValue / request.OutputDryQuintals, 2);
        entity.Status = ProcessingStatus.Completado;
        entity.CompletedAtUtc = PurchaseService.ToUtc(request.CompletedAtUtc ?? DateTime.UtcNow);
        if (!string.IsNullOrWhiteSpace(request.Notes)) entity.Notes = request.Notes.Trim();
        entity.UpdatedAtUtc = DateTime.UtcNow;
        foreach (var allocation in entity.LotAllocations.Where(x => x.InventoryLot.AvailableQuantityQuintals == 0))
            allocation.InventoryLot.Status = InventoryLotStatus.Agotado;
        db.InventoryLots.Add(new InventoryLot { Code = $"LOT-{entity.Code}-SECO", ProcessingBatch = entity,
            ProcessingBatchId = entity.Id, Variety = entity.Variety, State = CocoaState.Seco,
            InitialQuantityQuintals = request.OutputDryQuintals, AvailableQuantityQuintals = request.OutputDryQuintals,
            UnitCost = entity.OutputUnitCost.Value, HumidityPercent = 0,
            Status = InventoryLotStatus.Disponible, ReceivedAtUtc = entity.CompletedAtUtc.Value,
            Notes = $"Lote seco producido por {entity.Code}" });
        db.InventoryMovements.Add(new InventoryMovement { Type = MovementType.SecadoEntradaSeco,
            Variety = entity.Variety, State = CocoaState.Seco, QuantityQuintals = request.OutputDryQuintals,
            UnitAmount = entity.OutputUnitCost.Value, ReferenceId = entity.Id, ReferenceCode = entity.Code,
            OccurredAtUtc = entity.CompletedAtUtc.Value, Notes = "Entrada de cacao seco terminado" });
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<ProcessingBatchDto> CancelAsync(Guid id, CancellationToken ct)
    {
        var entity = await db.ProcessingBatches.Include(x => x.LotAllocations).ThenInclude(x => x.InventoryLot)
            .SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new KeyNotFoundException("Proceso no encontrado.");
        if (entity.Status != ProcessingStatus.EnProceso) throw new InvalidOperationException("Solo se puede cancelar un proceso en curso.");
        foreach (var allocation in entity.LotAllocations)
        {
            allocation.InventoryLot.AvailableQuantityQuintals += allocation.QuantityQuintals;
            allocation.InventoryLot.Status = InventoryLotStatus.Disponible;
            allocation.InventoryLot.UpdatedAtUtc = DateTime.UtcNow;
        }
        entity.Status = ProcessingStatus.Cancelado; entity.CompletedAtUtc = DateTime.UtcNow; entity.UpdatedAtUtc = DateTime.UtcNow;
        db.InventoryMovements.Add(new InventoryMovement { Type = MovementType.Reversion, Variety = entity.Variety,
            State = CocoaState.Baba, QuantityQuintals = entity.InputWetQuintals, UnitAmount = entity.InputUnitCost,
            ReferenceId = entity.Id, ReferenceCode = entity.Code, OccurredAtUtc = DateTime.UtcNow,
            Notes = "Reversión por cancelación del secado" });
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    private static ProcessingBatchDto Map(ProcessingBatch x) => new(x.Id, x.Code, x.Variety,
        x.InputWetQuintals, x.ExpectedDryYieldPercent, x.OutputDryQuintals, x.ActualDryYieldPercent,
        x.LossPercent, x.InputUnitCost, x.OutputUnitCost, x.Status, x.StartedAtUtc, x.CompletedAtUtc, x.Notes);
}
