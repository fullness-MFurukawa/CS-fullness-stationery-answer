using Backend.Domain.Exceptions;
namespace Backend.Domain.Models;

/// <summary>
/// ドメインエンティティの基底クラス
/// 識別子（同一性）による等価比較を提供する
/// </summary>
/// <typeparam name="TId">エンティティの同一性を表す識別子の型</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    /// <summary>
    /// エンティティの同一性を表す識別子
    /// </summary>
    public TId Id { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">エンティティの同一性を表す識別子</param>
    /// <exception cref="DomainException">識別子が既定値(空GUID・0など)の場合</exception>
    protected Entity(TId id)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
        {
            throw new DomainException("エンティティの識別IDが指定されていません。");
        }
        Id = id;
    }

    /// <summary>
    /// 同じ型かつ同じ識別子を持つエンティティかどうかを判定
    /// </summary>
    /// <param name="other">比較対象のエンティティ</param>
    /// <returns>同一エンティティの場合はtrue</returns>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// オブジェクトとの等価判定
    /// </summary>
    /// <param name="obj">比較対象のオブジェクト</param>
    /// <returns>同一エンティティの場合はtrue</returns>
    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    /// <summary>
    /// 識別子に基づくハッシュコードを返す
    /// </summary>
    /// <returns>ハッシュコード</returns>
    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);

    /// <summary>
    /// 等価演算子
    /// </summary>
    /// <param name="left">左辺のエンティティ</param>
    /// <param name="right">右辺のエンティティ</param>
    /// <returns>同一エンティティの場合はtrue</returns>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    /// <summary>
    /// 非等価演算子
    /// </summary>
    /// <param name="left">左辺のエンティティ</param>
    /// <param name="right">右辺のエンティティ</param>
    /// <returns>異なるエンティティの場合はtrue</returns>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}