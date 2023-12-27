// (c) Nick Polyak 2021 - http://awebpros.com/
// License: MIT License (https://opensource.org/licenses/MIT)
//
// short overview of copyright rules:
// 1. you can use this framework in any commercial or non-commercial 
//    product as long as you retain this copyright message
// 2. Do not blame the author of this software if something goes wrong. 
// 
// Also, please, mention this software in any documentation for the 
// products that use it.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using NP.Avalonia.Visuals.Behaviors;
using NP.Utilities.BasicInterfaces;
using System;

namespace NP.Avalonia.UniDock
{
    public static class DockAttachedProperties
    {
        #region TheDockManager Attached Avalonia Property
        public static DockManager GetTheDockManager(AvaloniaObject obj)
        {
            return obj.GetValue(TheDockManagerProperty);
        }

        public static void SetTheDockManager(AvaloniaObject obj, DockManager? value)
        {
            obj.SetValue(TheDockManagerProperty!, value);
        }

        public static readonly AttachedProperty<DockManager> TheDockManagerProperty =
            AvaloniaProperty.RegisterAttached<object, Control, DockManager>
            (
                "TheDockManager"
            );
        #endregion TheDockManager Attached Avalonia Property

        static DockAttachedProperties()
        {
            TheDockManagerProperty.Changed.Subscribe(OnDockManagerChanged);
        }

        private static void OnDockManagerChanged(AvaloniaPropertyChangedEventArgs<DockManager> dockManagerChangeArgs)
        {
            var oldDockManager = dockManagerChangeArgs.OldValue.Value;

            var sender = dockManagerChangeArgs.Sender;

            var dockManager = dockManagerChangeArgs.NewValue.Value;

            if (sender is Window window)
            {
                if (oldDockManager != null)
                {
                    oldDockManager.RemoveWindow(window);
                }

                if (dockManager != null)
                {
                    dockManager.AddWindow(window);
                }
            }
            else if (sender is IDockGroup group)
            {
                if (oldDockManager != null)
                {
                    if (group is ILeafDockObj leafObj)
                    {
                        oldDockManager.DockLeafObjs.Remove(leafObj);
                    }

                    oldDockManager.RemoveConnectedGroup(group);
                }

                if (dockManager != null)
                {
                    dockManager.AddConnectedGroup(group);

                    if (group is ILeafDockObj leafObj)
                    {
                        dockManager.DockLeafObjs.Add(leafObj);
                    }
                }
            }
        }

        #region DockSide Attached Avalonia Property
        public static DockKind? GetDockSide(AvaloniaObject obj)
        {
            return obj.GetValue(DockSideProperty);
        }

        public static void SetDockSide(AvaloniaObject obj, DockKind? value)
        {
            obj.SetValue(DockSideProperty, value);
        }

        public static readonly AttachedProperty<DockKind?> DockSideProperty =
            AvaloniaProperty.RegisterAttached<object, Control, DockKind?>
            (
                "DockSide"
            );
        #endregion DockSide Attached Avalonia Property


        #region IconButtonForeground Attached Avalonia Property
        public static IBrush GetIconButtonForeground(AvaloniaObject obj)
        {
            return obj.GetValue(IconButtonForegroundProperty);
        }

        public static void SetIconButtonForeground(AvaloniaObject obj, IBrush value)
        {
            obj.SetValue(IconButtonForegroundProperty, value);
        }

        public static readonly AttachedProperty<IBrush> IconButtonForegroundProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IBrush>
            (
                "IconButtonForeground"
            );
        #endregion IconButtonForeground Attached Avalonia Property


        #region WindowId Attached Avalonia Property
        public static string? GetWindowId(AvaloniaObject obj)
        {
            return obj.GetValue(WindowIdProperty);
        }

        public static void SetWindowId(AvaloniaObject obj, string? value)
        {
            obj.SetValue(WindowIdProperty, value);
        }

        public static readonly AttachedProperty<string?> WindowIdProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string?>
            (
                "WindowId"
            );
        #endregion WindowId Attached Avalonia Property

        #region DockChildWindowOwner Attached Avalonia Property
        // specifies if the floating windows that have been pulled out of this window
        // or out of its floating descendants, should be owned by this window.
        public static Window GetDockChildWindowOwner(AvaloniaObject obj)
        {
            return obj.GetValue(DockChildWindowOwnerProperty);
        }

        public static void SetDockChildWindowOwner(AvaloniaObject obj, Window value)
        {
            obj.SetValue(DockChildWindowOwnerProperty, value);
        }

        public static readonly AttachedProperty<Window> DockChildWindowOwnerProperty =
            AvaloniaProperty.RegisterAttached<object, Control, Window>
            (
                "DockChildWindowOwner"
            );
        #endregion DockChildWindowOwner Attached Avalonia Property


        #region SizeGridLength Attached Avalonia Property
        public static GridLength? GetSizeGridLength(AvaloniaObject obj)
        {
            return obj.GetValue(SizeGridLengthProperty);
        }

        public static void SetSizeGridLength(AvaloniaObject obj, GridLength? value)
        {
            obj.SetValue(SizeGridLengthProperty, value);
        }

        public static readonly AttachedProperty<GridLength?> SizeGridLengthProperty =
            AvaloniaProperty.RegisterAttached<object, Control, GridLength?>
            (
                "SizeGridLength"
            );
        #endregion SizeGridLength Attached Avalonia Property


        #region IsDockVisible Attached Avalonia Property
        public static bool GetIsDockVisible(this IAvaloniaObject obj)
        {
            return obj.GetValue(IsDockVisibleProperty);
        }

        public static void SetIsDockVisible(IAvaloniaObject obj, bool value)
        {
            obj.SetValue(IsDockVisibleProperty, value);
        }

        public static readonly AttachedProperty<bool> IsDockVisibleProperty =
            AvaloniaProperty.RegisterAttached<object, IControl, bool>
            (
                "IsDockVisible",
                true
            );
        #endregion IsDockVisible Attached Avalonia Property


        #region OriginalDockGroup Attached Avalonia Property
        public static IDockGroup? GetOriginalDockGroup(IControl obj)
        {
            return obj.GetValue(OriginalDockGroupProperty);
        }

        public static void SetOriginalDockGroup(IControl obj, IDockGroup? value)
        {
            obj.SetValue(OriginalDockGroupProperty, value);
        }

        public static readonly AttachedProperty<IDockGroup?> OriginalDockGroupProperty =
            AvaloniaProperty.RegisterAttached<object, IControl, IDockGroup?>
            (
                "OriginalDockGroup"
            );
        #endregion OriginalDockGroup Attached Avalonia Property


        #region ShowHeader Attached Avalonia Property
        public static bool GetShowHeader(AvaloniaObject obj)
        {
            return obj.GetValue(ShowHeaderProperty);
        }

        public static void SetShowHeader(AvaloniaObject obj, bool value)
        {
            obj.SetValue(ShowHeaderProperty, value);
        }

        public static readonly AttachedProperty<bool> ShowHeaderProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>
            (
                "ShowHeader"
            );
        #endregion ShowHeader Attached Avalonia Property


        public static void ShowDockWindow(this Window dockWindow)
        {
            Window ownerWindow = DockAttachedProperties.GetDockChildWindowOwner(dockWindow);

            dockWindow.ShowWindow(ownerWindow);
        }

        #region CanReattachToDefaultGroup Attached Avalonia Property
        public static bool GetCanReattachToDefaultGroup(IDockGroup obj)
        {
            return obj.GetValue(CanReattachToDefaultGroupProperty);
        }

        public static void SetCanReattachToDefaultGroup(IDockGroup obj, bool value)
        {
            obj.SetValue(CanReattachToDefaultGroupProperty, value);
        }

        public static readonly AttachedProperty<bool> CanReattachToDefaultGroupProperty =
            AvaloniaProperty.RegisterAttached<IDockGroup, IDockGroup, bool>
            (
                "CanReattachToDefaultGroup"
            );
        #endregion CanReattachToDefaultGroup Attached Avalonia Property


        #region ShowCompass Attached Avalonia Property
        public static bool GetShowCompass(AvaloniaObject obj)
        {
            return obj.GetValue(ShowCompassProperty);
        }

        public static void SetShowCompass(AvaloniaObject obj, bool value)
        {
            obj.SetValue(ShowCompassProperty, value);
        }

        public static readonly AttachedProperty<bool> ShowCompassProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>
            (
                "ShowCompass"
            );
        #endregion ShowCompass Attached Avalonia Property


        #region IsInDockEditableState Attached Avalonia Property
        public static bool GetIsInDockEditableState(AvaloniaObject obj)
        {
            return obj.GetValue(IsInDockEditableStateProperty);
        }

        public static void SetIsInDockEditableState(AvaloniaObject obj, bool value)
        {
            obj.SetValue(IsInDockEditableStateProperty, value);
        }

        public static readonly AttachedProperty<bool> IsInDockEditableStateProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>
            (
                "IsInDockEditableState"
            );
        #endregion IsInDockEditableState Attached Avalonia Property


        #region ShowGroupBoundaries Attached Avalonia Property
        public static bool GetShowGroupBoundaries(AvaloniaObject obj)
        {
            return obj.GetValue(ShowGroupBoundariesProperty);
        }

        public static void SetShowGroupBoundaries(AvaloniaObject obj, bool value)
        {
            obj.SetValue(ShowGroupBoundariesProperty, value);
        }

        public static readonly AttachedProperty<bool> ShowGroupBoundariesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>
            (
                "ShowGroupBoundaries"
            );
        #endregion ShowGroupBoundaries Attached Avalonia Property


        #region IsPointerOverHeader Attached Avalonia Property
        public static bool GetIsPointerOverHeader(AvaloniaObject obj)
        {
            return obj.GetValue(IsPointerOverHeaderProperty);
        }

        public static void SetIsPointerOverHeader(AvaloniaObject obj, bool value)
        {
            obj.SetValue(IsPointerOverHeaderProperty, value);
        }

        public static readonly AttachedProperty<bool> IsPointerOverHeaderProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>
            (
                "IsPointerOverHeader"
            );
        #endregion IsPointerOverHeader Attached Avalonia Property


        #region IsUnderLockedGroup Attached Avalonia Property
        public static bool GetIsUnderLockedGroup(IDockGroup obj)
        {
            return obj.GetValue(IsUnderLockedGroupProperty);
        }

        public static void SetIsUnderLockedGroup(IDockGroup obj, bool value)
        {
            obj.SetValue(IsUnderLockedGroupProperty, value);
        }

        public static readonly AttachedProperty<bool> IsUnderLockedGroupProperty =
            AvaloniaProperty.RegisterAttached<object, IDockGroup, bool>
            (
                "IsUnderLockedGroup",
                false, 
                true
            );
        #endregion IsUnderLockedGroup Attached Avalonia Property


        #region AllowCenterDocking Attached Avalonia Property
        public static bool GetAllowCenterDocking(AvaloniaObject obj)
        {
            return obj.GetValue(AllowCenterDockingProperty);
        }

        public static void SetAllowCenterDocking(AvaloniaObject obj, bool value)
        {
            obj.SetValue(AllowCenterDockingProperty, value);
        }

        public static readonly AttachedProperty<bool> AllowCenterDockingProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>
            (
                "AllowCenterDocking",
                true
            );
        #endregion AllowCenterDocking Attached Avalonia Property


        #region CanFloat Attached Avalonia Property
        public static bool GetCanFloat(AvaloniaObject obj)
        {
            return obj.GetValue(CanFloatProperty);
        }

        public static void SetCanFloat(AvaloniaObject obj, bool value)
        {
            obj.SetValue(CanFloatProperty, value);
        }

        public static readonly AttachedProperty<bool> CanFloatProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>
            (
                "CanFloat",
                true
            );
        #endregion CanFloat Attached Avalonia Property


        #region CanClose Attached Avalonia Property
        public static bool GetCanClose(AvaloniaObject obj)
        {
            return obj.GetValue(CanCloseProperty);
        }

        public static void SetCanClose(AvaloniaObject obj, bool value)
        {
            obj.SetValue(CanCloseProperty, value);
        }

        public static readonly AttachedProperty<bool> CanCloseProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>
            (
                "CanClose",
                true
            );
        #endregion CanClose Attached Avalonia Property
    }
}
