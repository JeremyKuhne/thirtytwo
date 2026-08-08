// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.WinUI;

/// <summary>
///  Identifies the stage at which WinUI host initialization failed.
/// </summary>
public enum XamlHostInitializationStage
{
    /// <summary>Thread apartment, affinity, or core dispatcher validation.</summary>
    ThreadValidation,

    /// <summary>Windows App SDK dispatcher queue discovery or creation.</summary>
    DispatcherQueue,

    /// <summary>WinUI XAML manager initialization.</summary>
    XamlManager,

    /// <summary>Process application creation or compatibility validation.</summary>
    Application
}