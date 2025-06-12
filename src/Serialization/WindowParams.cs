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
using NP.Ava.Visuals;
using NP.Utilities;
using System.Xml.Serialization;

namespace NP.Ava.UniDock.Serialization
{
    public class WindowParams
    {
        [XmlAttribute]
        public string? TopLeft { get; set; }

        [XmlAttribute]
        public string? Size { get; set; }

        [XmlAttribute]
        public string? SaveTopLeft { get; set; }

        [XmlAttribute]
        public string? SavedSize { get; set; }

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

            wp.TopLeft = w.Position.ToPoint2D().ToString();

            wp.Size = w.Bounds.Size.ToPoint2D().ToString();

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
                wp.SaveTopLeft = dockWindow.SavedPosition?.ToString();
                wp.SavedSize = dockWindow.SavedSize?.ToString();

                wp.IsDockWindow = dockWindow.IsDockWindow;

                wp.TopLevelGroupId = dockWindow.TheDockGroup.DockId;
            }

            return wp;
        }

        public static void  SetWindowFromParams(this Window w, WindowParams wp, bool setWindowPositionParams)
        {
            if (setWindowPositionParams)
            {
                w.Position = wp.TopLeft?.ParseToPoint2D()?.ToPixelPoint() ?? new Avalonia.PixelPoint();
                Point2D restoredSize = wp.Size?.ParseToPoint2D() ?? new Point2D<double>();

                w.Width = restoredSize.X;
                w.Height = restoredSize.Y;

                w.Title = wp.Title;
                w.WindowState = wp.TheWindowState;
            }

            if (w is FloatingWindow dockWindow)
            {
                dockWindow.SavedPosition = wp.SaveTopLeft?.ParseToPoint2D();
                dockWindow.SavedSize = wp.SavedSize?.ParseToPoint2D();
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
