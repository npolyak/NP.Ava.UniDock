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
using Avalonia.VisualTree;
using NP.Avalonia.UniDockService;
using NP.Avalonia.Visuals.Behaviors;
using NP.Concepts.Behaviors;
using NP.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;

namespace NP.Avalonia.UniDock
{
    public interface IDockGroup : IDockManagerContainer, IControl, IRemovable, IDockDataContextContainer
    {
        public string DockId { get; set; }

        event Action<IDockGroup> DockIdChanged;

        event Action<IDockGroup> IsDockVisibleChangedEvent;

        internal void FireIsDockVisibleChangedEvent();

        IDictionary<IDockGroup, IDisposable> ChildSubscriptions { get; }

        Subject<Unit> DockChangedWithin { get; }

        IDockGroup? DockParent { get; set; }

        bool IsDockVisible
        {
            get => DockAttachedProperties.GetIsDockVisible(this);
            set => DockAttachedProperties.SetIsDockVisible(this, value);
        }

        Point FloatingSize { get; set; }

        // transient value only needed to pass size coefficient during the drop
        // into a StackDockPanel
        GridLength? InitialSizeCoeff { get; set; }

        public GroupKind TheGroupKind { get; }

        IList<IDockGroup> DockChildren { get; }

        bool AutoDestroy { get; set; }

        bool AutoInvisible { get; set; }

        RootDockGroup? ProducingUserDefinedWindowGroup { get; set; }

        bool ShowHeader { get => true; set { } }

        bool IsGroupEmpty => DockChildren == null || DockChildren.Count == 0;

        bool ShowCompass { get; set; }

        bool CanFloat
        {
            get => false;
            set => throw new NotImplementedException();
        }

        bool HasStableDescendant { get; }

        bool CanClose
        {
            get => false;
            set => throw new NotImplementedException();
        }

        string? GroupOnlyById { get; set; }

        /// stable groups become invisible when they do not have any items.
        /// They are not removed, when empty or have only one item. 
        /// They are used to set the default locations of predefined DockItems
        bool IsStableGroup
        {
            get => false;
            set
            {

            }
        }

        // IsPredefined == true can only be for DockItems
        // so that
        //      They can be restored from dock id (without any extra data) - for DockItems
        // IsPredefined == false means
        //      If dock item - it does not have a default location and needs a full parameter list with values to
        //          be restored - or rather - recreated
        bool IsPredefined 
        {
            get => false;
            set { }
        }

        bool IsRoot => DockParent == null;

        double DefaultDockOrderInGroup
        {
            get => 0;
            set { }
        }

        string? DefaultDockGroupId { get => null; }

        void CleanSelfOnRemove()
        {
            //this.TheDockManager = null;
        }

        protected void SimplifySelf();

        void Simplify() 
        {
            IDockGroup? dockParent = DockParent;
            SimplifySelf();
            dockParent?.Simplify();
        }

        IDockGroup? GetContainingGroup() => this;

        IControl GetVisual() => this;

        DockKind? CurrentGroupDock { get; }

        IEnumerable<DockItem> LeafItems
        {
            get
            {
                return this.GetDockGroupSelfAndDescendants()
                           .OfType<DockItem>()
                           .Distinct();
            }
        }

        bool IsGroupLocked
        {
            get => false;
            set
            {

            }
        }

        public bool AllowCenterDocking
        {
            get => true;
            set
            {

            }
        }
    }

    public interface ILeafDockObj : IDockGroup
    {
    }

    public static class DockGroupHelper
    {
        public static void FireChangeWithin(this IDockGroup group)
        {
            group.DockChangedWithin?.OnNext(Unit.Default);
        }

        public static void RemoveItselfFromParent(this IDockGroup item)
        {
            IDockGroup? parent = item.DockParent;

            if (parent != null)
            {
                parent.DockChildren!.Remove(item);
                item.DockParent = null;
            }

            item.CleanSelfOnRemove();
        }

        public static int GetNumberChildren(this IDockGroup item)
        {
            return item?.DockChildren?.Count ?? 0;
        }

        public static bool HasLeafAncestor(this IDockGroup item)
        {
            return 
                item.GetDockGroupAncestors()
                    .Any(ancestor => ancestor is ILeafDockObj);
        }


        public static bool HasLockedAncestor(this IDockGroup item)
        {
            return
                item.GetDockGroupAncestors()
                    .Any(anc => anc.IsGroupLocked);
        }


        public static GridLength GetSizeCoeff(this IDockGroup group, int idx)
        {
            if (group is StackDockGroup dockStackGroup)
            {
                return dockStackGroup.GetSizeCoefficient(idx);
            }

            return default;
        }

        public static void SetSizeCoeff(this IDockGroup group, int idx, GridLength coeff)
        {
            if (group is StackDockGroup dockStackGroup)
            {
                dockStackGroup.SetSizeCoefficient(idx, coeff);
            }
        }

        public static void SimplifySelfImpl(this IDockGroup group)
        {
            if (!group.AutoDestroy || group.IsStableGroup)
            {
                return;
            }

            if (group.GetNumberChildren() == 0)
            {
                group.RemoveItselfFromParent();
                group.TheDockManager = null;
            }

            IDockGroup? dockParent = group.DockParent;
            if (dockParent == null)
            {
                return;
            }

            if (group.GetNumberChildren() == 1)
            {
                FloatingWindow? window = group.GetGroupWindow();
                try
                {
                    window?.SetCloseIsNotAllowed();

                    int idx = dockParent.DockChildren.IndexOf(group);

                    GridLength sizeCoeff = dockParent.GetSizeCoeff(idx);

                    group.RemoveItselfFromParent();

                    IDockGroup child = group.DockChildren.First();
                    child.RemoveItselfFromParent();

                    group.TheDockManager = null;

                    dockParent.DockChildren.Insert(idx, child);

                    dockParent.SetSizeCoeff(idx, sizeCoeff);
                }
                finally
                {
                    window?.ResetIsCloseAllowed();
                }
            }
        }

        public static void SetIsDockVisible(this IDockGroup group)
        {
            bool isDockVisible = group.DockChildren.Any(child => child.IsDockVisible);

            if ((!isDockVisible) && group.AutoInvisible)
            {
                group.IsDockVisible = false;
            }
            else
            {
                group.IsDockVisible = true;
            }
        }

        private static IDisposable? _isDockVisibleChangeSubscription = null;
        internal static void SetIsDockVisibleChangeSubscription()
        {
            if (_isDockVisibleChangeSubscription == null)
            {
                _isDockVisibleChangeSubscription =
                    DockAttachedProperties
                        .IsDockVisibleProperty
                        .Changed
                        .Subscribe(OnIsDockVisbleChanged);
            }
        }
        public static IEnumerable<ILeafDockObj> GetLeafGroups(this IDockGroup dockGroup)
        {
            var leafGroups =
                dockGroup.GetDockGroupSelfAndDescendants(stopCondition: item => item is ILeafDockObj)
                         .OfType<ILeafDockObj>()
                         .Distinct();

            return leafGroups;
        }

        public static IEnumerable<IDockGroup> GetGroupsWithoutLockParts(this IDockGroup dockGroup)
        {
            return dockGroup.GetDockGroupSelfAndDescendants(stopCondition: item => (item.IsGroupLocked))
                            .Distinct();
        }

        public static IEnumerable<IDockGroup> GetLeafGroupsIncludingGroupsWithLock(this IDockGroup dockGroup)
        {
            var leafGroups =
                dockGroup.GetDockGroupSelfAndDescendants(stopCondition: item => (item.IsGroupLocked) || (item is ILeafDockObj))
                        .Where(g => g.IsGroupLocked || g is ILeafDockObj)
                        .Distinct();

            return leafGroups;
        }

        public static IEnumerable<IDockGroup> GetLeafGroupsWithoutLock(this IDockGroup dockGroup)
        {
            return dockGroup.GetLeafGroupsIncludingGroupsWithLock()
                            .Where(group => !group.IsGroupLocked)
                            .SelectMany(g => g.LeafItems).ToList();
        }

        public static IEnumerable<DockItem> GetLeafItems(this IDockGroup dockGroup)
        {
            return dockGroup.GetLeafGroups().SelectMany(g => g.LeafItems);
        }

        private static void OnIsDockVisbleChanged(AvaloniaPropertyChangedEventArgs<bool> args)
        {
            IDockGroup? dockGroup = args.Sender as IDockGroup;

            dockGroup?.FireIsDockVisibleChangedEvent();
        }

        public static FloatingWindow? GetGroupWindow(this IDockGroup group)
        {
            return group.GetDockGroupRoot()?.GetControlsWindow<FloatingWindow>();
        }

        public static DockObjectInfo ToDockObjectInfo(this IDockGroup group)
        {
            return new DockObjectInfo(group.DockId, group.TheGroupKind);
        }

        public static T? GetResource<T>(this IDockGroup? dockGroup, object resourceKey)
            where T : class
        {
            if (dockGroup == null)
                return null;

            T? result = dockGroup.FindResource(resourceKey) as T;

            if (result != null)
            {
                return result;
            }

            RootDockGroup? rootGroup = dockGroup.GetDockGroupRoot() as RootDockGroup;

            if (dockGroup != rootGroup)
            {
                result = rootGroup?.FindResource(resourceKey) as T;

                if (result != null)
                {
                    return result;
                }
            }

            RootDockGroup? userDefinedWindowGroup = rootGroup?.ProducingUserDefinedWindowGroup;

            return userDefinedWindowGroup?.GetResource<T>(resourceKey);
        }

        public static bool MatchesDefaultGroup(this IDockGroup? group, IDockGroup? dockGroupToMatch)
        {
            if ((group?.DockId).IsNullOrEmpty() || (group?.DefaultDockGroupId).IsNullOrEmpty())
            {
                return false;
            }

            return group!.DefaultDockGroupId == dockGroupToMatch?.DockId;
        }

        public static bool IsAllowedToReattachToDefaultGroup(this IDockGroup? group)
        {
            if (group == null)
                return false;

            return 
                group.TheDockManager != null &&
                group.DefaultDockGroupId != null &&
                !group.IsUnderDefaultParent();
        }

        public static bool IsUnderDefaultParent(this IDockGroup? group)
        {
            if (group?.DockParent == null)
                return false;

            return group.MatchesDefaultGroup(group.DockParent);
        }

        public static void SetCanReattachToDefaultGroup(this IDockGroup group)
        {
            DockAttachedProperties.SetCanReattachToDefaultGroup(group, group.IsAllowedToReattachToDefaultGroup());
        }

        public static void SubscribeToChildChange(this IDockGroup group, IDockGroup child)
        {
            IDisposable subscription = child.DockChangedWithin.Subscribe(u => group.DockChangedWithin.OnNext(u));

            group.ChildSubscriptions[child] = subscription;
        }

        public static void UnsubscribeFromChildChange(this IDockGroup group, IDockGroup child)
        {
            if (group.ChildSubscriptions.Remove(child, out IDisposable? subscription))
            {
                subscription?.Dispose();
            }
        }

        public static void ReattachToDefaultGroup(this IDockGroup group)
        {
            if (!group.IsAllowedToReattachToDefaultGroup())
            {
                throw new ProgrammingError
                (
                    "we cannot reattach to the " +
                    "default group, so we should never " +
                    "get to this method");
            }

            DockManager dm = group.TheDockManager!;

            IDockGroup? defaultGroup =
                dm.FindGroupById(group.DefaultDockGroupId);

            if (defaultGroup == null)
            {
                $"Default group '{group.DefaultDockGroupId}' does not exist".ThrowProgError();
            }

            if (!defaultGroup!.IsStableGroup)
            {
                $"Default group '{group.DefaultDockGroupId}' is not stable".ThrowProgError();
            }

            IDockGroup? parent = group.DockParent;
            IDockGroup topDockGroup = group.GetDockGroupRoot();

            if (parent != null)
            {
                group.RemoveItselfFromParent();
            }

            defaultGroup
                .DockChildren
                .InsertInOrder
                (
                    group,
                    dockGroup => dockGroup?.DefaultDockOrderInGroup ?? 0,
                    (i1, i2) => i1 < i2 ? -1 : i1 > i2 ? 1 : 0);

            parent?.Simplify();

            group.SetCanReattachToDefaultGroup();
            DockStaticEvents.FirePossibleDockChangeHappenedInsideEvent(topDockGroup);
        }

        public static bool HasStableGroup(this IDockGroup group)
        {
            return group.GetDockGroupSelfAndDescendants().Any(g => g.IsStableGroup);
        }

        public static void ClearGroups(this IDockGroup group)
        {

        }

        internal static DropPanelWithCompass? GetDropPanel(this IControl? control)
        {
            Panel? overlayWindowHolderPanel =
                control.GetVisualDescendants().OfType<Panel>().FirstOrDefault(p => p.Name == "PART_OverlayWindowHolder");

            if (overlayWindowHolderPanel == null)
                return null;

            DropPanelWithCompass? dropPanel =
                 OverlayWindowBehavior.GetOverlayWindow(overlayWindowHolderPanel)?.GetVisualDescendants()?.OfType<DropPanelWithCompass>()?.FirstOrDefault();

            return dropPanel;
        }

        public static DockKind? GetCurrentGroupDock(this IControl control)
        {
            return control.GetDropPanel()?.DockSide;
        }
    }
}
