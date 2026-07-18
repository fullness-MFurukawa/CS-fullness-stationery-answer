namespace Ec.Application.Interfaces;

/// <summary>
/// パスワードのハッシュ化と照合を行う
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// 平文のパスワードをハッシュ化する
    /// </summary>
    /// <param name="password">平文のパスワード</param>
    /// <returns>ハッシュ化されたパスワード</returns>
    string HashPassword(string password);

    /// <summary>
    /// 平文のパスワードがハッシュ値と一致するかを検証する
    /// </summary>
    /// <param name="hashedPassword">保存されているハッシュ化されたパスワード</param>
    /// <param name="password">照合する平文のパスワード</param>
    /// <returns>一致する場合はtrue</returns>
    bool VerifyPassword(string hashedPassword, string password);
}