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
using Avalonia.Markup.Xaml.Templates;

namespace NP.Avalonia.UniDock
{
    public class DockItemHeaderControl : ContentControl
    {
        #region ButtonsTemplate Styled Avalonia Property
        public ControlTemplate ButtonsTemplate
        {
            get { return GetValue(ButtonsTemplateProperty); }
            set { SetValue(ButtonsTemplateProperty, value); }
        }

        public static readonly StyledProperty<ControlTemplate> ButtonsTemplateProperty =
            AvaloniaProperty.Register<DockItemHeaderControl, ControlTemplate>
            (
                nameof(ButtonsTemplate)
            );
        #endregion ButtonsTemplate Styled Avalonia Property
    }
}
