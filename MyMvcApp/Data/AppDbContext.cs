using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyMvcApp.Data;

namespace MyMvcApp.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Motorcycle> Motorcycles => Set<Motorcycle>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<MotorcycleOrder> Orders => Set<MotorcycleOrder>();
    public DbSet<MotorcycleOrderItem> OrderItems => Set<MotorcycleOrderItem>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<MotorcycleReview> Reviews => Set<MotorcycleReview>();
    public DbSet<StoreSetting> StoreSettings => Set<StoreSetting>();
    public DbSet<ProductContactRequest> ProductContactRequests => Set<ProductContactRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Motorcycle>(entity =>
        {
            entity.ToTable("XeMays", table =>
            {
                table.ExcludeFromMigrations();
                table.UseSqlOutputClause(false);
            });
            entity.HasKey(motorcycle => motorcycle.Id);
            entity.HasIndex(motorcycle => motorcycle.Code).HasDatabaseName("IX_XeMays_MaXe").IsUnique();
            entity.Property(motorcycle => motorcycle.Code).HasColumnName("MaXe").HasMaxLength(32);
            entity.Property(motorcycle => motorcycle.Name).HasColumnName("TenXe").HasMaxLength(160);
            entity.Property(motorcycle => motorcycle.Brand).HasColumnName("HangXe").HasMaxLength(80);
            entity.Property(motorcycle => motorcycle.Type).HasColumnName("LoaiXe").HasMaxLength(80);
            entity.Property(motorcycle => motorcycle.Price)
                .HasColumnName("GiaBan")
                .HasConversion(
                    price => (decimal)price,
                    price => decimal.ToInt64(decimal.Round(price, 0, MidpointRounding.AwayFromZero)))
                .HasPrecision(18, 2);
            entity.Property<decimal>("GiaNiemYet").HasPrecision(18, 2);
            entity.Property(motorcycle => motorcycle.Engine).HasColumnName("MoTa").HasMaxLength(4000);
            entity.Property(motorcycle => motorcycle.Image).HasColumnName("HinhAnh").HasMaxLength(1000);
            entity.Property(motorcycle => motorcycle.StockQuantity).HasColumnName("SoLuong");
            entity.Property(motorcycle => motorcycle.IsActive).HasColumnName("DangKinhDoanh");
            entity.Ignore(motorcycle => motorcycle.Power);
            entity.Ignore(motorcycle => motorcycle.Gear);
            entity.Ignore(motorcycle => motorcycle.IsHiddenByStock);
            entity.Ignore(motorcycle => motorcycle.AvailabilityStatus);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasIndex(customer => customer.Email).IsUnique();
            entity.Property(customer => customer.Email).HasMaxLength(320);
        });

        modelBuilder.Entity<MotorcycleOrder>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasIndex(order => order.OrderNumber).IsUnique();
            entity.Property(order => order.OrderNumber).HasMaxLength(64);
            entity.Property(order => order.PromotionCode).HasMaxLength(32);
            entity.HasOne(order => order.Customer)
                .WithMany(customer => customer.Orders)
                .HasForeignKey(order => order.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MotorcycleOrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasOne(item => item.Order)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Motorcycle>()
                .WithMany()
                .HasForeignKey(item => item.MotorcycleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.ToTable("Promotions");
            entity.HasIndex(promotion => promotion.Code).IsUnique();
            entity.Property(promotion => promotion.Code).HasMaxLength(32);
        });

        modelBuilder.Entity<StaffMember>(entity =>
        {
            entity.ToTable("StaffMembers");
            entity.HasIndex(member => member.Email).IsUnique();
            entity.Property(member => member.Email).HasMaxLength(320);
        });

        modelBuilder.Entity<StoreSetting>(entity =>
        {
            entity.ToTable("StoreSettings");
            entity.HasKey(setting => setting.Key);
        });

        modelBuilder.Entity<MotorcycleReview>(entity =>
        {
            entity.ToTable("Reviews");
            entity.Property(review => review.ReviewerEmail).HasMaxLength(320);
            entity.HasOne(review => review.Motorcycle)
                .WithMany()
                .HasForeignKey(review => review.MotorcycleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductContactRequest>(entity =>
        {
            entity.ToTable("ProductContactRequests");
            entity.Property(request => request.FullName).HasMaxLength(120);
            entity.Property(request => request.Phone).HasMaxLength(30);
            entity.Property(request => request.Email).HasMaxLength(320);
            entity.Property(request => request.Note).HasMaxLength(1000);
            entity.HasOne(request => request.Motorcycle)
                .WithMany()
                .HasForeignKey(request => request.MotorcycleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(request => new { request.IsContacted, request.CreatedAtUtc });
        });
    }
}
