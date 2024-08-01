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
using NP.Utilities;

namespace NP.Ava.UniDock
{
    public static class DockKindHelper
    {
        public static Orientation? ToOrientation(this Side2D? dock)
        {
            return dock switch
            {
                Side2D.Left => Orientation.Horizontal,
                Side2D.Right => Orientation.Horizontal,
                Side2D.Top => Orientation.Vertical,
                Side2D.Bottom => Orientation.Vertical,
                _ => null
            };
        }

        public static int ToInsertIdx(this int idx, Side2D? dock)
        {
            switch(dock)
            {
                case Side2D.Right:
                case Side2D.Bottom:
                    return idx + 1;

                default:
                    return idx;
            }
        }
    }
}
