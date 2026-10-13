using CseLibrary.Core.Paging;
using CseLibrary.Core.Results;

namespace CseLibrary.Tests.Unit;

public class CoreContractTests
{
    [Fact]
    public void Result_success_and_failure_carry_their_state()
    {
        Assert.True(Result.Success().Succeeded);

        var failed = Result.Failure(ErrorCodes.NotFound, "Not found");
        Assert.False(failed.Succeeded);
        Assert.Equal(ErrorCodes.NotFound, failed.ErrorCode);
        Assert.Equal("Not found", failed.Error);

        var ok = Result<int>.Success(42);
        Assert.True(ok.Succeeded);
        Assert.Equal(42, ok.Value);
        Assert.Null(ok.ErrorCode);

        var typedFail = Result<int>.Failure(ErrorCodes.Duplicate, "Exists");
        Assert.False(typedFail.Succeeded);
        Assert.Equal(ErrorCodes.Duplicate, typedFail.ErrorCode);
    }

    [Theory] // D2: page size above the max is clamped; nonsense values fall back to the edges
    [InlineData(1, 20, 1, 20, 0)]
    [InlineData(3, 10, 3, 10, 20)]
    [InlineData(1, 500, 1, PageRequest.MaxPageSize, 0)]
    [InlineData(0, 0, 1, 1, 0)]
    [InlineData(-5, -1, 1, 1, 0)]
    public void PageRequest_clamps_page_and_size(int page, int size, int expPage, int expSize, int expSkip)
    {
        var r = new PageRequest(page, size);
        Assert.Equal(expPage, r.Page);
        Assert.Equal(expSize, r.PageSize);
        Assert.Equal(expSkip, r.Skip);
    }

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    public void PagedResult_total_pages_rounds_up(int total, int size, int expPages) =>
        Assert.Equal(expPages, new PagedResult<int>([], total, 1, size).TotalPages);
}
