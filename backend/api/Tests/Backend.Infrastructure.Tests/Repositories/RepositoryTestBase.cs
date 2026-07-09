using Backend.Application.Interfaces;
using Backend.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Backend.Infrastructure.Tests.Repositories;

/// <summary>
/// 実データベースを使用するリポジトリテストの基底クラス
/// テストごとにトランザクションを開始し、終了時に必ずロールバックする
/// </summary>
public abstract class RepositoryTestBase
{
    /// <summary>
    /// テスト対象のリポジトリに渡すデータベースコンテキスト
    /// </summary>
    protected AppDbContext Context { get; private set; } = null!;

    /// <summary>
    /// トランザクション制御に使用するユニットオブワーク
    /// </summary>
    protected IUnitOfWork UnitOfWork { get; private set; } = null!;

    /// <summary>
    /// appsettings.Test.json から接続文字列を取得する
    /// </summary>
    /// <returns>データベース接続文字列</returns>
    private static string GetConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Test.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        return configuration.GetConnectionString("FullnessEc")
            ?? throw new InvalidOperationException("接続文字列 'FullnessEc' が設定されていません。");
    }

    /// <summary>
    /// テスト開始時にコンテキストを生成しトランザクションを開始する
    /// </summary>
    [TestInitialize]
    public async Task SetUpAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(GetConnectionString())
            .Options;

        Context = new AppDbContext(options);
        UnitOfWork = new UnitOfWork(Context);

        await UnitOfWork.BeginTransactionAsync();
    }

    /// <summary>
    /// テスト終了時に必ずロールバックしてリソースを解放する
    /// </summary>
    [TestCleanup]
    public async Task TearDownAsync()
    {
        try
        {
            await UnitOfWork.RollbackAsync();
        }
        finally
        {
            await Context.DisposeAsync();
        }
    }
}