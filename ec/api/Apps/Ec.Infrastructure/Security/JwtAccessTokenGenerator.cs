using System.Security.Claims;
using System.Text;

using Ec.Application.Interfaces;
using Ec.Application.Results;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Ec.Infrastructure.Security;

/// <summary>
/// JWTを用いたアクセストークンの生成
/// </summary>
public class JwtAccessTokenGenerator : IAccessTokenGenerator
{
    /// <summary>
    /// HMAC-SHA256に必要な署名鍵の最小バイト数
    /// </summary>
    private const int MinimumSigningKeyBytes = 32;

    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="options">JWTの構成情報</param>
    /// <exception cref="InvalidOperationException">署名鍵が未設定、または長さが不足している場合</exception>
    public JwtAccessTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        if (Encoding.UTF8.GetByteCount(_options.SigningKey) < MinimumSigningKeyBytes)
        {
            throw new InvalidOperationException(
                $"JWTの署名鍵は{MinimumSigningKeyBytes}バイト以上で設定してください。");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    /// <summary>
    /// 指定したクレームを含むアクセストークンを生成する
    /// </summary>
    /// <param name="claims">トークンに含める利用者のクレーム</param>
    /// <returns>生成されたアクセストークン</returns>
    public AccessToken Generate(IEnumerable<Claim> claims)
    {
        var issuedAt = DateTimeOffset.UtcNow;
        var expiresAt = issuedAt.AddMinutes(_options.ExpiresMinutes);

        // 業務クレームに加え、トークンを一意に識別する jti を付与する
        var tokenClaims = claims.ToList();
        tokenClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(tokenClaims),
            IssuedAt = issuedAt.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = _signingCredentials
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(descriptor);

        return new AccessToken(token, expiresAt);
    }
}