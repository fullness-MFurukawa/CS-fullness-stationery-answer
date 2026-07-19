using System.Security.Claims;
using Ec.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
namespace Ec.Api.Controllers;

/// <summary>
/// 認証済みの顧客を扱うコントローラの基底クラス
/// </summary>
/// <remarks>
/// トークンのsubクレームから顧客識別IDを取り出す処理を1か所へまとめる。
/// UC005・UC007で顧客IDが必要になるが、各アクションで取り出すと重複するため基底に置く。
/// 顧客IDをフロントエンドから送らせると他人のIDを詐称できるため、
/// 必ず認証済みのトークンから取得する。
/// </remarks>
public abstract class EcControllerBase : ControllerBase
{
    /// <summary>
    /// 認証済みの顧客の識別ID(uuid)を取得する
    /// </summary>
    /// <returns>顧客識別ID(uuid)</returns>
    /// <exception cref="AuthenticationFailedException">subクレームが存在しない、またはUUIDとして解釈できない場合</exception>
    /// <remarks>
    /// [Authorize]を付けたアクションでのみ呼ぶ。認証を通っていれば通常subは必ず存在するが、
    /// 万一欠落・不正な場合は認証失敗として扱う。
    /// </remarks>
    protected Guid GetCurrentCustomerId()
    {
        var sub = User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var customerId))
        {
            throw new AuthenticationFailedException("認証情報から顧客を特定できません。");
        }
        return customerId;
    }
}