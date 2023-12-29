using NP.Concepts.Behaviors;
using System.Collections.Generic;

namespace NP.Ava.UniDock
{
    public class RemoveDockGroupBehavior<T> : RemoveItemBehavior<T>
        where T : IDockGroup
    {
        public RemoveDockGroupBehavior(ICollection<T> items) : base(items)
        {

        }

        protected override void OnRemoveItem(T item)
        {
            item.RemoveItselfFromParent();
            item.TheDockManager = null;
        }
    }
}
