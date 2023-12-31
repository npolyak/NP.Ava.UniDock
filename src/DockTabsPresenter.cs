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
using Avalonia.Controls.Generators;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Styling;
using NP.Utilities;
using System;

namespace NP.Ava.UniDock
{
    public class DockTabsPresenter : ItemsControl
    {
        protected override Type StyleKeyOverride => typeof(ItemsControl);

        public DockTabsPresenter()
        {
            
        }

        public override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        {
            bool result = base.NeedsContainerOverride(item, index, out recycleKey);

            if (item  is DockItem)
            {
                result = true;
            }
            return result;
        }

        public override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        {
            DockTabItem control = new DockTabItem();
                //(DockTabItem) base.CreateContainerForItemOverride(item, index, recycleKey);

            //control.ContentTemplate = this.ItemTemplate;

            control.Classes.Add("Dock");

            control.DataContext = item;

            return control;
        }

        //private static readonly FuncTemplate<Panel?> DefaultPanel =
        //    new FuncTemplate<Panel?>
        //    (
        //        () => 
        //        new WrapPanel() 
        //            { 
        //                Orientation = Orientation.Horizontal 
        //            });

        static DockTabsPresenter()
        {
            //ItemsPanelProperty.OverrideDefaultValue<DockTabsPresenter>(DefaultPanel);
        }
    }
}
