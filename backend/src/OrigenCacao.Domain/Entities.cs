namespace OrigenCacao.Domain;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public enum CocoaVariety { Nacional, Ccn51, Trinitario, Otro }
public enum CocoaState { Baba, Seco }
public enum PaymentMethod { Efectivo, Transferencia, Cheque, Otro }
public enum MovementType { Compra, Venta, AjusteEntrada, AjusteSalida, Reversion, SecadoSalidaBaba, SecadoEntradaSeco }
public enum PublicContentSection
{
    Hero,
    Nosotros,
    Contacto,
    Servicio,
    Beneficio,
    Proceso,
    Impacto,
    Testimonio,
    Galeria,
    Footer,
    CarruselNosotros
}
public enum CashRegisterStatus { Abierta, Cerrada }
public enum CashMovementDirection { Ingreso, Egreso }
public enum CashMovementCategory { CompraCacao, VentaCacao, GastoOperativo, Aporte, Retiro, Ajuste }
public enum ProcessingStatus { EnProceso, Completado, Cancelado }
public enum InventoryLotStatus { Disponible, Agotado, EnProceso, Anulado }

public sealed class Producer : Entity
{
    public string FullName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string FarmLocation { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public PaymentMethod PreferredPaymentMethod { get; set; } = PaymentMethod.Efectivo;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}

public sealed class Purchase : Entity
{
    public string Code { get; set; } = string.Empty;
    public Guid ProducerId { get; set; }
    public Producer Producer { get; set; } = null!;
    public CocoaVariety Variety { get; set; }
    public CocoaState State { get; set; }
    public decimal GrossWeightLbs { get; set; }
    public decimal TareLbs { get; set; }
    public decimal HumidityPercent { get; set; }
    public decimal ShrinkagePercent { get; set; }
    public decimal NetWeightLbs { get; set; }
    public decimal PayableQuintals { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPaid { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime PurchasedAtUtc { get; set; }
    public string? Notes { get; set; }
    public bool IsVoided { get; set; }
}

public sealed class Sale : Entity
{
    public string Code { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerTaxId { get; set; }
    public string? CustomerEmail { get; set; }
    public CocoaVariety Variety { get; set; }
    public CocoaState State { get; set; }
    public decimal QuantityQuintals { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostBasisPerQuintal { get; set; }
    public decimal Total { get; set; }
    public decimal GrossProfit { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Transferencia;
    public DateTime SoldAtUtc { get; set; }
    public string? Notes { get; set; }
    public bool IsVoided { get; set; }
    public ICollection<SaleLotAllocation> LotAllocations { get; set; } = new List<SaleLotAllocation>();
}

public sealed class PublicContent : Entity
{
    public string ContentKey { get; set; } = string.Empty;
    public PublicContentSection Section { get; set; }
    public string Eyebrow { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? PrimaryCtaLabel { get; set; }
    public string? PrimaryCtaUrl { get; set; }
    public string? SecondaryCtaLabel { get; set; }
    public string? SecondaryCtaUrl { get; set; }
    public string? Icon { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; } = true;
}

public sealed class CashRegister : Entity
{
    public DateOnly BusinessDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal? CountedClosingBalance { get; set; }
    public decimal? ExpectedClosingBalance { get; set; }
    public decimal? ClosingDifference { get; set; }
    public CashRegisterStatus Status { get; set; } = CashRegisterStatus.Abierta;
    public DateTime OpenedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAtUtc { get; set; }
    public string? Notes { get; set; }
    public ICollection<CashMovement> Movements { get; set; } = new List<CashMovement>();
}

public sealed class CashMovement : Entity
{
    public Guid CashRegisterId { get; set; }
    public CashRegister CashRegister { get; set; } = null!;
    public CashMovementDirection Direction { get; set; }
    public CashMovementCategory Category { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public string? ReferenceCode { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Efectivo;
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class ProcessingBatch : Entity
{
    public string Code { get; set; } = string.Empty;
    public CocoaVariety Variety { get; set; }
    public decimal InputWetQuintals { get; set; }
    public decimal ExpectedDryYieldPercent { get; set; }
    public decimal? OutputDryQuintals { get; set; }
    public decimal? ActualDryYieldPercent { get; set; }
    public decimal? LossPercent { get; set; }
    public decimal InputUnitCost { get; set; }
    public decimal? OutputUnitCost { get; set; }
    public ProcessingStatus Status { get; set; } = ProcessingStatus.EnProceso;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? Notes { get; set; }
    public ICollection<ProcessingLotAllocation> LotAllocations { get; set; } = new List<ProcessingLotAllocation>();
}

public sealed class InventoryLot : Entity
{
    public string Code { get; set; } = string.Empty;
    public Guid? PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public Guid? ProcessingBatchId { get; set; }
    public ProcessingBatch? ProcessingBatch { get; set; }
    public CocoaVariety Variety { get; set; }
    public CocoaState State { get; set; }
    public decimal InitialQuantityQuintals { get; set; }
    public decimal AvailableQuantityQuintals { get; set; }
    public decimal UnitCost { get; set; }
    public decimal HumidityPercent { get; set; }
    public InventoryLotStatus Status { get; set; } = InventoryLotStatus.Disponible;
    public DateTime ReceivedAtUtc { get; set; }
    public string? Notes { get; set; }
    public ICollection<SaleLotAllocation> SaleAllocations { get; set; } = new List<SaleLotAllocation>();
    public ICollection<ProcessingLotAllocation> ProcessingAllocations { get; set; } = new List<ProcessingLotAllocation>();
}

public sealed class SaleLotAllocation : Entity
{
    public Guid SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public Guid InventoryLotId { get; set; }
    public InventoryLot InventoryLot { get; set; } = null!;
    public decimal QuantityQuintals { get; set; }
    public decimal UnitCost { get; set; }
}

public sealed class ProcessingLotAllocation : Entity
{
    public Guid ProcessingBatchId { get; set; }
    public ProcessingBatch ProcessingBatch { get; set; } = null!;
    public Guid InventoryLotId { get; set; }
    public InventoryLot InventoryLot { get; set; } = null!;
    public decimal QuantityQuintals { get; set; }
    public decimal UnitCost { get; set; }
}

public sealed class InventoryMovement : Entity
{
    public MovementType Type { get; set; }
    public CocoaVariety Variety { get; set; }
    public CocoaState State { get; set; }
    public decimal QuantityQuintals { get; set; }
    public decimal UnitAmount { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string? Notes { get; set; }
}

public sealed class BusinessSettings
{
    public int Id { get; set; } = 1;
    public string BusinessName { get; set; } = "Grupo Álvarez";
    public string LogoUrl { get; set; } = "/grupo-alvarez-cacao-logo.png";
    public string PriceClockLabel { get; set; } = "Hora Ecuador";
    public string TimeZone { get; set; } = "America/Guayaquil";
    public decimal MarginPerQuintal { get; set; } = 18m;
    public decimal WetPriceFactor { get; set; } = 0.40m;
    public bool UseManualPrice { get; set; } = true;
    public decimal? ManualDryPricePerQuintal { get; set; } = 300m;
    public decimal CurrentMarketPricePerMetricTon { get; set; } = 0m;
    public decimal CurrentDryPricePerQuintal { get; set; } = 300m;
    public decimal CurrentWetPricePerQuintal { get; set; } = 120m;
    public DateTime CurrentPriceUpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ApiLastSuccessAtUtc { get; set; }
    public string? ApiLastError { get; set; }
    public string PriceSource { get; set; } = "Precio manual";
    public string ContactWhatsApp { get; set; } = "+593 99 000 0000";
    public string ContactAddress { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string GoogleMapsEmbedUrl { get; set; } = string.Empty;
    public string Location { get; set; } = "Babahoyo, Los Ríos, Ecuador";
    public bool PickupEnabled { get; set; } = true;
    public bool EmailSendingEnabled { get; set; }
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpEmail { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool SmtpUseSsl { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class PriceHistory : Entity
{
    public decimal MarketPricePerMetricTon { get; set; }
    public decimal DryPricePerQuintal { get; set; }
    public decimal WetPricePerQuintal { get; set; }
    public decimal MarginPerQuintal { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime QuotedAtUtc { get; set; }
}

public sealed class AdminUser : Entity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAtUtc { get; set; }
}
