using Backend.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Contexts;

/// <summary>
/// fullness_ecデータベースへのアクセスを提供するDbContext
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="options">DbContextの構成オプション</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

    /// <summary>
    /// 部署
    /// </summary>
    public DbSet<Department> Departments => Set<Department>();

    /// <summary>
    /// 社員
    /// </summary>
    public DbSet<Employee> Employees => Set<Employee>();

    /// <summary>
    /// 社員アカウント
    /// </summary>
    public DbSet<EmployeeAccount> EmployeeAccounts => Set<EmployeeAccount>();

    /// <summary>
    /// 商品カテゴリ
    /// </summary>
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    /// <summary>
    /// 商品
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// 商品在庫
    /// </summary>
    public DbSet<ProductStock> ProductStocks => Set<ProductStock>();

    /// <summary>
    /// 注文ステータス
    /// </summary>
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();

    /// <summary>
    /// 支払い方法
    /// </summary>
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    /// <summary>
    /// 顧客
    /// </summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>
    /// 注文
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// 注文明細
    /// </summary>
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    /// <summary>
    /// モデル構築時のマッピング設定（リレーション・制約など）
    /// </summary>
    /// <param name="modelBuilder">モデルビルダー</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- 部署(department) ----
        modelBuilder.Entity<Department>(entity =>
        {
            // 部署識別ID: 必須・一意・既定値 gen_random_uuid()
            entity.Property(e => e.DepartmentUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.DepartmentUuid).IsUnique();

            // 部署名: 必須・最大100文字
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            // 部署と社員 1:N（削除は制限：社員が居る部署は削除不可）
            entity.HasMany(e => e.Employees).WithOne(e => e.Department).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- 社員(employee) ----
        modelBuilder.Entity<Employee>(entity =>
        {
            // 社員識別ID: 必須・一意・既定値 gen_random_uuid()
            entity.Property(e => e.EmployeeUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.EmployeeUuid).IsUnique();

            // 社員名: 必須・最大100文字
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            // 社員名カナ: 任意・最大100文字
            entity.Property(e => e.NameKana).HasMaxLength(100);
            // 部署とのリレーション … Department 側で設定済み
            // アカウントとの1対1 … EmployeeAccount 側で設定する
        });

        // ---- 社員アカウント(employee_account) ----
        modelBuilder.Entity<EmployeeAccount>(entity =>
        {
            // アカウント識別ID: 必須・一意・既定値 gen_random_uuid()
            entity.Property(e => e.AccountUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.AccountUuid).IsUnique();

            // アカウント名: 必須・最大20文字・一意
            entity.Property(e => e.Name).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Name).IsUnique();

            // パスワード(ハッシュ値): 必須・最大255文字
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);

            // 社員 1 -- 0..1 アカウント（FKは employee_id、削除は制限）
            entity.HasOne(e => e.Employee).WithOne(e => e.Account).HasForeignKey<EmployeeAccount>(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

         // ---- 商品カテゴリ(product_category) ----
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            // 商品カテゴリ識別ID: 必須・一意・既定値 gen_random_uuid()
            entity.Property(e => e.CategoryUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.CategoryUuid).IsUnique();

            // 商品カテゴリ名: 必須・最大30文字
            entity.Property(e => e.Name).IsRequired().HasMaxLength(30);

            // 商品カテゴリ 1 -- * 商品（削除は制限：商品が属するカテゴリは削除不可）
            entity.HasMany(e => e.Products).WithOne(e => e.Category).HasForeignKey(e => e.ProductCategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- 商品(product) ----
        modelBuilder.Entity<Product>(entity =>
        {
            // 商品識別ID: 必須・一意・既定値 gen_random_uuid()
            entity.Property(e => e.ProductUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.ProductUuid).IsUnique();

            // 商品名: 必須・最大100文字
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            // 価格: 必須
            entity.Property(e => e.Price).IsRequired();

            // 画像URL: 任意・最大200文字
            entity.Property(e => e.ImageUrl).HasMaxLength(200);

            // 削除フラグ: 必須・既定値 0
            entity.Property(e => e.DeleteFlg).IsRequired().HasDefaultValue(0);

            // カテゴリとのリレーション … ProductCategory 側で設定済み
            // 在庫との1対1 … ProductStock 側で設定する
        });

        // ---- 商品在庫(product_stock) ----
        modelBuilder.Entity<ProductStock>(entity =>
        {
            // 商品在庫識別ID: 必須・一意・既定値 gen_random_uuid()
            entity.Property(e => e.StockUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.StockUuid).IsUnique();

            // 在庫数: 必須・既定値 0
            entity.Property(e => e.Quantity).IsRequired().HasDefaultValue(0);

            // 商品 1 -- 0..1 在庫（FKは product_id）
            entity.HasOne(e => e.Product).WithOne(e => e.Stock).HasForeignKey<ProductStock>(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        // ---- 注文ステータス(order_status) ----
        modelBuilder.Entity<OrderStatus>(entity =>
        {
            // 注文ステータス名: 必須・最大100文字
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            // 注文ステータス 1 -- * 注文（削除は制限：使用中のステータスは削除不可）
            entity.HasMany(e => e.Orders).WithOne(e => e.OrderStatus).HasForeignKey(e => e.OrderStatusId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- 支払い方法(payment_method) ----
        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            // 支払い方法名: 必須・最大100文字
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            // 支払い方法 1 -- * 注文（削除は制限：使用中の支払い方法は削除不可）
            entity.HasMany(e => e.Orders).WithOne(e => e.PaymentMethod).HasForeignKey(e => e.PaymentMethodId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- 顧客(customer) ----
        modelBuilder.Entity<Customer>(entity =>
        {
            // 顧客識別ID: 必須・一意・既定値 gen_random_uuid()
            entity.Property(e => e.CustomerUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.CustomerUuid).IsUnique();

            // 顧客名: 必須・最大20文字
            entity.Property(e => e.Name).IsRequired().HasMaxLength(20);

            // 顧客名カナ: 任意・最大20文字
            entity.Property(e => e.NameKana).HasMaxLength(20);

            // 住所1: 必須・最大100文字
            entity.Property(e => e.Address1).IsRequired().HasMaxLength(100);

            // 住所2: 任意・最大100文字
            entity.Property(e => e.Address2).HasMaxLength(100);

            // 電話番号: 必須・最大20文字
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);

            // メールアドレス: 必須・最大200文字・一意
            entity.Property(e => e.MailAddress).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.MailAddress).IsUnique();

            // アカウント名: 必須・最大30文字・一意
            entity.Property(e => e.Username).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.Username).IsUnique();

            // パスワード(ハッシュ値): 必須・最大255文字
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);

            // 登録日: timestamp(タイムゾーンなし)・既定値 CURRENT_TIMESTAMP
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            // 顧客 1 -- * 注文（削除は制限）
            entity.HasMany(e => e.Orders).WithOne(e => e.Customer).HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- 注文(orders) ----
        modelBuilder.Entity<Order>(entity =>
        {
            // 注文識別ID: 必須・一意・既定値 gen_random_uuid()
            entity.Property(e => e.OrderUuid).IsRequired().HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.OrderUuid).IsUnique();

             // 注文日: timestamp(タイムゾーンなし)・既定値 CURRENT_TIMESTAMP
            entity.Property(e => e.OrderDate).HasColumnType("timestamp without time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 合計金額: 必須
            entity.Property(e => e.AmountTotal).IsRequired();

            // 顧客・ステータス・支払い方法とのリレーション … 各 principal 側で設定済み
            // 注文明細との1対多 … OrderDetail 側で設定する
        });

        // ---- 注文明細(orders_detail) ----
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            // 注文数: 必須
            entity.Property(e => e.Count).IsRequired();

            // 注文 1 -- * 注文明細（削除は連鎖：注文が消えたら明細も削除）
            entity.HasOne(e => e.Order).WithMany(e => e.OrderDetails).HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Cascade);

            // 商品 1 -- * 注文明細（削除は制限：明細で使用中の商品は削除不可）
            entity.HasOne(e => e.Product).WithMany(e => e.OrderDetails).HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
    }   
}