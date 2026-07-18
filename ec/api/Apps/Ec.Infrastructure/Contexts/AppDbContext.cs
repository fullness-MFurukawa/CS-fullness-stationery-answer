using Ec.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
namespace Ec.Infrastructure.Contexts;

/// <summary>
/// fullness_ecデータベースへのアクセスを提供するDbContext
/// </summary>
/// <remarks>
/// EC側は顧客向け機能のみを扱うため、社員・部署・社員アカウントのテーブルは持たない。
/// これらは管理サービス側の関心である。
/// </remarks>
public class AppDbContext : DbContext
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="options">DbContextの構成オプション</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>商品カテゴリ</summary>
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    /// <summary>商品</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>商品在庫</summary>
    public DbSet<ProductStock> ProductStocks => Set<ProductStock>();

    /// <summary>注文ステータス</summary>
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();

    /// <summary>支払い方法</summary>
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    /// <summary>顧客</summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>注文</summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>注文明細</summary>
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    /// <summary>
    /// モデル構築時のマッピング設定（リレーション・制約など）
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- 商品カテゴリ(product_category) ----
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.Property(e => e.CategoryUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.CategoryUuid).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(30);
            entity.HasMany(e => e.Products).WithOne(e => e.Category).HasForeignKey(e => e.ProductCategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- 商品(product) ----
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ProductUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.ProductUuid).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(200);
            entity.Property(e => e.DeleteFlg).IsRequired().HasDefaultValue(0);
        });

        // ---- 商品在庫(product_stock) ----
        modelBuilder.Entity<ProductStock>(entity =>
        {
            entity.Property(e => e.StockUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.StockUuid).IsUnique();
            entity.Property(e => e.Quantity).IsRequired().HasDefaultValue(0);
            entity.HasOne(e => e.Product).WithOne(e => e.Stock).HasForeignKey<ProductStock>(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        // ---- 注文ステータス(order_status) ----
        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasMany(e => e.Orders).WithOne(e => e.OrderStatus).HasForeignKey(e => e.OrderStatusId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- 支払い方法(payment_method) ----
        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasMany(e => e.Orders).WithOne(e => e.PaymentMethod).HasForeignKey(e => e.PaymentMethodId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- 顧客(customer) ----
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.CustomerUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.CustomerUuid).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(20);
            entity.Property(e => e.NameKana).HasMaxLength(20);
            entity.Property(e => e.Address1).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Address2).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.MailAddress).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.MailAddress).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasMany(e => e.Orders).WithOne(e => e.Customer).HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- 注文(orders) ----
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.OrderUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.OrderUuid).IsUnique();
            entity.Property(e => e.OrderDate).HasColumnType("timestamp without time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.AmountTotal).IsRequired();
        });

        // ---- 注文明細(orders_detail) ----
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.Property(e => e.Count).IsRequired();
            entity.HasOne(e => e.Order).WithMany(e => e.OrderDetails).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product).WithMany(e => e.OrderDetails).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}