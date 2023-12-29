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
using Avalonia.Controls.Generators;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using NP.Utilities;

namespace NP.Avalonia.UniDock
{
    public class DockTabsPresenter : ItemsControl
    {
        public override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        {
            var control = base.CreateContainerForItemOverride(item, index, recycleKey);

            control.Classes.RemoveAllOneByOne();

            control.Classes.Add("Dock");

            control.DataContext = item;

            return control;
        }

        private static readonly FuncTemplate<Panel?> DefaultPanel =
            new FuncTemplate<Panel?>
            (
                () => 
                new WrapPanel() 
                    { 
                        Orientation = Orientation.Horizontal 
                    });

        static DockTabsPresenter()
        {
            ItemsPanelProperty.OverrideDefaultValue<DockTabsPresenter>(DefaultPanel);
        }
    }
}
