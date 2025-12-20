namespace CartService.Application.Entities;

public abstract class BaseEntity<T>
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

    public override bool Equals(object? obj) =>
        obj is BaseEntity<T> other &&
        GetType() == other.GetType() &&
        !EqualityComparer<T>.Default.Equals(Id, default) &&
        EqualityComparer<T>.Default.Equals(Id, other.Id);

    public override int GetHashCode() =>
        HashCode.Combine(GetType(), EqualityComparer<T>.Default.GetHashCode(Id!));
}
