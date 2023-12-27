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
using NP.Avalonia.Visuals;
using NP.Utilities;
using System;
using System.Xml.Serialization;

namespace NP.Avalonia.UniDock.Serialization
{
    public class WindowParams
    {
        public Point2D? TopLeft { get; set; }

        public Point2D? Size { get; set; }

        public Point2D? SaveTopLeft { get; set; }

        public Point2D? SavedSize { get; set; }

        [XmlAttribute]
        public bool IsDockWindow { get; set; } = false;

        [XmlAttribute]
        public string? Title { get; set; }

        [XmlAttribute]
        public WindowState TheWindowState { get; set; }

        [XmlAttribute]
        public string? OwnerWindowId { get; set; }

        [XmlAttribute]
        public string? WindowId { get; set; }

        [XmlAttribute]
        public string? DockChildWindowOwnerId { get; set; }

        [XmlAttribute]
        public string? TopLevelGroupId { get; set; }
    }

    public static class WindowParamsHelper
    {
        public static WindowParams ToWindowParams(this Window w)
        {
            WindowParams wp = new WindowParams();

            wp.TopLeft = w.Position.ToPoint2D();

            wp.Size = w.Bounds.Size.ToPoint2D();

            wp.Title = w.Title;

            wp.TheWindowState = w.WindowState;

            wp.WindowId = DockAttachedProperties.GetWindowId(w);

            if (w.Owner != null)
            {
                wp.OwnerWindowId = DockAttachedProperties.GetWindowId(w.Owner);
            }

            Window dockChildWindowOwnerWindow =
                DockAttachedProperties.GetDockChildWindowOwner(w);

            if (dockChildWindowOwnerWindow != null)
            {
                wp.DockChildWindowOwnerId =
                    DockAttachedProperties.GetWindowId(dockChildWindowOwnerWindow);
            }

            if (w is FloatingWindow dockWindow)
            {
                wp.SaveTopLeft = dockWindow.SavedPosition;
                wp.SavedSize = dockWindow.SavedSize;

                wp.IsDockWindow = dockWindow.IsDockWindow;

                wp.TopLevelGroupId = dockWindow.TheDockGroup.DockId;
            }

            return wp;
        }

        public static void  SetWindowFromParams(this Window w, WindowParams wp, bool setWindowPositionParams)
        {
            if (setWindowPositionParams)
            {
                w.Position = wp.TopLeft.ToPixelPoint();
                w.Width = wp.Size!.X;
                w.Height = wp.Size.Y;

                w.Title = wp.Title;
                w.WindowState = wp.TheWindowState;
            }

            if (w is FloatingWindow dockWindow)
            {
                dockWindow.SavedPosition = wp.SaveTopLeft;
                dockWindow.SavedSize = wp.SavedSize;
                dockWindow.IsDockWindow = wp.IsDockWindow;
            }

            string? windowId = wp.WindowId;

            if (windowId != null)
            {
                DockAttachedProperties.SetWindowId(w, windowId);
            }
        }

        public static Window RestoreWindow(this WindowParams wp, DockManager dm)
        {
            FloatingWindow w = dm.FloatingWindowFactory.CreateFloatingWindow();

            w.TheDockManager = dm;

            w.TheDockGroup.DockId = wp.TopLevelGroupId!;

            return w;
        }
    }
}
