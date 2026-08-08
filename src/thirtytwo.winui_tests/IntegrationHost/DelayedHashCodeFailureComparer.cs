// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IntegrationHost;

internal sealed class DelayedHashCodeFailureComparer : IEqualityComparer<object>
{
    private bool _failureEnabled;
    private object? _failureKey;
    private int _successfulHashCallsRemaining;

    public new bool Equals(object? first, object? second)
        => EqualityComparer<object>.Default.Equals(first, second);

    public int GetHashCode(object value)
    {
        if (_failureEnabled
            && EqualityComparer<object>.Default.Equals(value, _failureKey)
            && _successfulHashCallsRemaining-- == 0)
        {
            throw new InvalidOperationException("Expected resource key hash failure.");
        }

        return EqualityComparer<object>.Default.GetHashCode(value);
    }

    internal void FailAfterSuccessfulCalls(object failureKey, int successfulHashCalls)
    {
        ArgumentNullException.ThrowIfNull(failureKey);
        ArgumentOutOfRangeException.ThrowIfNegative(successfulHashCalls);
        _failureKey = failureKey;
        _successfulHashCallsRemaining = successfulHashCalls;
        _failureEnabled = true;
    }

    internal void DisableFailure()
    {
        _failureEnabled = false;
        _failureKey = null;
    }
}