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

using System;
using System.Collections.Generic;

namespace NP.Avalonia.UniDock
{
    public class SetDockGroupBehavior : IDisposable
    {
        private RemoveDockGroupBehavior<IDockGroup>? _removeItemBehavior;
        private SetParentBehavior<IDockGroup>? _setParentBehavior;
        private SetDockManagerPropertyFromParentGroupBehavior? _setDockManagerBehavior;
        private SetProducingUserDefinedWindowFromParentGroupBehavior? _setProducingUserDefinedWindowGroupBehavior;
        public SetDockGroupBehavior(IDockGroup parent, IList<IDockGroup> items)
        {
            _removeItemBehavior = new RemoveDockGroupBehavior<IDockGroup>(items);
            _setParentBehavior = new SetParentBehavior<IDockGroup>(parent, items);
            _setDockManagerBehavior = 
                new SetDockManagerPropertyFromParentGroupBehavior
                (
                    parent, 
                    items
                );

            _setProducingUserDefinedWindowGroupBehavior = 
                new SetProducingUserDefinedWindowFromParentGroupBehavior
                (
                    parent,
                    items
                );
        }

        public void Dispose()
        {
            _setDockManagerBehavior?.Dispose();
            _setDockManagerBehavior = null;
            _setParentBehavior?.Dispose();
            _setParentBehavior = null;
            _removeItemBehavior?.Dispose();
            _removeItemBehavior = null;
            _setProducingUserDefinedWindowGroupBehavior?.Dispose();
            _setProducingUserDefinedWindowGroupBehavior = null;
        }
    }
}
