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
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using NP.Avalonia.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NP.Avalonia.UniDock
{
    public class DragTabBehavior : DragItemBehavior<TabItem>
    {

        #region IsSet Attached Avalonia Property
        public static bool GetIsSet(AvaloniaObject obj)
        {
            return obj.GetValue(IsSetProperty);
        }

        public static void SetIsSet(AvaloniaObject obj, bool value)
        {
            obj.SetValue(IsSetProperty, value);
        }

        public static readonly AttachedProperty<bool> IsSetProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>
            (
                "IsSet"
            );
        #endregion IsSet Attached Avalonia Property


        private static DockItem? GetDockItem(TabItem tabItem) =>
            tabItem.Content as DockItem;

        private static IList<IDockGroup>? GetDockItemsCollection(DockItem dockItem) =>
            (dockItem.DockParent as TabbedDockGroup)?.Items;

        protected override bool MoveItemWithinContainer(Control itemsContainer, PointerEventArgs e)
        {
            Point pointerPositionWithinItemsContainer = e.GetPosition(itemsContainer);

            DockItem? draggedDockItem = _draggedDockGroup as DockItem;
            IList<IDockGroup> itemsList = GetDockItemsCollection(draggedDockItem!)!;

            if (itemsContainer.IsPointWithinControl(pointerPositionWithinItemsContainer))
            {
                var siblingTabs =
                    itemsContainer.GetVisualDescendants().OfType<TabItem>().ToList();

                TabItem? tabMouseOver =
                    siblingTabs?.FirstOrDefault(tab => tab.IsPointerWithinControl(e));

                if (tabMouseOver != null && tabMouseOver != _startItem)
                {
                    int draggedDockItemIdx = itemsList.IndexOf(_draggedDockGroup!);

                    IDockGroup dropDockItem = _dockGroupGetter(tabMouseOver);
                    int dropIdx = itemsList!.IndexOf(dropDockItem);

                    itemsList?.Remove(_draggedDockGroup!);

                    itemsList?.Insert(dropIdx, _draggedDockGroup!);

                    draggedDockItem!.IsSelected = true;
                }

                return true;
            }

            return false;
        }

        public DragTabBehavior() : base(GetDockItem!)
        {

        }

        static DragTabBehavior()
        {
            Instance = new DragTabBehavior();
            IsSetProperty.Changed.Subscribe(OnIsSetChanged);
        }
    }
}
