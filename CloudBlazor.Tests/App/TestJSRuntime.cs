using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;

namespace CloudBlazor.Tests.App;

/// <summary>
/// Records interop calls so tests can assert which browser API a navigation service
/// reached for, and with which arguments.
/// </summary>
internal sealed class TestJSRuntime : IJSRuntime
{
    /// <summary>
    /// Identifiers that should throw a <see cref="JSException"/>, so the fallback paths
    /// can be exercised.
    /// </summary>
    public HashSet<string> FailingIdentifiers { get; } = new(StringComparer.Ordinal);

    public List<InvocationRecord> Invocations { get; } = [];

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        Invocations.Add(new InvocationRecord(identifier, args ?? []));

        if (FailingIdentifiers.Contains(identifier))
            throw new JSException($"'{identifier}' is configured to fail.");

        // Every call in these services returns void; IJSVoidResult is what the runtime
        // uses for that, and default is a valid value for it.
        return ValueTask.FromResult<TValue>(typeof(TValue) == typeof(IJSVoidResult) ? default! : default!);
    }

    internal sealed record InvocationRecord(string Identifier, object?[] Arguments);
}
