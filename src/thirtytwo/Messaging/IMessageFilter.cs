// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows;

/// <summary>
///  Preprocesses messages retrieved by the current UI thread before managed window lookup and native dispatch.
/// </summary>
/// <remarks>
///  <para>
///   This interface is a per-thread integration point for components whose HWNDs are not represented by a managed
///   <see cref="Window"/>. Such components can perform required message translation without adding a dependency on
///   their UI technology to the core message loop.
///  </para>
///  <para>
///   Register filters with <see cref="Application.AddMessageFilter"/>. They run in registration order against a stable
///   snapshot; adding or removing a filter during a callback affects the next retrieved message.
///  </para>
///  <para>
///   Changes made to the message are visible to later filters and normal message processing. Returning
///   <see langword="true"/> stops later filters, managed-window preprocessing, translation, and dispatch for that
///   message.
///  </para>
/// </remarks>
public interface IMessageFilter
{
    /// <summary>
    ///  Returns <see langword="true"/> when the message has been handled and must not be dispatched.
    /// </summary>
    /// <param name="message">The message to inspect or modify.</param>
    /// <returns><see langword="true"/> when the message was handled; otherwise, <see langword="false"/>.</returns>
    bool PreFilterMessage(ref MSG message);
}
