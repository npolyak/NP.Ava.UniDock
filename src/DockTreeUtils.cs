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

using NP.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NP.Avalonia.UniDock
{
    public static class DockGroupTreeUtils
    {
        public static IDockGroup? ToParent(IDockGroup g) => g?.DockParent;

        public static IEnumerable<IDockGroup>? ToChildren(IDockGroup g) =>
            g.DockChildren;

        public static IEnumerable<IDockGroup> GetDockGroupAncestors(this IDockGroup g)
        {
            return g.Ancestors(ToParent!);
        }

        public static IEnumerable<IDockGroup> GetDockGroupSelfAndAncestors(this IDockGroup g)
        {
            return g.SelfAndAncestors(ToParent!);
        }

        public static IDockGroup GetDockGroupRoot(this IDockGroup g)
        {
            return g.GetRootNode(ToParent!);
        }

        public static IEnumerable<IDockGroup> GetDockGroupSelfAndDescendants(this IDockGroup g, Func<IDockGroup, bool>? stopCondition = null) =>
            g.SelfAndDescendants(ToChildren!, stopCondition);

        public static IEnumerable<IDockGroup> GetDockGroupDescendants(this IDockGroup g) =>
           g.Descendants(ToChildren!);
    }
}
