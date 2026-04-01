using MedicalStockManager.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalStockManager.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Service> Services => Set<Service>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<InventorySession> InventorySessions => Set<InventorySession>();
    public DbSet<InventoryLine> InventoryLines => Set<InventoryLine>();
    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
    public DbSet<PurchaseRequestLine> PurchaseRequestLines => Set<PurchaseRequestLine>();
    public DbSet<MaterialRequest> MaterialRequests => Set<MaterialRequest>();
    public DbSet<MaterialRequestLine> MaterialRequestLines => Set<MaterialRequestLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("Services");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(s => s.Name).IsUnique();
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Locations");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(l => l.Name).IsUnique();
        });

        modelBuilder.Entity<StockBatch>(entity =>
        {
            entity.ToTable("StockBatches");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.BatchNumber).HasMaxLength(80).IsRequired();

            entity.HasOne(b => b.StockItem)
                .WithMany()
                .HasForeignKey(b => b.StockItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(b => b.Location)
                .WithMany(l => l.Batches)
                .HasForeignKey(b => b.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.ToTable("StockItems");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(150).IsRequired();
            entity.Property(item => item.Reference).HasMaxLength(50).IsRequired();
            entity.Property(item => item.Unit).HasMaxLength(30).IsRequired();
            entity.HasIndex(item => item.Reference).IsUnique();

            entity.HasOne(item => item.Service)
                .WithMany(s => s.StockItems)
                .HasForeignKey(item => item.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUsers");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Username).HasMaxLength(80).IsRequired();
            entity.Property(user => user.FullName).HasMaxLength(150).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(80).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(user => user.Role).HasMaxLength(40).IsRequired();
            entity.HasIndex(user => user.Username).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(log => log.Id);
            entity.Property(log => log.Username).HasMaxLength(80).IsRequired();
            entity.Property(log => log.Action).HasMaxLength(120).IsRequired();
            entity.Property(log => log.EntityType).HasMaxLength(120).IsRequired();
            entity.Property(log => log.EntityId).HasMaxLength(120);
            entity.Property(log => log.Details).HasMaxLength(300);
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("StockMovements");
            entity.HasKey(movement => movement.Id);
            entity.Property(movement => movement.Notes).HasMaxLength(300);
            entity.Property(movement => movement.BatchNumber).HasMaxLength(80);

            entity.HasOne(movement => movement.StockItem)
                .WithMany(item => item.Movements)
                .HasForeignKey(movement => movement.StockItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(movement => movement.StockBatch)
                .WithMany()
                .HasForeignKey(movement => movement.StockBatchId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(movement => movement.SourceLocation)
                .WithMany()
                .HasForeignKey(movement => movement.SourceLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(movement => movement.DestinationLocation)
                .WithMany()
                .HasForeignKey(movement => movement.DestinationLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("Suppliers");
            entity.HasKey(supplier => supplier.Id);
            entity.Property(supplier => supplier.Name).HasMaxLength(150).IsRequired();
            entity.Property(supplier => supplier.ContactName).HasMaxLength(120);
            entity.Property(supplier => supplier.Phone).HasMaxLength(40);
            entity.Property(supplier => supplier.Email).HasMaxLength(120);
            entity.Property(supplier => supplier.Address).HasMaxLength(250);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.ToTable("PurchaseOrders");
            entity.HasKey(order => order.Id);
            entity.Property(order => order.OrderNumber).HasMaxLength(50).IsRequired();
            entity.Property(order => order.Notes).HasMaxLength(300);
            entity.HasIndex(order => order.OrderNumber).IsUnique();

            entity.HasOne(order => order.Supplier)
                .WithMany(supplier => supplier.PurchaseOrders)
                .HasForeignKey(order => order.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.ToTable("PurchaseOrderLines");
            entity.HasKey(line => line.Id);
            entity.Property(line => line.UnitPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(line => line.PurchaseOrder)
                .WithMany(order => order.Lines)
                .HasForeignKey(line => line.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(line => line.StockItem)
                .WithMany(item => item.PurchaseOrderLines)
                .HasForeignKey(line => line.StockItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseRequest>(entity =>
        {
            entity.ToTable("PurchaseRequests");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.RequestNumber).HasMaxLength(60).IsRequired();
            entity.Property(r => r.RequestedBy).HasMaxLength(80).IsRequired();
            entity.Property(r => r.Justification).HasMaxLength(300);
            entity.Property(r => r.ProcessedBy).HasMaxLength(80);
            entity.Property(r => r.RejectionReason).HasMaxLength(300);
            entity.HasIndex(r => r.RequestNumber).IsUnique();

            entity.HasOne(r => r.LinkedOrder)
                .WithMany()
                .HasForeignKey(r => r.LinkedOrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PurchaseRequestLine>(entity =>
        {
            entity.ToTable("PurchaseRequestLines");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.ItemLabel).HasMaxLength(150).IsRequired();
            entity.Property(l => l.Unit).HasMaxLength(30);
            entity.Property(l => l.Notes).HasMaxLength(200);
            entity.Property(l => l.EstimatedUnitPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(l => l.PurchaseRequest)
                .WithMany(r => r.Lines)
                .HasForeignKey(l => l.PurchaseRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.StockItem)
                .WithMany()
                .HasForeignKey(l => l.StockItemId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InventorySession>(entity =>
        {
            entity.ToTable("InventorySessions");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Title).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Notes).HasMaxLength(300);
            entity.Property(s => s.CreatedBy).HasMaxLength(80);
        });

        modelBuilder.Entity<InventoryLine>(entity =>
        {
            entity.ToTable("InventoryLines");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Notes).HasMaxLength(200);

            entity.HasOne(l => l.InventorySession)
                .WithMany(s => s.Lines)
                .HasForeignKey(l => l.InventorySessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.StockItem)
                .WithMany()
                .HasForeignKey(l => l.StockItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MaterialRequest>(entity =>
        {
            entity.ToTable("MaterialRequests");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.RequestNumber).HasMaxLength(60).IsRequired();
            entity.Property(r => r.RequestedByUsername).HasMaxLength(80).IsRequired();
            entity.HasIndex(r => r.RequestNumber).IsUnique();

            entity.HasOne(r => r.RequestingService)
                .WithMany()
                .HasForeignKey(r => r.RequestingServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MaterialRequestLine>(entity =>
        {
            entity.ToTable("MaterialRequestLines");
            entity.HasKey(l => l.Id);
            
            entity.HasOne(l => l.MaterialRequest)
                .WithMany(r => r.Lines)
                .HasForeignKey(l => l.MaterialRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.StockItem)
                .WithMany()
                .HasForeignKey(l => l.StockItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
