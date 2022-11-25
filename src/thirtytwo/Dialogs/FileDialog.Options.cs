// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Windows.Dialogs;

public unsafe partial class FileDialog
{
    [Flags]
    public enum Options : uint
    {
        /// <summary>
        ///  When saving a file, prompt before overwriting an existing file of the same name.
        ///  This is a default value for the Save dialog.
        /// </summary>
        OverwritePrompt = FILEOPENDIALOGOPTIONS.FOS_OVERWRITEPROMPT,

        /// <summary>
        ///  In the Save dialog, only allow the user to choose a file that has one of the file name extensions specified.
        /// </summary>
        StrictFileTypes = FILEOPENDIALOGOPTIONS.FOS_STRICTFILETYPES,

        /// <summary>
        ///  Don't change the current working directory.
        /// </summary>
        NoChangeDirectory = FILEOPENDIALOGOPTIONS.FOS_NOCHANGEDIR,

        /// <summary>
        ///  Present an Open dialog that offers a choice of folders rather than files.
        /// </summary>
        PickFolders = FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS,

        /// <summary>
        ///  Ensures that returned items are file system items.
        /// </summary>
        ForceFileSystem = FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM,

        /// <summary>
        ///  Enables the user to choose any item in the Shell namespace.
        /// </summary>
        AllNonStorageItems = FILEOPENDIALOGOPTIONS.FOS_ALLNONSTORAGEITEMS,

        /// <summary>
        ///  Do not check for situations that would prevent an application from opening the selected file,
        ///  such as sharing violations or access denied errors.
        /// </summary>
        NoValidate = FILEOPENDIALOGOPTIONS.FOS_NOVALIDATE,

        /// <summary>
        ///  Enables the user to select multiple items in the open dialog.
        /// </summary>
        AllowMultiselect = FILEOPENDIALOGOPTIONS.FOS_ALLOWMULTISELECT,

        /// <summary>
        ///  The item returned must be in an existing folder. This is a default value.
        /// </summary>
        PathMustExist = FILEOPENDIALOGOPTIONS.FOS_PATHMUSTEXIST,

        /// <summary>
        ///  The item returned must exist. This is a default value for the Open dialog.
        /// </summary>
        FileMustExist = FILEOPENDIALOGOPTIONS.FOS_FILEMUSTEXIST,

        /// <summary>
        ///  Prompt for creation if the item returned in the save dialog does not exist. Note that this does not
        ///  actually create the item.
        /// </summary>
        CreatePrompty = FILEOPENDIALOGOPTIONS.FOS_CREATEPROMPT,

        /// <summary>
        ///  In the case of a sharing violation when an application is opening a file, call the application back
        ///  through OnShareViolation for guidance.
        /// </summary>
        ShareAware = FILEOPENDIALOGOPTIONS.FOS_SHAREAWARE,

        /// <summary>
        ///  Do not return read-only items. This is a default value for the Save dialog.
        /// </summary>
        NoReadOnlyReturn = FILEOPENDIALOGOPTIONS.FOS_NOREADONLYRETURN,

        /// <summary>
        ///  Do not test whether creation of the item as specified in the Save dialog will be successful.
        ///  If this flag is not set, the calling application must handle errors, such as denial of access,
        ///  discovered when the item is created.
        /// </summary>
        NoTestFileCreate = FILEOPENDIALOGOPTIONS.FOS_NOTESTFILECREATE,

        /// <summary>
        ///  Hide the list of places from which the user has recently opened or saved items. This value is not
        ///  supported as of Windows 7.
        /// </summary>
        HideMruPlaces = FILEOPENDIALOGOPTIONS.FOS_HIDEMRUPLACES,

        /// <summary>
        ///  Hide all of the standard namespace locations (such as Favorites, Libraries, Computer, and Network) shown
        ///  in the navigation pane.
        /// </summary>
        HidePinnedPlaces = FILEOPENDIALOGOPTIONS.FOS_HIDEPINNEDPLACES,

        /// <summary>
        ///  Shortcuts should not be treated as their target items. This allows an application to open a .lnk file
        ///  rather than what that file is a shortcut to.
        /// </summary>
        NoDereferenceLinks = FILEOPENDIALOGOPTIONS.FOS_NODEREFERENCELINKS,

        /// <summary>
        ///  The OK button will be disabled until the user navigates the view or edits the filename (if applicable).
        ///  Note: Disabling of the OK button does not prevent the dialog from being submitted by the Enter key.
        /// </summary>
        OkButtonNeedsInteraction = FILEOPENDIALOGOPTIONS.FOS_OKBUTTONNEEDSINTERACTION,

        /// <summary>
        ///  Do not add the item being opened or saved to the recent documents list.
        /// </summary>
        DoNotAddToRecent = FILEOPENDIALOGOPTIONS.FOS_DONTADDTORECENT,

        /// <summary>
        ///  Include hidden and system items.
        /// </summary>
        ForceShowHidden = FILEOPENDIALOGOPTIONS.FOS_FORCESHOWHIDDEN,

        /// <summary>
        ///  This value is not supported as of Windows 7.
        /// </summary>
        DefaultNoMiniMode = FILEOPENDIALOGOPTIONS.FOS_DEFAULTNOMINIMODE,

        /// <summary>
        ///  Indicates to the Open dialog box that the preview pane should always be displayed.
        /// </summary>
        ForcePreviewPaneOn = FILEOPENDIALOGOPTIONS.FOS_FORCEPREVIEWPANEON,

        /// <summary>
        ///  Indicates that the caller is opening a file as a stream (BHID_Stream), so there is no need to download
        ///  that file.
        /// </summary>
        SupportsStreamableItems = FILEOPENDIALOGOPTIONS.FOS_SUPPORTSTREAMABLEITEMS
    }
}