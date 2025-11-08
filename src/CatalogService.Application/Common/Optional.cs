namespace CatalogService.Application.Common;

public readonly struct Optional<T>
{
    public readonly bool HasValue { get; }
    public T Value
    {
        get => HasValue ? field : throw new InvalidOperationException();
        init;
    }

    public Optional(T value)
    {
        HasValue = true;
        Value = value;
    }

    public static implicit operator Optional<T>(T value) => new(value);
}