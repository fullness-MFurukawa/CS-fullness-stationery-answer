using System.Security.Claims;
using Ec.Application.Results;
namespace Ec.Application.Interfaces;

/// <summary>
/// クレームからアクセストークンを生成する
/// </summary>
public interface IAccessTokenGenerator
{
    /// <summary>
    /// 指定したクレームを含むアクセストークンを生成する
    /// </summary>
    /// <param name="claims">トークンに含める利用者のクレーム</param>
    /// <returns>生成されたアクセストークン</returns>
    AccessToken Generate(IEnumerable<Claim> claims);
}