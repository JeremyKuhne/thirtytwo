// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

public static partial class DeviceContextExtensions
{
    /// <summary>
    ///  Scope for putting back an object selected into a device context.
    /// </summary>
    /// <param name="priorObject">The previously selected GDI object that will be restored on dispose.</param>
    /// <param name="deviceContext">The device context that currently has a replacement object selected.</param>
    [method: SetsRequiredMembers]
    public readonly ref struct ObjectScope<T>(HGDIOBJ priorObject, T deviceContext) where T : IHandle<HDC>
    {
        /// <summary>
        ///  Gets the GDI object that was selected into the device context before this scope.
        /// </summary>
        public required HGDIOBJ PriorObject { get; init; } = priorObject;

        /// <summary>
        ///  Gets the target device context that will be restored when disposed.
        /// </summary>
        public required T DeviceContext { get; init; } = deviceContext;

        /// <summary>
        ///  Restores <see cref="PriorObject"/> into <see cref="DeviceContext"/>.
        /// </summary>
        public void Dispose() => DeviceContext.SelectObject(PriorObject);
    }
}