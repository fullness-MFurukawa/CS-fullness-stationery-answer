using Backend.Api.ViewModels.Responses;
using Backend.Domain.Adapters;
using Backend.Domain.Models;

namespace Backend.Api.Adapters;

/// <summary>
/// 社員のドメインオブジェクトとレスポンスを変換するアダプタ
/// </summary>
/// <remarks>
/// レスポンスは部署名のみを持ち、部署の識別IDを含まないため復元できない。
/// </remarks>
public class EmployeeResponseAdapter : IEntityAdapter<EmployeeResponse, Employee>
{
    /// <summary>
    /// レスポンスからドメインオブジェクトへ変換する（未サポート）
    /// </summary>
    /// <param name="source">社員のレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">部署の識別IDが失われるため復元できない</exception>
    public Employee ToDomain(EmployeeResponse source)
        => throw new NotSupportedException("レスポンスから社員への復元は行えません。");

    /// <summary>
    /// ドメインオブジェクトからレスポンスへ変換する
    /// </summary>
    /// <param name="domain">ドメインの社員</param>
    /// <returns>社員のレスポンス</returns>
    public EmployeeResponse ToSource(Employee domain)
        => new(
            domain.Id,
            domain.Name,
            domain.NameKana,
            domain.Department.Name);
}