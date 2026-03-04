using System;
using UnityEngine;

/// <summary>
/// Fire-and-forget helper for Awaitable.
/// Catches OperationCanceledException silently so that
/// destroyCancellationToken / Application.exitCancellationToken
/// don't spam the console when play-mode stops.
/// </summary>
public static class AwaitableExtensions
{
    public static async void Forget(this Awaitable awaitable)
    {
        try
        {
            await awaitable;
        }
        catch (OperationCanceledException) { }
    }
}
