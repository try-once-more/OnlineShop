using System.Diagnostics.CodeAnalysis;

namespace CatalogService.Domain.Entities;

public abstract class BaseEntity<T> : IEqualityComparer<T>
{
    /// <summary>
    /// Identifier of the entity
    /// </summary>
    public T? Id { get; init; }

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

    public bool Equals(T? x, T? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;
        return x.Equals(y);
    }

    public int GetHashCode([DisallowNull] T obj) => obj?.GetHashCode() ?? 0;
}
