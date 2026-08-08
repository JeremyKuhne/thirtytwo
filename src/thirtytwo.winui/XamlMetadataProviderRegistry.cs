// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml.Markup;

namespace Windows.WinUI;

/// <summary>
///  Composes XAML metadata providers in deterministic registration order.
/// </summary>
/// <remarks>
///  <para>Instances are bound to their construction thread; cross-thread member access throws.</para>
/// </remarks>
public sealed class XamlMetadataProviderRegistry : IXamlMetadataProvider
{
    private readonly XamlThreadAffinity _affinity = new();
    private readonly List<IXamlMetadataProvider> _providers = [];
    private readonly HashSet<Type> _providerTypes = [];

    /// <summary>Occurs when a later provider also resolves a type already resolved by an earlier provider.</summary>
    public event EventHandler<XamlMetadataCollisionEventArgs>? CollisionDetected;

    /// <summary>Gets the number of registered provider types.</summary>
    public int Count
    {
        get
        {
            _affinity.VerifyAccess();
            return _providers.Count;
        }
    }

    /// <summary>Gets provider types in deterministic registration order.</summary>
    public IReadOnlyList<Type> ProviderTypes
    {
        get
        {
            _affinity.VerifyAccess();
            return _providers.Select(provider => provider.GetType()).ToArray();
        }
    }

    /// <summary>Gets the managed owner thread identifier.</summary>
    public int OwnerManagedThreadId => _affinity.ManagedThreadId;

    /// <summary>Gets the native owner thread identifier.</summary>
    public uint OwnerNativeThreadId => _affinity.NativeThreadId;

    /// <summary>
    ///  Registers a provider. A provider type already present is treated as the same registration.
    /// </summary>
    /// <returns><see langword="true"/> when the provider was added.</returns>
    public bool Register(IXamlMetadataProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _affinity.VerifyAccess();

        if (!_providerTypes.Add(provider.GetType()))
        {
            return false;
        }

        _providers.Add(provider);
        return true;
    }

    /// <summary>Resolves a XAML type name using first-provider precedence.</summary>
    public IXamlType? GetXamlType(string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        _affinity.VerifyAccess();
        return Resolve(fullName, provider => provider.GetXamlType(fullName));
    }

    /// <summary>Resolves a managed type using first-provider precedence.</summary>
    public IXamlType? GetXamlType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        _affinity.VerifyAccess();
        return Resolve(type.FullName ?? type.Name, provider => provider.GetXamlType(type));
    }

    /// <summary>Gets namespace definitions in provider registration order.</summary>
    public XmlnsDefinition[] GetXmlnsDefinitions()
    {
        _affinity.VerifyAccess();
        List<XmlnsDefinition> definitions = [];
        IXamlMetadataProvider[] providers = [.. _providers];
        foreach (IXamlMetadataProvider provider in providers)
        {
            definitions.AddRange(provider.GetXmlnsDefinitions());
        }

        return [.. definitions];
    }

    private IXamlType? Resolve(string requestedType, Func<IXamlMetadataProvider, IXamlType?> resolve)
    {
        IXamlType? result = null;
        Type? winningProviderType = null;
        IXamlMetadataProvider[] providers = [.. _providers];

        foreach (IXamlMetadataProvider provider in providers)
        {
            IXamlType? candidate = resolve(provider);
            if (candidate is null)
            {
                continue;
            }

            if (result is null)
            {
                result = candidate;
                winningProviderType = provider.GetType();
                continue;
            }

            XamlHostEventSource.Log.MetadataCollision(
                requestedType,
                winningProviderType!.FullName ?? winningProviderType.Name,
                provider.GetType().FullName ?? provider.GetType().Name);
            CollisionDetected?.Invoke(
                this,
                new(requestedType, winningProviderType, provider.GetType()));
        }

        return result;
    }
}