namespace CseLibrary.Core.Exceptions;

/// <summary>A unique index rejected the write (SQL errors 2601/2627, D-14).</summary>
public sealed class DuplicateKeyException(string message, Exception inner) : Exception(message, inner);
