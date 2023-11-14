// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows;

public static unsafe partial class DeviceContextExtensions
{
    /// <summary>
    ///  Scope for putting back an object selected into a device context.
    /// </summary>
    [method: SetsRequiredMembers]
    public readonly ref struct ObjectScope<T>(HGDIOBJ priorObject, T deviceContext) where T : IHandle<HDC>
    {
        public required HGDIOBJ PriorObject { get; init; } = priorObject;
        public required T DeviceContext { get; init; } = deviceContext;

        public void Dispose() => DeviceContext.SelectObject(PriorObject);
    }
}