// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Windows.Support;

namespace Windows;

public static unsafe partial class DeviceContextExtensions
{
    /// <summary>
    ///  Scope for putting back an object selected into a device context.
    /// </summary>
    public readonly ref struct ObjectScope<T> where T : IHandle<HDC>
    {
        required public HGDIOBJ PriorObject { get; init; }
        required public T DeviceContext { get; init; }

        [SetsRequiredMembers]
        public ObjectScope(HGDIOBJ priorObject, T deviceContext)
        {
            PriorObject = priorObject;
            DeviceContext = deviceContext;
        }

        public void Dispose()
        {
            DeviceContext.SelectObject(PriorObject);
        }
    }
}