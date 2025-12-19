using System.Diagnostics.CodeAnalysis;

namespace CartService.Application.Entities;

public abstract class BaseEntity<T> : IEqualityComparer<T>
{
    public required T Id
    {
        get;
        init
        {
            if (EqualityComparer<T>.Default.Equals(value, default))
                throw new EntityIdInvalidException();

            field = value;
        }
    }

    public bool Equals(BaseEntity<T>? other) => other is not null && GetType() == other.GetType() && Id.Equals(other.Id);
    public override bool Equals(object? obj) => obj is BaseEntity<T> other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public bool Equals(T? x, T? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;
        return x.Equals(y);
    }

    public int GetHashCode([DisallowNull] T obj) => obj?.GetHashCode() ?? 0;
}
