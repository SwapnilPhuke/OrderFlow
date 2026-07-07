namespace OrderFlow.Application.Interfaces;

/// <summary>
/// Abstracts DateTime.UtcNow to allow deterministic testing.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
