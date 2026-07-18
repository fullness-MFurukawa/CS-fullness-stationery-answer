namespace Ec.Application.Params;

/// <summary>
/// 商品検索の入力値（UC003）
/// </summary>
/// <param name="CategoryId">絞り込むカテゴリの識別ID。未指定（null）の場合は全商品を対象とする</param>
public sealed record ProductSearchParam(Guid? CategoryId);