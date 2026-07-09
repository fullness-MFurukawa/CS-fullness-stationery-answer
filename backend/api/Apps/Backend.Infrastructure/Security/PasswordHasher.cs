using Backend.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using IdentityPasswordHasher = Microsoft.AspNetCore.Identity.PasswordHasher<object>;

namespace Backend.Infrastructure.Security;

/// <summary>
/// ASP.NET Core IdentityのPasswordHasherを使用したパスワードのハッシュ化と照合
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Identity のハッシュ化実装（PBKDF2 / v3形式）
    /// </summary>
    private readonly IdentityPasswordHasher _hasher = new();

    /// <summary>
    /// ハッシュ化にユーザー情報を使用しないため渡すダミーインスタンス
    /// </summary>
    private static readonly object DummyUser = new();

    /// <summary>
    /// 平文のパスワードをハッシュ化する
    /// </summary>
    /// <param name="password">平文のパスワード</param>
    /// <returns>ハッシュ化されたパスワード</returns>
    public string HashPassword(string password)
        => _hasher.HashPassword(DummyUser, password);

    /// <summary>
    /// 平文のパスワードがハッシュ値と一致するかを検証する
    /// </summary>
    /// <param name="hashedPassword">保存されているハッシュ化されたパスワード</param>
    /// <param name="password">照合する平文のパスワード</param>
    /// <returns>一致する場合はtrue</returns>
    public bool VerifyPassword(string hashedPassword, string password)
    {
        var result = _hasher.VerifyHashedPassword(DummyUser, hashedPassword, password);

        // 再ハッシュ推奨（設定変更等）でも認証としては成功とみなす
        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}