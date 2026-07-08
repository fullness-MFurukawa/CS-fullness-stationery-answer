namespace Backend.Domain.Adapters;
/// <summary>
/// 外部モデル(EF Coreエンティティ・ViewModel等)とドメインエンティティを相互変換するアダプタ
/// </summary>
/// <typeparam name="TSource">変換対象となる外部モデルの型</typeparam>
/// <typeparam name="TDomain">ドメインエンティティの型</typeparam>
public interface IEntityAdapter<TSource, TDomain>
{
    /// <summary>
    /// 外部モデルからドメインエンティティへ変換
    /// </summary>
    /// <param name="source">変換元の外部モデル</param>
    /// <returns>ドメインエンティティ</returns>
    TDomain ToDomain(TSource source);

    /// <summary>
    /// ドメインエンティティから外部モデルへ変換
    /// </summary>
    /// <param name="domain">変換元のドメインエンティティ</param>
    /// <returns>外部モデル</returns>
    TSource ToSource(TDomain domain);
}