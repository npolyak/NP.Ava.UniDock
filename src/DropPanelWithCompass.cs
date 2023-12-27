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
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using System.Linq;
using System;
using Avalonia.Media;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;

namespace NP.Avalonia.UniDock
{
    public class DropPanelWithCompass : TemplatedControl
    {

        #region DockSide Styled Avalonia Property
        public DockKind? DockSide
        {
            get { return GetValue(DockSideProperty); }
            set { SetValue(DockSideProperty, value); }
        }

        public static readonly StyledProperty<DockKind?> DockSideProperty =
            AvaloniaProperty.Register<DropPanelWithCompass, DockKind?>
            (
                nameof(DockSide)
            );
        #endregion DockSide Styled Avalonia Property

        public bool CanStartPointerDetection
        {
            get => TheCompass?.CanStartPointerDetection ?? false;
            set
            {
                if (TheCompass != null)
                {
                    TheCompass.CanStartPointerDetection = value;
                }
            }
        }


        #region TheOrientation Styled Avalonia Property
        public Orientation? TheOrientation
        {
            get { return GetValue(TheOrientationProperty); }
            set { SetValue(TheOrientationProperty, value); }
        }

        public static readonly StyledProperty<Orientation?> TheOrientationProperty =
            AvaloniaProperty.Register<DropPanelWithCompass, Orientation?>
            (
                nameof(TheOrientation)
            );
        #endregion TheOrientation Styled Avalonia Property


        private DockCompass? TheCompass => 
            this.GetVisualDescendants()
                .OfType<DockCompass>()
                .FirstOrDefault();


        public void FinishPointerDetection()
        {
            TheCompass?.FinishPointerDetection();
        }


        #region ShowSeparator Styled Avalonia Property
        public bool ShowSeparator
        {
            get { return GetValue(ShowSeparatorProperty); }
            set { SetValue(ShowSeparatorProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowSeparatorProperty =
            AvaloniaProperty.Register<DropPanelWithCompass, bool>
            (
                nameof(ShowSeparator)
            );
        #endregion ShowSeparator Styled Avalonia Property


        #region ShowCompass Styled Avalonia Property
        public bool ShowCompass
        {
            get { return GetValue(ShowCompassProperty); }
            set { SetValue(ShowCompassProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowCompassProperty =
            AvaloniaProperty.Register<DropPanelWithCompass, bool>
            (
                nameof(ShowCompass)
            );
        #endregion ShowCompass Styled Avalonia Property


        #region SelectBackground Styled Avalonia Property
        public IBrush SelectBackground
        {
            get { return GetValue(SelectBackgroundProperty); }
            set { SetValue(SelectBackgroundProperty, value); }
        }

        public static readonly StyledProperty<IBrush> SelectBackgroundProperty =
            AvaloniaProperty.Register<DropPanelWithCompass, IBrush>
            (
                nameof(SelectBackground)
            );
        #endregion SelectBackground Styled Avalonia Property


        #region CompassHorizontalAlignment Styled Avalonia Property
        public HorizontalAlignment CompassHorizontalAlignment
        {
            get { return GetValue(CompassHorizontalAlignmentProperty); }
            set { SetValue(CompassHorizontalAlignmentProperty, value); }
        }

        public static readonly StyledProperty<HorizontalAlignment> CompassHorizontalAlignmentProperty =
            AvaloniaProperty.Register<DropPanelWithCompass, HorizontalAlignment>
            (
                nameof(CompassHorizontalAlignment)
            );
        #endregion CompassHorizontalAlignment Styled Avalonia Property


        #region CompassVerticalAlignment Styled Avalonia Property
        public VerticalAlignment CompassVerticalAlignment
        {
            get { return GetValue(CompassVerticalAlignmentProperty); }
            set { SetValue(CompassVerticalAlignmentProperty, value); }
        }

        public static readonly StyledProperty<VerticalAlignment> CompassVerticalAlignmentProperty =
            AvaloniaProperty.Register<DropPanelWithCompass, VerticalAlignment>
            (
                nameof(CompassVerticalAlignment)
            );
        #endregion CompassVerticalAlignment Styled Avalonia Property


        #region ShowHull Styled Avalonia Property
        public bool ShowHull
        {
            get { return GetValue(ShowHullProperty); }
            set { SetValue(ShowHullProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowHullProperty =
            DockCompass.ShowHullProperty.AddOwner<DropPanelWithCompass>();
        #endregion ShowHull Styled Avalonia Property


        #region AllowCenterDocking Styled Avalonia Property
        public bool AllowCenterDocking
        {
            get { return GetValue(AllowCenterDockingProperty); }
            set { SetValue(AllowCenterDockingProperty, value); }
        }

        public static readonly StyledProperty<bool> AllowCenterDockingProperty =
            DockCompass.AllowCenterDockingProperty.AddOwner<DropPanelWithCompass>();
        #endregion AllowCenterDocking Styled Avalonia Property


        #region AllowVerticalDocking Styled Avalonia Property
        public bool AllowVerticalDocking
        {
            get { return GetValue(AllowVerticalDockingProperty); }
            set { SetValue(AllowVerticalDockingProperty, value); }
        }

        public static readonly StyledProperty<bool> AllowVerticalDockingProperty =
            DockCompass.AllowVerticalDockingProperty.AddOwner<DropPanelWithCompass>();
        #endregion AllowVerticalDocking Styled Avalonia Property


        #region AllowHorizontalDocking Styled Avalonia Property
        public bool AllowHorizontalDocking
        {
            get { return GetValue(AllowHorizontalDockingProperty); }
            set { SetValue(AllowHorizontalDockingProperty, value); }
        }

        public static readonly StyledProperty<bool> AllowHorizontalDockingProperty =
            DockCompass.AllowHorizontalDockingProperty.AddOwner<DropPanelWithCompass>();
        #endregion AllowHorizontalDocking Styled Avalonia Property
    }
}
