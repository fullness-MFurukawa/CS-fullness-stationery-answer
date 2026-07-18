namespace Ec.Domain.Adapters;

/// <summary>
/// ソースからドメインの集約(Aggregate)を組み立てるファクトリ
/// </summary>
/// <typeparam name="TSource">集約の組み立てに用いるソースの型</typeparam>
/// <typeparam name="TAggregate">組み立てるドメイン集約の型</typeparam>
public interface IAggregateFactory<in TSource, out TAggregate>
{
    /// <summary>
    /// ソースからドメイン集約を組み立てる
    /// </summary>
    /// <param name="source">集約の組み立てに用いるソース</param>
    /// <returns>組み立てたドメイン集約</returns>
    TAggregate Create(TSource source);
}