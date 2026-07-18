using Ec.Domain.Models;
namespace Ec.Domain.Repositories;

/// <summary>
/// 注文(注文集約)の永続化を担うリポジトリ
/// </summary>
/// <remarks>
/// 管理サービス側と異なり、EC側は注文を生成する。
/// また、参照できるのは自身の注文のみである（UC007）。
/// 管理側のような全件検索は行わない。
/// </remarks>
public interface IOrderRepository
{
    /// <summary>
    /// 注文を新規登録(注文明細を含む注文集約を登録)
    /// </summary>
    /// <param name="order">登録する注文</param>
    /// <remarks>
    /// 在庫の更新と同一のトランザクションで実行しなければならない。
    /// 注文だけが登録されて在庫が減らない状態や、その逆を許してはならない。
    /// </remarks>
    Task AddAsync(Order order);

    /// <summary>
    /// 顧客を指定して注文履歴を取得(UC007)
    /// </summary>
    /// <param name="customerId">顧客識別ID(uuid)</param>
    /// <returns>該当する顧客の注文一覧（注文日時の降順）</returns>
    /// <remarks>
    /// 他人の購入履歴を参照できてはならないため、
    /// 顧客識別IDによる絞り込みは呼び出し側の任意ではなく必須とする。
    /// 引数をnull許容にしないことで、絞り込みの省略を型で防ぐ。
    /// </remarks>
    Task<IReadOnlyList<Order>> FindByCustomerAsync(Guid customerId);

    /// <summary>
    /// 識別IDと顧客を指定して注文を取得(UC007)
    /// </summary>
    /// <param name="id">注文識別ID(uuid)</param>
    /// <param name="customerId">顧客識別ID(uuid)</param>
    /// <returns>該当する注文。存在しない場合、または他の顧客の注文である場合はnull</returns>
    /// <remarks>
    /// 購入履歴詳細（FP012）で用いる。
    /// 「自分の注文しか閲覧できない」ことはUC007の制約そのものであるため、
    /// 顧客識別IDを引数に含めて型で表現する。
    /// 注文識別IDのみを受け取る形にすると、
    /// 呼び出し側が所有者の確認を書き忘れたときに、
    /// 識別IDを推測して他人の注文を閲覧されうる。
    ///
    /// 管理サービス側の同名メソッドは注文識別IDのみを受け取るが、
    /// 管理者はすべての注文を閲覧できる立場であり、制約が異なる。
    /// </remarks>
    Task<Order?> FindByIdAsync(Guid id, Guid customerId);
}