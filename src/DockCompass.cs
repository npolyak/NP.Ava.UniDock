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
using NP.Avalonia.Visuals;
using NP.Avalonia.Visuals.Behaviors;
using NP.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NP.Avalonia.UniDock
{
    public class DockCompass : TemplatedControl
    {
        private IList<DockSideControl>? SideControls { get; set; }

        private IDisposable? _subscriptionDisposable = null;

        private bool IsSubscribed => _subscriptionDisposable != null;

        internal bool CanStartPointerDetection { get; set; } = true;


        #region IsAttached Styled Avalonia Property
        public bool IsAttached
        {
            get { return GetValue(IsAttachedProperty); }
            private set { SetValue(IsAttachedProperty, value); }
        }

        public static readonly StyledProperty<bool> IsAttachedProperty =
            AvaloniaProperty.Register<DockCompass, bool>
            (
                nameof(IsAttached)
            );
        #endregion IsAttached Styled Avalonia Property


        protected void StartPointerDetection()
        {
            if (!CanStartPointerDetection || !IsEffectivelyVisible)
                return;

            UpdateSideControls();

            if (IsSubscribed)
            {
                return;
            }

            _subscriptionDisposable =
                CurrentScreenPointBehavior.CurrentScreenPoint.Subscribe(OnPointerMoved);
        }


        #region ShowHull Styled Avalonia Property
        public bool ShowHull
        {
            get { return GetValue(ShowHullProperty); }
            set { SetValue(ShowHullProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowHullProperty =
            AvaloniaProperty.Register<DockCompass, bool>
            (
                nameof(ShowHull),
                true
            );
        #endregion ShowHull Styled Avalonia Property


        #region AllowCenterDocking Styled Avalonia Property
        public bool AllowCenterDocking
        {
            get { return GetValue(AllowCenterDockingProperty); }
            set { SetValue(AllowCenterDockingProperty, value); }
        }

        public static readonly StyledProperty<bool> AllowCenterDockingProperty =
            AvaloniaProperty.Register<DockCompass, bool>
            (
                nameof(AllowCenterDocking),
                true
            );
        #endregion AllowCenterDocking Styled Avalonia Property


        #region AllowVerticalDocking Styled Avalonia Property
        public bool AllowVerticalDocking
        {
            get { return GetValue(AllowVerticalDockingProperty); }
            set { SetValue(AllowVerticalDockingProperty, value); }
        }

        public static readonly StyledProperty<bool> AllowVerticalDockingProperty =
            AvaloniaProperty.Register<DockCompass, bool>
            (
                nameof(AllowVerticalDocking),
                true
            );
        #endregion AllowVerticalDocking Styled Avalonia Property


        #region AllowHorizontalDocking Styled Avalonia Property
        public bool AllowHorizontalDocking
        {
            get { return GetValue(AllowHorizontalDockingProperty); }
            set { SetValue(AllowHorizontalDockingProperty, value); }
        }

        public static readonly StyledProperty<bool> AllowHorizontalDockingProperty =
            AvaloniaProperty.Register<DockCompass, bool>
            (
                nameof(AllowHorizontalDocking),
                true
            );
        #endregion AllowHorizontalDocking Styled Avalonia Property


        #region StartOrEndPointerDetection Styled Avalonia Property
        public bool StartOrEndPointerDetection
        {
            get { return GetValue(StartOrEndPointerDetectionProperty); }
            set { SetValue(StartOrEndPointerDetectionProperty, value); }
        }

        public static readonly StyledProperty<bool> StartOrEndPointerDetectionProperty =
            AvaloniaProperty.Register<DockCompass, bool>
            (
                nameof(StartOrEndPointerDetection)
            );
        #endregion StartOrEndPointerDetection Styled Avalonia Property


        public DockCompass()
        {
            this.GetObservable(StartOrEndPointerDetectionProperty)
                .Subscribe(OnStartOrEndPointerDetectionChanged);

            this.DetachedFromVisualTree += DockCompass_DetachedFromVisualTree; 
            this.AttachedToVisualTree += DockCompass_AttachedToVisualTree;
        }

        private void DockCompass_DetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            IsAttached = false;
        }

        private void DockCompass_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            IsAttached = true;
        }

        private void OnStartOrEndPointerDetectionChanged(bool flag)
        {
            if (flag == true)
            {
                StartPointerDetection();
            }
            else
            {
                FinishPointerDetection();
            };
        }

        private void UpdateSideControls()
        {
            if (SideControls == null || SideControls.Count < 5)
            {
                SideControls =
                    this.GetVisualDescendants()
                        .OfType<DockSideControl>()
                        .ToList();
            }
        }

        private void OnPointerMoved(Point2D pointerScreenLocation)
        {
            if (!(this as IVisual).IsAttachedToVisualTree)
            {
                return;
            }

            UpdateSideControls();

            var currentSideControl =
                SideControls
                    ?.FirstOrDefault
                    (
                        c => PointHelper.GetScreenBounds(c).ContainsPoint(pointerScreenLocation));

            if (currentSideControl == null || (!currentSideControl.IsVisible))
            {
                this.ClearValue(DockAttachedProperties.DockSideProperty);
                return;
            }

            var currentDockSide = DockAttachedProperties.GetDockSide(this);
            var dockSide = DockAttachedProperties.GetDockSide(currentSideControl);

            if (dockSide != currentDockSide)
            {
                DockAttachedProperties.SetDockSide(this, dockSide);
            }
        }

        public void FinishPointerDetection()
        {
            _subscriptionDisposable?.Dispose();
            _subscriptionDisposable = null;
            this.ClearValue(DockAttachedProperties.DockSideProperty);
        }
    }
}
