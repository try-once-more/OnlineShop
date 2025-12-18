namespace CartService.Application.Entities;

public abstract class BaseEntity<T> : IEquatable<BaseEntity<T>> where T : IEquatable<T>
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

    public static bool operator ==(BaseEntity<T>? a, BaseEntity<T>? b) => a is null ? b is null : a.Equals(b);

    public static bool operator !=(BaseEntity<T>? a, BaseEntity<T>? b) => !(a == b);
    public bool Equals(BaseEntity<T>? other) => other is not null && GetType() == other.GetType() && Id.Equals(other.Id);
    public override bool Equals(object? obj) => obj is BaseEntity<T> other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
}
