// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.UI.Xaml;

namespace Windows.WinUI;

/// <summary>
///  Composes application resource dictionaries using WinUI merge precedence.
/// </summary>
/// <remarks>
///  <para>Instances are bound to their construction thread; cross-thread member access throws.</para>
/// </remarks>
public sealed class XamlResourceDictionaryRegistry
{
    private readonly XamlThreadAffinity _affinity = new();
    private readonly ResourceDictionary _applicationResources;
    private readonly List<ResourceDictionary> _dictionaries = [];
    private readonly HashSet<ResourceDictionary> _registered = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<object, ResourceDictionary> _resourceOwners = [];

    public XamlResourceDictionaryRegistry(ResourceDictionary applicationResources)
    {
        ArgumentNullException.ThrowIfNull(applicationResources);
        _applicationResources = applicationResources;

        foreach (ResourceDictionary dictionary in applicationResources.MergedDictionaries)
        {
            _dictionaries.Add(dictionary);
            _registered.Add(dictionary);
            IndexResources(dictionary);
        }
    }

    /// <summary>Occurs when a later dictionary overrides a key from an earlier dictionary.</summary>
    public event EventHandler<XamlResourceCollisionEventArgs>? CollisionDetected;

    /// <summary>Gets the number of registered dictionaries.</summary>
    public int Count
    {
        get
        {
            _affinity.VerifyAccess();
            return _dictionaries.Count;
        }
    }

    /// <summary>Gets dictionaries in WinUI merge order.</summary>
    public IReadOnlyList<ResourceDictionary> Dictionaries
    {
        get
        {
            _affinity.VerifyAccess();
            return [.. _dictionaries];
        }
    }

    /// <summary>Gets the managed owner thread identifier.</summary>
    public int OwnerManagedThreadId => _affinity.ManagedThreadId;

    /// <summary>Gets the native owner thread identifier.</summary>
    public uint OwnerNativeThreadId => _affinity.NativeThreadId;

    /// <summary>
    ///  Registers a dictionary. Registering the same instance more than once has no effect.
    /// </summary>
    /// <returns><see langword="true"/> when the dictionary was added.</returns>
    public bool Register(ResourceDictionary dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        _affinity.VerifyAccess();

        if (!_registered.Add(dictionary))
        {
            return false;
        }

        try
        {
            ReportCollisions(dictionary);
            _dictionaries.Add(dictionary);
            try
            {
                _applicationResources.MergedDictionaries.Add(dictionary);
                IndexResources(dictionary);
                return true;
            }
            catch
            {
                _dictionaries.Remove(dictionary);
                throw;
            }
        }
        catch
        {
            _registered.Remove(dictionary);
            throw;
        }
    }

    private void ReportCollisions(ResourceDictionary winningDictionary)
    {
        if (CollisionDetected is null && !XamlHostEventSource.Log.IsEnabled())
        {
            return;
        }

        foreach (KeyValuePair<object, object> resource in winningDictionary)
        {
            if (_resourceOwners.TryGetValue(resource.Key, out ResourceDictionary? existing))
            {
                XamlHostEventSource.Log.ResourceCollision(resource.Key.ToString() ?? resource.Key.GetType().Name);
                CollisionDetected?.Invoke(
                    this,
                    new(resource.Key, existing, winningDictionary));
            }
        }
    }

    private void IndexResources(ResourceDictionary dictionary)
    {
        foreach (KeyValuePair<object, object> resource in dictionary)
        {
            _resourceOwners[resource.Key] = dictionary;
        }
    }
}