using Microsoft.EntityFrameworkCore;
using OrigenCacao.Domain;

namespace OrigenCacao.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Producer> Producers => Set<Producer>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();
    public DbSet<BusinessSettings> BusinessSettings => Set<BusinessSettings>();
    public DbSet<PriceHistory> PriceHistory => Set<PriceHistory>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<PublicContent> PublicContents => Set<PublicContent>();
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();
    public DbSet<CashMovement> CashMovements => Set<CashMovement>();
    public DbSet<ProcessingBatch> ProcessingBatches => Set<ProcessingBatch>();
    public DbSet<InventoryLot> InventoryLots => Set<InventoryLot>();
    public DbSet<SaleLotAllocation> SaleLotAllocations => Set<SaleLotAllocation>();
    public DbSet<ProcessingLotAllocation> ProcessingLotAllocations => Set<ProcessingLotAllocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("cacao");

        modelBuilder.Entity<Producer>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.DocumentNumber).IsUnique();
            e.Property(x => x.FullName).HasMaxLength(160).IsRequired();
            e.Property(x => x.DocumentNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Email).HasMaxLength(180);
            e.Property(x => x.FarmLocation).HasMaxLength(240);
        });

        modelBuilder.Entity<Purchase>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.PurchasedAtUtc);
            e.HasOne(x => x.Producer).WithMany(x => x.Purchases).HasForeignKey(x => x.ProducerId).OnDelete(DeleteBehavior.Restrict);
            Decimal(e, x => x.GrossWeightLbs); Decimal(e, x => x.TareLbs); Decimal(e, x => x.HumidityPercent);
            Decimal(e, x => x.ShrinkagePercent); Decimal(e, x => x.NetWeightLbs); Decimal(e, x => x.PayableQuintals, 14, 4);
            Decimal(e, x => x.UnitPrice); Decimal(e, x => x.TotalPaid);
        });

        modelBuilder.Entity<Sale>(e =>
        {
            e.HasKey(x => x.Id); e.HasIndex(x => x.Code).IsUnique(); e.HasIndex(x => x.SoldAtUtc);
            e.Property(x => x.CustomerEmail).HasMaxLength(180);
            Decimal(e, x => x.QuantityQuintals, 14, 4); Decimal(e, x => x.UnitPrice);
            Decimal(e, x => x.CostBasisPerQuintal); Decimal(e, x => x.Total); Decimal(e, x => x.GrossProfit);
        });

        modelBuilder.Entity<InventoryMovement>(e =>
        {
            e.HasKey(x => x.Id); e.HasIndex(x => new { x.Variety, x.State, x.OccurredAtUtc });
            Decimal(e, x => x.QuantityQuintals, 14, 4); Decimal(e, x => x.UnitAmount);
        });

        modelBuilder.Entity<BusinessSettings>(e =>
        {
            e.HasKey(x => x.Id); e.ToTable("BusinessSettings");
            Decimal(e, x => x.MarginPerQuintal); Decimal(e, x => x.WetPriceFactor, 8, 4);
            Decimal(e, x => x.ManualDryPricePerQuintal); Decimal(e, x => x.CurrentMarketPricePerMetricTon);
            Decimal(e, x => x.CurrentDryPricePerQuintal); Decimal(e, x => x.CurrentWetPricePerQuintal);
            e.Property(x => x.ContactAddress).HasMaxLength(400);
            e.Property(x => x.ContactPhone).HasMaxLength(40);
            e.Property(x => x.ContactEmail).HasMaxLength(180);
            e.Property(x => x.GoogleMapsEmbedUrl).HasMaxLength(1200);
            e.Property(x => x.SmtpHost).HasMaxLength(240);
            e.Property(x => x.SmtpEmail).HasMaxLength(180);
            e.Property(x => x.SmtpPassword).HasMaxLength(1000);
        });

        modelBuilder.Entity<PriceHistory>(e =>
        {
            e.HasKey(x => x.Id); e.HasIndex(x => x.QuotedAtUtc);
            Decimal(e, x => x.MarketPricePerMetricTon); Decimal(e, x => x.DryPricePerQuintal);
            Decimal(e, x => x.WetPricePerQuintal); Decimal(e, x => x.MarginPerQuintal);
        });

        modelBuilder.Entity<AdminUser>(e =>
        {
            e.HasKey(x => x.Id); e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(180).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<PublicContent>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ContentKey).IsUnique();
            e.HasIndex(x => new { x.Section, x.DisplayOrder });
            e.Property(x => x.ContentKey).HasMaxLength(80).IsRequired();
            e.Property(x => x.Eyebrow).HasMaxLength(120);
            e.Property(x => x.Title).HasMaxLength(240).IsRequired();
            e.Property(x => x.Subtitle).HasMaxLength(500);
            e.Property(x => x.PrimaryCtaLabel).HasMaxLength(100);
            e.Property(x => x.PrimaryCtaUrl).HasMaxLength(500);
            e.Property(x => x.SecondaryCtaLabel).HasMaxLength(100);
            e.Property(x => x.SecondaryCtaUrl).HasMaxLength(500);
            e.Property(x => x.Icon).HasMaxLength(80);
            e.Property(x => x.ImageUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<CashRegister>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.BusinessDate).IsUnique();
            Decimal(e, x => x.OpeningBalance); Decimal(e, x => x.CountedClosingBalance);
            Decimal(e, x => x.ExpectedClosingBalance); Decimal(e, x => x.ClosingDifference);
        });

        modelBuilder.Entity<CashMovement>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OccurredAtUtc);
            e.HasOne(x => x.CashRegister).WithMany(x => x.Movements).HasForeignKey(x => x.CashRegisterId).OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Description).HasMaxLength(300).IsRequired();
            e.Property(x => x.ReferenceCode).HasMaxLength(80);
            Decimal(e, x => x.Amount);
        });

        modelBuilder.Entity<ProcessingBatch>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => new { x.Status, x.StartedAtUtc });
            Decimal(e, x => x.InputWetQuintals, 14, 4); Decimal(e, x => x.ExpectedDryYieldPercent);
            Decimal(e, x => x.OutputDryQuintals, 14, 4); Decimal(e, x => x.ActualDryYieldPercent);
            Decimal(e, x => x.LossPercent); Decimal(e, x => x.InputUnitCost); Decimal(e, x => x.OutputUnitCost);
        });

        modelBuilder.Entity<InventoryLot>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => new { x.Variety, x.State, x.Status, x.ReceivedAtUtc });
            e.HasOne(x => x.Purchase).WithMany().HasForeignKey(x => x.PurchaseId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ProcessingBatch).WithMany().HasForeignKey(x => x.ProcessingBatchId).OnDelete(DeleteBehavior.Restrict);
            Decimal(e, x => x.InitialQuantityQuintals, 14, 4);
            Decimal(e, x => x.AvailableQuantityQuintals, 14, 4);
            Decimal(e, x => x.UnitCost);
            Decimal(e, x => x.HumidityPercent);
        });

        modelBuilder.Entity<SaleLotAllocation>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.SaleId, x.InventoryLotId }).IsUnique();
            e.HasOne(x => x.Sale).WithMany(x => x.LotAllocations).HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.InventoryLot).WithMany(x => x.SaleAllocations).HasForeignKey(x => x.InventoryLotId).OnDelete(DeleteBehavior.Restrict);
            Decimal(e, x => x.QuantityQuintals, 14, 4);
            Decimal(e, x => x.UnitCost);
        });

        modelBuilder.Entity<ProcessingLotAllocation>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ProcessingBatchId, x.InventoryLotId }).IsUnique();
            e.HasOne(x => x.ProcessingBatch).WithMany(x => x.LotAllocations).HasForeignKey(x => x.ProcessingBatchId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.InventoryLot).WithMany(x => x.ProcessingAllocations).HasForeignKey(x => x.InventoryLotId).OnDelete(DeleteBehavior.Restrict);
            Decimal(e, x => x.QuantityQuintals, 14, 4);
            Decimal(e, x => x.UnitCost);
        });
    }

    private static void Decimal<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity,
        System.Linq.Expressions.Expression<Func<TEntity, decimal>> property, int precision = 14, int scale = 2) where TEntity : class
        => entity.Property(property).HasPrecision(precision, scale);

    private static void Decimal<TEntity>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity> entity,
        System.Linq.Expressions.Expression<Func<TEntity, decimal?>> property, int precision = 14, int scale = 2) where TEntity : class
        => entity.Property(property).HasPrecision(precision, scale);
}
