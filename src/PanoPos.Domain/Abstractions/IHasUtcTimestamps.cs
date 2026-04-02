namespace PanoPos.Domain.Abstractions;

public interface IHasUtcTimestamps
{
    DateTime CreatedAtUtc { get; }
}
