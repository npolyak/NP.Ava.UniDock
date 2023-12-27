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

using NP.Concepts.Behaviors;
using System;
using System.Collections.Generic;

namespace NP.Avalonia.UniDock
{
    public class SetParentBehavior<T> : ForEachItemOverrideBehavior<T>
        where T : IDockGroup
    {
        public IDockGroup Parent { get; }

        public SetParentBehavior(IDockGroup parent, IEnumerable<T>? items) : base(items)
        {
            Parent = parent;
        }

        protected override void OnItemAdded(T item)
        {
            item.DockParent = Parent;
        }

        protected override void OnItemRemoved(T item)
        {
            item.DockParent = null;
        }
    }
}
