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

using Avalonia.Layout;

namespace NP.Avalonia.UniDock
{
    public enum DockKind
    {
        Center,
        Left,
        Top,
        Right,
        Bottom
    }

    public static class DockKindHelper
    {
        public static Orientation? ToOrientation(this DockKind? dock)
        {
            return dock switch
            {
                DockKind.Left => Orientation.Horizontal,
                DockKind.Right => Orientation.Horizontal,
                DockKind.Top => Orientation.Vertical,
                DockKind.Bottom => Orientation.Vertical,
                _ => null
            };
        }

        public static int ToInsertIdx(this int idx, DockKind? dock)
        {
            switch(dock)
            {
                case DockKind.Right:
                case DockKind.Bottom:
                    return idx + 1;

                default:
                    return idx;
            }
        }
    }
}
