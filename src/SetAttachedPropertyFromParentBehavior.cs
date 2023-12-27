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
using NP.Concepts.Behaviors;
using System.Collections.Generic;
using System;
using NP.Utilities;
using System.Linq;
using Avalonia.Controls;

namespace NP.Avalonia.UniDock
{
    public class SetAttachedPropertyFromParentBehavior<TParent, T, TProp> : ForEachItemOverrideBehavior<T>
        where TParent : IDockManagerContainer, IControl
        where T : IControl
    {
        protected TParent Parent { get; }
        private AvaloniaProperty<TProp> _attachedProp;

        private IDisposable? _subscription;

        private IEnumerable<T>? _items;
        public SetAttachedPropertyFromParentBehavior
        (
            TParent parent, 
            IEnumerable<T>? items,
            AvaloniaProperty<TProp> attachedProperty) : base(items)
        {
            _items = items;
            Parent = parent;
            _attachedProp = attachedProperty;

            _subscription = _attachedProp.Changed.Subscribe(OnAttachedPropChanged);
        }

        private void OnAttachedPropChanged(AvaloniaPropertyChangedEventArgs<TProp> change)
        {
            if (!ReferenceEquals(change.Sender, Parent))
            {
                return;
            }

            _items?.ToList()?.DoForEach(item => SetItemValue(item));
        }

        private void SetItemValue(T item)
        {
            item.SetValue(_attachedProp, Parent.GetValue(_attachedProp));
        }

        protected override void OnItemAdded(T item)
        {
            SetItemValue(item);
        }

        protected override void OnItemRemoved(T item)
        {
            
        }

        public override void Dispose()
        {
            base.Dispose();

            _subscription?.Dispose();
            _subscription = null;
        }
    }

    public class SetAttachedPropertyFromParentGroupBehavior<TProp> : 
        SetAttachedPropertyFromParentBehavior<IDockGroup, IDockGroup, TProp>
    {
        public SetAttachedPropertyFromParentGroupBehavior
        (
            IDockGroup parent, 
            IEnumerable<IDockGroup>? items, 
            AvaloniaProperty<TProp> attachedProperty) :
            base(parent, items, attachedProperty)
        {

        }
    }

    public class SetDockManagerPropertyFromParentGroupBehavior :
        SetAttachedPropertyFromParentGroupBehavior<DockManager>
    {
        public SetDockManagerPropertyFromParentGroupBehavior
        (
            IDockGroup parent, 
            IEnumerable<IDockGroup>? items
        ) :
            base(parent, items, DockAttachedProperties.TheDockManagerProperty)
        {

        }

        protected override void OnItemAdded(IDockGroup item)
        {
            IDockGroup topGroup = Parent.GetDockGroupRoot();

            base.OnItemAdded(item);

            DockStaticEvents.FirePossibleDockChangeHappenedInsideEvent(topGroup);
        }
    }

    public class SetProducingUserDefinedWindowFromParentGroupBehavior :
        SetAttachedPropertyFromParentGroupBehavior<RootDockGroup>
    {
        public SetProducingUserDefinedWindowFromParentGroupBehavior
        (
            IDockGroup parent,
            IEnumerable<IDockGroup>? items
        ) : base(parent, items, DockGroupBaseControl.ProducingUserDefinedWindowGroupProperty!)
        {

        }

        protected override void OnItemAdded(IDockGroup item)
        {
            if (item.ProducingUserDefinedWindowGroup == null)
            {
                base.OnItemAdded(item);
            }
        }
    }
}
