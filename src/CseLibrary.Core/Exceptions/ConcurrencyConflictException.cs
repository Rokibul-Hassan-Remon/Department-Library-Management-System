namespace CseLibrary.Core.Exceptions;

/// <summary>The row changed since it was read (translated from EF's concurrency exception, D-14).</summary>
public sealed class ConcurrencyConflictException(string message, Exception inner) : Exception(message, inner);
