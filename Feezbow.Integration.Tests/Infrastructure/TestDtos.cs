namespace Feezbow.Integration.Tests.Infrastructure;

/// <summary>
/// Mirrors Result&lt;T&gt; for JSON deserialization in tests.
/// Result&lt;T&gt; has an internal constructor that System.Text.Json cannot call.
/// </summary>
public record ApiResult<T>(T Value, string Message, bool IsSuccess);

/// <summary>
/// Generic wrapper for API responses that return Result&lt;long&gt;.
/// </summary>
public record CreateResponse(ApiResult<long> Result);
