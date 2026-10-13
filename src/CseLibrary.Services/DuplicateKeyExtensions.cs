using CseLibrary.Core.Exceptions;
using CseLibrary.Core.Interfaces;
using CseLibrary.Core.Results;

namespace CseLibrary.Services;

internal static class DuplicateKeyExtensions
{
    /// <summary>
    /// The pre-check gives the friendly message; the filtered unique index catches a concurrent duplicate,
    /// which becomes <paramref name="duplicate"/> instead of an HTTP 500 (D-14).
    /// </summary>
    public static async Task<TResult> OnDuplicate<TResult>(this Task<TResult> write, TResult duplicate) where TResult : Result
    {
        try
        {
            return await write;
        }
        catch (DuplicateKeyException)
        {
            return duplicate;
        }
    }

    public static async Task<Result> SaveAsync(this IUnitOfWork uow, CancellationToken ct)
    {
        await uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
