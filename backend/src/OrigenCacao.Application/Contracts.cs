using OrigenCacao.Domain;

namespace OrigenCacao.Application;

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string Token, DateTime ExpiresAtUtc, string FullName, string Email);

public sealed record ProducerDto(Guid Id, string FullName, string DocumentNumber, string FarmLocation,
    string Phone, string? Email, PaymentMethod PreferredPaymentMethod, string? Notes, bool IsActive, int PurchaseCount,
    decimal TotalQuintals, decimal TotalPaid, DateTime CreatedAtUtc);
public sealed record UpsertProducerRequest(string FullName, string DocumentNumber, string FarmLocation,
    string Phone, string? Email, PaymentMethod PreferredPaymentMethod, string? Notes, bool IsActive = true);

public sealed record PurchaseDto(Guid Id, string Code, Guid ProducerId, string ProducerName, string? ProducerEmail,
    CocoaVariety Variety, CocoaState State, decimal GrossWeightLbs, decimal TareLbs,
    decimal HumidityPercent, decimal ShrinkagePercent, decimal NetWeightLbs,
    decimal PayableQuintals, decimal UnitPrice, decimal TotalPaid, PaymentMethod PaymentMethod,
    DateTime PurchasedAtUtc, string? Notes, bool IsVoided);
public sealed record CreatePurchaseRequest(Guid ProducerId, CocoaVariety Variety, CocoaState State,
    decimal GrossWeightLbs, decimal TareLbs, decimal HumidityPercent, decimal ShrinkagePercent,
    decimal? UnitPrice, PaymentMethod PaymentMethod, DateTime? PurchasedAtUtc, string? Notes);
public sealed record PurchasePreviewRequest(decimal GrossWeightLbs, decimal TareLbs,
    decimal HumidityPercent, decimal ShrinkagePercent, decimal UnitPrice);

public sealed record SaleDto(Guid Id, string Code, string CustomerName, string? CustomerTaxId, string? CustomerEmail,
    CocoaVariety Variety, CocoaState State, decimal QuantityQuintals, decimal UnitPrice,
    decimal CostBasisPerQuintal, decimal Total, decimal GrossProfit, PaymentMethod PaymentMethod, DateTime SoldAtUtc,
    string? Notes, bool IsVoided);
public sealed record CreateSaleRequest(string CustomerName, string? CustomerTaxId, string? CustomerEmail,
    CocoaVariety Variety, CocoaState State, decimal QuantityQuintals, decimal UnitPrice,
    PaymentMethod PaymentMethod, DateTime? SoldAtUtc, string? Notes);

public sealed record InventoryItemDto(CocoaVariety Variety, CocoaState State,
    decimal QuantityQuintals, decimal AveragePurchaseCost, decimal EstimatedValue);
public sealed record InventoryLotDto(Guid Id, string Code, Guid? PurchaseId, Guid? ProcessingBatchId,
    CocoaVariety Variety, CocoaState State, decimal InitialQuantityQuintals,
    decimal AvailableQuantityQuintals, decimal UnitCost, decimal HumidityPercent,
    InventoryLotStatus Status, DateTime ReceivedAtUtc, string? Notes);

public sealed record PublicPriceDto(string BusinessName, decimal DryPricePerQuintal,
    decimal WetPricePerQuintal, decimal MarketPricePerMetricTon, DateTime UpdatedAtUtc,
    string Source, bool IsManual, string ContactWhatsApp, string ContactAddress, string ContactPhone,
    string ContactEmail, string GoogleMapsEmbedUrl, string Location, bool PickupEnabled,
    DateTime NextAutomaticRefreshAtUtc);
public sealed record PricePointDto(DateTime QuotedAtUtc, decimal DryPricePerQuintal,
    decimal WetPricePerQuintal, decimal MarketPricePerMetricTon, string Source);
public sealed record SettingsDto(string BusinessName, decimal MarginPerQuintal, decimal WetPriceFactor,
    bool UseManualPrice, decimal? ManualDryPricePerQuintal, decimal CurrentMarketPricePerMetricTon,
    decimal CurrentDryPricePerQuintal, decimal CurrentWetPricePerQuintal,
    DateTime CurrentPriceUpdatedAtUtc, DateTime? ApiLastSuccessAtUtc, string? ApiLastError,
    string PriceSource, string ContactWhatsApp, string ContactAddress, string ContactPhone,
    string ContactEmail, string GoogleMapsEmbedUrl, string Location, bool PickupEnabled,
    bool EmailSendingEnabled, string SmtpHost, int SmtpPort, string SmtpEmail,
    bool SmtpPasswordConfigured, bool SmtpUseSsl);
public sealed record UpdateSettingsRequest(string BusinessName, decimal MarginPerQuintal,
    decimal WetPriceFactor, bool UseManualPrice, decimal? ManualDryPricePerQuintal,
    string ContactWhatsApp, string ContactAddress, string ContactPhone, string ContactEmail,
    string GoogleMapsEmbedUrl, string Location, bool PickupEnabled, bool EmailSendingEnabled,
    string SmtpHost, int SmtpPort, string SmtpEmail, string? SmtpPassword, bool SmtpUseSsl);

public sealed record PublicContentDto(Guid Id, string ContentKey, PublicContentSection Section,
    string Eyebrow, string Title, string Subtitle, string Body, string? PrimaryCtaLabel,
    string? PrimaryCtaUrl, string? SecondaryCtaLabel, string? SecondaryCtaUrl, string? Icon,
    string? ImageUrl, int DisplayOrder, bool IsPublished, DateTime UpdatedAtUtc);
public sealed record UpsertPublicContentRequest(string ContentKey, PublicContentSection Section,
    string Eyebrow, string Title, string Subtitle, string Body, string? PrimaryCtaLabel,
    string? PrimaryCtaUrl, string? SecondaryCtaLabel, string? SecondaryCtaUrl, string? Icon,
    string? ImageUrl, int DisplayOrder, bool IsPublished);

public sealed record CashMovementDto(Guid Id, CashMovementDirection Direction, CashMovementCategory Category,
    decimal Amount, string Description, Guid? ReferenceId, string? ReferenceCode,
    PaymentMethod PaymentMethod, DateTime OccurredAtUtc);
public sealed record CashRegisterDto(Guid Id, DateOnly BusinessDate, decimal OpeningBalance,
    decimal TotalIncome, decimal TotalExpense, decimal ExpectedBalance, decimal? CountedClosingBalance,
    decimal? ClosingDifference, CashRegisterStatus Status, DateTime OpenedAtUtc, DateTime? ClosedAtUtc,
    string? Notes, IReadOnlyList<CashMovementDto> Movements);
public sealed record OpenCashRegisterRequest(DateOnly BusinessDate, decimal OpeningBalance, string? Notes);
public sealed record AddCashMovementRequest(CashMovementDirection Direction, CashMovementCategory Category,
    decimal Amount, string Description, PaymentMethod PaymentMethod, DateTime? OccurredAtUtc);
public sealed record CloseCashRegisterRequest(decimal CountedClosingBalance, string? Notes);

public sealed record ProcessingBatchDto(Guid Id, string Code, CocoaVariety Variety,
    decimal InputWetQuintals, decimal ExpectedDryYieldPercent, decimal? OutputDryQuintals,
    decimal? ActualDryYieldPercent, decimal? LossPercent, decimal InputUnitCost,
    decimal? OutputUnitCost, ProcessingStatus Status, DateTime StartedAtUtc,
    DateTime? CompletedAtUtc, string? Notes);
public sealed record CreateProcessingBatchRequest(CocoaVariety Variety, decimal InputWetQuintals,
    decimal ExpectedDryYieldPercent, DateTime? StartedAtUtc, string? Notes);
public sealed record CompleteProcessingBatchRequest(decimal OutputDryQuintals,
    DateTime? CompletedAtUtc, string? Notes);

public sealed record DashboardDto(decimal PurchasesThisMonth, decimal SalesThisMonth,
    decimal GrossProfitThisMonth, decimal InventoryQuintals, int ActiveProducers,
    int PurchasesCountThisMonth, decimal? CurrentCashBalance, IReadOnlyList<PurchaseDto> RecentPurchases,
    IReadOnlyList<InventoryItemDto> Inventory);

public interface IAuthService { Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct); }
public interface IProducerService
{
    Task<IReadOnlyList<ProducerDto>> ListAsync(string? search, CancellationToken ct);
    Task<ProducerDto?> GetAsync(Guid id, CancellationToken ct);
    Task<ProducerDto> CreateAsync(UpsertProducerRequest request, CancellationToken ct);
    Task<ProducerDto?> UpdateAsync(Guid id, UpsertProducerRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
public interface IPurchaseService
{
    Task<IReadOnlyList<PurchaseDto>> ListAsync(int take, CancellationToken ct);
    Task<PurchaseDto?> GetAsync(Guid id, CancellationToken ct);
    Task<PurchaseDto> CreateAsync(CreatePurchaseRequest request, CancellationToken ct);
    Task<bool> VoidAsync(Guid id, CancellationToken ct);
}
public sealed record PurchaseReceiptResult(byte[] Content, string FileName);
public sealed record ReceiptEmailRequest(string? Email);
public sealed record EmailSendResult(bool Sent, string Recipient, string Message);
public interface IPurchaseReceiptService
{
    Task<PurchaseReceiptResult?> GenerateAsync(Guid purchaseId, CancellationToken ct);
}
public interface ISaleReceiptService
{
    Task<PurchaseReceiptResult?> GenerateAsync(Guid saleId, CancellationToken ct);
}
public interface IEmailService
{
    Task SendReceiptAsync(string recipient, string subject, string body, byte[] content, string fileName, CancellationToken ct);
}
public interface ISaleService
{
    Task<IReadOnlyList<SaleDto>> ListAsync(int take, CancellationToken ct);
    Task<SaleDto?> GetAsync(Guid id, CancellationToken ct);
    Task<SaleDto> CreateAsync(CreateSaleRequest request, CancellationToken ct);
    Task<bool> VoidAsync(Guid id, CancellationToken ct);
}
public interface IInventoryService
{
    Task<IReadOnlyList<InventoryItemDto>> GetAsync(CancellationToken ct);
    Task<IReadOnlyList<InventoryLotDto>> GetLotsAsync(CocoaState? state, CancellationToken ct);
}
public interface IPublicContentService
{
    Task<IReadOnlyList<PublicContentDto>> ListPublicAsync(PublicContentSection? section, CancellationToken ct);
    Task<IReadOnlyList<PublicContentDto>> ListAdminAsync(CancellationToken ct);
    Task<PublicContentDto?> GetAsync(Guid id, CancellationToken ct);
    Task<PublicContentDto> CreateAsync(UpsertPublicContentRequest request, CancellationToken ct);
    Task<PublicContentDto?> UpdateAsync(Guid id, UpsertPublicContentRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
public interface ICashRegisterService
{
    Task<CashRegisterDto?> GetCurrentAsync(CancellationToken ct);
    Task<IReadOnlyList<CashRegisterDto>> ListAsync(int take, CancellationToken ct);
    Task<CashRegisterDto> OpenAsync(OpenCashRegisterRequest request, CancellationToken ct);
    Task<CashRegisterDto> AddMovementAsync(Guid id, AddCashMovementRequest request, CancellationToken ct);
    Task<CashRegisterDto> CloseAsync(Guid id, CloseCashRegisterRequest request, CancellationToken ct);
}
public interface IProcessingService
{
    Task<IReadOnlyList<ProcessingBatchDto>> ListAsync(int take, CancellationToken ct);
    Task<ProcessingBatchDto> CreateAsync(CreateProcessingBatchRequest request, CancellationToken ct);
    Task<ProcessingBatchDto> CompleteAsync(Guid id, CompleteProcessingBatchRequest request, CancellationToken ct);
    Task<ProcessingBatchDto> CancelAsync(Guid id, CancellationToken ct);
}
public interface ISettingsService
{
    Task<PublicPriceDto> GetPublicPriceAsync(CancellationToken ct);
    Task<IReadOnlyList<PricePointDto>> GetHistoryAsync(int days, CancellationToken ct);
    Task<SettingsDto> GetAsync(CancellationToken ct);
    Task<SettingsDto> UpdateAsync(UpdateSettingsRequest request, CancellationToken ct);
}
public interface IDashboardService { Task<DashboardDto> GetAsync(CancellationToken ct); }
public interface ICocoaPriceUpdater { Task<PriceUpdateResult> RefreshAsync(CancellationToken ct); }
public sealed record PriceUpdateResult(bool Updated, decimal? MarketPrice, decimal? DryPrice, string Message);
