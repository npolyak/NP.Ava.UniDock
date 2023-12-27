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

using Avalonia.Controls;
using Avalonia.VisualTree;
using NP.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace NP.Avalonia.UniDock.Serialization
{
    public class DockManagerParams
    {
        [XmlArray]
        public List<WindowParams>? WindowsSerializationParams { get; set; }

        [XmlArray]
        public List<DockGroupParams>? DisconnectedDockGroupParams { get; set; }

        [XmlArray]
        public List<DockGroupParams>? ConnectedDockGroupParams { get; set; }
    }

    public static class DockManagerParamsHelper
    {
        public static DockManagerParams ToParams(this DockManager dockManager)
        {
            DockManagerParams dmp = new DockManagerParams
            {
                WindowsSerializationParams = new List<WindowParams>(),
                DisconnectedDockGroupParams = new List<DockGroupParams>(),
                ConnectedDockGroupParams = new List<DockGroupParams>()
            };

            foreach(var window in dockManager.AllWindows)
            {
                dmp.WindowsSerializationParams.Add(window.ToWindowParams());
            }

            foreach(var group in dockManager.ConnectedGroups)
            {
                dmp.ConnectedDockGroupParams.Add(group.ToGroupParams());
            }    

            foreach(var group in dockManager.DisconnectedGroups)
            {
                dmp.DisconnectedDockGroupParams.Add(group.ToGroupParams());
            }

            return dmp;
        }

        private static Window? GetWindowById(this DockManager dm, string? windowId)
        {
            Window? w = 
                dm.AllWindows
                  .FirstOrDefault(win => DockAttachedProperties.GetWindowId(win) == windowId);

            return w;
        }

        private static WindowParams? GetWindowParamsById
        (
            this DockManagerParams dmp, 
            string? windowId)
        {
            WindowParams? wp = 
                dmp.WindowsSerializationParams
                   .NullToEmpty()
                   ?.FirstOrDefault(winParams => winParams.WindowId == windowId);

            return wp;
        }

        private static Window? ShowOwnersAndWindow
        (
            this DockManager dm, 
            DockManagerParams dmp, 
            string windowId)
        {
            WindowParams? wp = dmp.GetWindowParamsById(windowId);

            if (wp == null)
                return null;

            Window? w = dm.GetWindowById(windowId);

            if (w == null || w.IsVisible)
            {
                return w;
            }

            if (wp.DockChildWindowOwnerId != null)
            {
                Window? childWindowOwner = dm.GetWindowById(wp.DockChildWindowOwnerId);

                if (childWindowOwner != null)
                {
                    DockAttachedProperties.SetDockChildWindowOwner(w, childWindowOwner);
                }
            }

            if (wp.OwnerWindowId != null)
            {
                Window? owner = dm.ShowOwnersAndWindow(dmp, wp.OwnerWindowId);

                if (owner != null)
                {
                    w.Show(owner);
                    return w;
                }
            }

            w.Show();

            return w;
        }

        private static DockGroupParams? FindGroupParamsById(this DockManagerParams dmp, string? dockId)
        {
            var result = dmp.ConnectedDockGroupParams.NullToEmpty().FirstOrDefault(g => g.DockId == dockId);

            return result;
        }

        private static IDockGroup? BuildGroup(this DockManager dm, DockManagerParams dmp, string? dockId, IEnumerable<DockItem> dockItems)
        {
            DockGroupParams? p = dmp.FindGroupParamsById(dockId);
            IDockGroup? dg = dm.FindGroupById(dockId);

            if (p == null)
                return null;

            if (dg == null)
            {
                if (p.IsDockItem())
                {
                    dg = dockItems.FirstOrDefault(di => di.DockId == dockId);
                }
            }

            if (dg == null)
            {
                dg = p.ToGroup();
            }
            else
            {
                dg.SetGroupFromParams(p);
            }

            dg.TheDockManager = dm;

            foreach (var childDockId in p.ChildrenDockIds.NullToEmpty())
            {
                IDockGroup? childGroup = dm.BuildGroup(dmp, childDockId, dockItems);

                if (childGroup != null)
                {
                    if ((dg.DockChildren.Contains(childGroup)))
                    {
                        childGroup = dg.DockChildren.First(item => item.DockId == childGroup.DockId);
                        dg.DockChildren.Remove(childGroup); // to fix the order
                    }
                    dg.DockChildren.Add(childGroup);
                }
            }

            return dg;
        }

        public static void SetDockManagerFromParams
        (
            this DockManager dm, 
            DockManagerParams dmp, 
            bool restorePredefinedWindowsPositionParams = false)
        {
            List<DockItem> dockItems = dm.ConnectedGroups.OfType<DockItem>().ToList();

            dm.ClearGroups();

            var newWindowIds = dmp.WindowsSerializationParams.NullToEmpty().Select(wp => wp.WindowId).ToList();

            // clear the windows that do not exist in the serialization.
            var oldWindowsToRemove = 
                dm.AllWindows.Where(w => !DockAttachedProperties.GetWindowId(w).IsInValCollection(newWindowIds)).ToList();
            foreach(var w in oldWindowsToRemove)
            {
                w.Close();

                dm.RemoveWindow(w);

                DockAttachedProperties.SetTheDockManager(w, null!);
            }

            // create windows
            foreach(WindowParams wp in dmp.WindowsSerializationParams.NullToEmpty())
            {
                Window? w = 
                    dm.AllWindows.FirstOrDefault(win => DockAttachedProperties.GetWindowId(win) == wp.WindowId);

                if (w != null)
                {
                    bool setWindowPositionParams =
                        (!dm.PredefinedWindows.Contains(w)) || restorePredefinedWindowsPositionParams;

                    w.SetWindowFromParams(wp, setWindowPositionParams);
                }
                else if (wp.IsDockWindow)
                {
                    w = wp.RestoreWindow(dm);
                    w.SetWindowFromParams(wp, true);
                }
                else
                {
                    // if the window is predefined (not a dock window) it has to exist or it will be 
                    // skipped in reconstruction.
                }
            }

            foreach (WindowParams wp in dmp.WindowsSerializationParams.NullToEmpty())
            {
                // show windows - owners first
                dm.ShowOwnersAndWindow(dmp, wp.WindowId!);
            }

            // set the groups
            foreach(Window w in dm.AllWindows)
            {
                if (w is FloatingWindow dw)
                {
                    string dockId = dw.TheDockGroup.DockId;

                    dm.BuildGroup(dmp, dockId, dockItems);
                }
                else
                {
                    var winGroups = 
                        w.GetVisualDescendants().OfType<IDockGroup>().ToList();

                    winGroups.DoForEach(g => dm.BuildGroup(dmp, g.DockId, dockItems));
                }
            }

            // set the coefficients for the DockStackGroups
            foreach(var group in dm.ConnectedGroups)
            {
                DockGroupParams p = dmp.FindGroupParamsById(group.DockId)!;
                group.ProducingUserDefinedWindowGroup =
                    dm.FindGroupById(p.ProducingUserDefinedWindowGroupId) as RootDockGroup;

                if (group is StackDockGroup stackDockGroup)
                {
                    if (p?.SizeCoefficients != null)
                    {
                        stackDockGroup
                            .SetSizeCoefficients
                            (
                                p.SizeCoefficients.Select(str => GridLength.Parse(str)).ToArray());
                    }
                }
            }
        }
    }
}
