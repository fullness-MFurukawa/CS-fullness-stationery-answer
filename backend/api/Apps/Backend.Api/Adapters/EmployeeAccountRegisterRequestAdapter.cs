using Backend.Api.ViewModels.Requests;
using Backend.Application.Params;
using Backend.Domain.Adapters;

namespace Backend.Api.Adapters;

/// <summary>
/// 担当者アカウント登録のリクエストをユースケースの入力値へ変換するアダプタ
/// </summary>
/// <remarks>
/// <see cref="IEntityAdapter{TSource, TDomain}"/> は本来ドメインオブジェクトとの双方向変換を表す。
/// ここでは <see cref="EmployeeAccountRegisterParam"/> をドメイン側の型として扱っているため、
/// 逆方向（入力値からリクエストへの復元）は用途が存在せず未サポートとする。
/// 全機能の実装後、片方向のアダプタへ切り出すことを検討する。
/// </remarks>
public class EmployeeAccountRegisterRequestAdapter
    : IEntityAdapter<EmployeeAccountRegisterRequest, EmployeeAccountRegisterParam>
{
    /// <summary>
    /// リクエストからユースケースの入力値へ変換する
    /// </summary>
    /// <param name="source">担当者アカウント登録のリクエスト</param>
    /// <returns>担当者アカウント登録の入力値</returns>
    public EmployeeAccountRegisterParam ToDomain(EmployeeAccountRegisterRequest source)
        => new(
            source.EmployeeId!.Value,
            source.AccountName,
            source.Password);

    /// <summary>
    /// ユースケースの入力値からリクエストへ変換する（未サポート）
    /// </summary>
    /// <param name="domain">担当者アカウント登録の入力値</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">この方向の変換は使用しない</exception>
    public EmployeeAccountRegisterRequest ToSource(EmployeeAccountRegisterParam domain)
        => throw new NotSupportedException("入力値からリクエストへの変換は使用しません。");
}