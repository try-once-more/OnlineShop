namespace CatalogService.Domain.Entities;

public abstract class BaseEntity<T>
{
    /// <summary>
    /// Identifier of the entity
    /// </summary>
    public T? Id { get; init; }

    public override bool Equals(object? obj) =>
        obj is BaseEntity<T> other &&
        GetType() == other.GetType() &&
        !EqualityComparer<T>.Default.Equals(Id, default) &&
        EqualityComparer<T>.Default.Equals(Id, other.Id);

    public override int GetHashCode() =>
        HashCode.Combine(GetType(), EqualityComparer<T>.Default.GetHashCode(Id!));
}
