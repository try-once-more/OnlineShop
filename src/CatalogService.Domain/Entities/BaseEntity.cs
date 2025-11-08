namespace CatalogService.Domain.Entities;

public abstract class BaseEntity<T> : IEquatable<BaseEntity<T>> where T : IEquatable<T>
{

    /// <summary>
    /// Identifier of the entity
    /// </summary>
    public T? Id { get; init; }

    public static bool operator ==(BaseEntity<T>? a, BaseEntity<T>? b)
        => a is null ? b is null : a.Equals(b);

    public static bool operator !=(BaseEntity<T>? a, BaseEntity<T>? b)
        => !(a == b);

    public bool Equals(BaseEntity<T>? other) =>
        other is not null
        && GetType() == other.GetType()
        && !EqualityComparer<T>.Default.Equals(Id, default)
        && !EqualityComparer<T>.Default.Equals(other.Id, default)
        && Id!.Equals(other.Id);

    public override bool Equals(object? obj) =>
        obj is BaseEntity<T> other && Equals(other);

    public override int GetHashCode() =>
        EqualityComparer<T>.Default.Equals(Id, default) ? 0 : Id!.GetHashCode();
}
