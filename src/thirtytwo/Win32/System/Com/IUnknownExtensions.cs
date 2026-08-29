// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Win32.System.Com;

/// <summary>
///  Extension helpers for querying interfaces from an <see cref="IUnknown"/> pointer.
/// </summary>
public static unsafe class IUnknownExtensions
{
    /// <summary>
    ///  Provides extension helpers for a COM <see cref="IUnknown"/> interface reference.
    /// </summary>
    /// <param name="unknown">The COM interface reference that extension members operate on.</param>
    extension(ref IUnknown unknown)
    {
        /// <summary>
        ///  Attempts to query for the requested COM interface.
        /// </summary>
        /// <typeparam name="TInterface">The COM interface type to query.</typeparam>
        /// <returns>The queried interface pointer on success; otherwise <see langword="null"/>.</returns>
        /// <remarks>
        ///  <para>
        ///   On success, COM query semantics apply and the returned pointer has an incremented reference count.
        ///  </para>
        /// </remarks>
        public TInterface* TryQueryInterface<TInterface>() where TInterface : unmanaged, IComIID
        {
            TInterface* @interface = default;
            unknown.QueryInterface(IID.Get<TInterface>(), (void**)&@interface);
            return @interface;
        }

        /// <summary>
        ///  Queries for the requested COM interface and throws on failure.
        /// </summary>
        /// <typeparam name="TInterface">The COM interface type to query.</typeparam>
        /// <returns>The queried interface pointer.</returns>
        public TInterface* QueryInterface<TInterface>() where TInterface : unmanaged, IComIID
        {
            TInterface* @interface = default;
            unknown.QueryInterface(IID.Get<TInterface>(), (void**)&@interface).ThrowOnFailure();
            return @interface;
        }

        /// <summary>
        ///  Attempts to query for the requested COM interface and wraps it in an agile pointer helper.
        /// </summary>
        /// <typeparam name="TInterface">The COM interface type to query.</typeparam>
        /// <returns>
        ///  A new agile pointer wrapper that owns the queried reference, or <see langword="null"/> on failure.
        /// </returns>
        public AgileComPointer<TInterface>? TryQueryAgileInterface<TInterface>()
            where TInterface : unmanaged, IComIID
        {
            TInterface* @interface = unknown.TryQueryInterface<TInterface>();
            return @interface is null ? null : new(@interface, takeOwnership: true);
        }
    }
}