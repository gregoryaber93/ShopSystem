namespace ShopService.Domain.Abstractions;

public abstract class ValueObject : IEquatable<ValueObject>
{
    public abstract IEnumerable<object?> GetAtomicValues();

    public bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
        {
            return false;
        }

        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ValueObject);
    }

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Select(value => value?.GetHashCode() ?? 0)
            .Aggregate(0, HashCode.Combine);
    }
}