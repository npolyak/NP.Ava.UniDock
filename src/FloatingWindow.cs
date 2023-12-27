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
using Avalonia.Metadata;
using NP.Avalonia.Visuals;
using NP.Avalonia.Visuals.Behaviors;
using NP.Avalonia.Visuals.Controls;
using NP.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace NP.Avalonia.UniDock
{
    public class FloatingWindow : CustomWindow, IDockManagerContainer
    {
        public RootDockGroup TheDockGroup { get; } = 
            new RootDockGroup();

        public bool IsDockWindow { get; set; }

        [Content]
        public IDockGroup? DockContent
        {
            get => TheDockGroup.TheChild;

            set
            {
                TheDockGroup.TheChild = value;
            }
        }


        public RootDockGroup? ProducingUserDefinedWindowGroup
        { 
            get => TheDockGroup.ProducingUserDefinedWindowGroup;
            set
            {
                TheDockGroup.ProducingUserDefinedWindowGroup = value;
            }
        }

        public DockManager? TheDockManager
        {
            get => DockAttachedProperties.GetTheDockManager(this);
            set => DockAttachedProperties.SetTheDockManager(this, value!);
        }

        #region SavedPosition Styled Avalonia Property
        public Point2D? SavedPosition
        {
            get { return GetValue(SavedPositionProperty); }
            internal set { SetValue(SavedPositionProperty, value); }
        }

        public static readonly StyledProperty<Point2D?> SavedPositionProperty =
            AvaloniaProperty.Register<FloatingWindow, Point2D?>
            (
                nameof(SavedPosition)
            );
        #endregion SavedPosition Styled Avalonia Property


        #region SavedSize Styled Avalonia Property
        public Point2D? SavedSize
        {
            get { return GetValue(SavedSizeProperty); }
            internal set { SetValue(SavedSizeProperty, value); }
        }

        public static readonly StyledProperty<Point2D?> SavedSizeProperty =
            AvaloniaProperty.Register<FloatingWindow, Point2D?>
            (
                nameof(SavedSize)
            );
        #endregion SavedSize Styled Avalonia Property


        private void OnDockManagerChanged(AvaloniaPropertyChangedEventArgs e)
        {
            TheDockGroup.TheDockManager = TheDockManager;
        }


        static FloatingWindow()
        {
            DockAttachedProperties
                .TheDockManagerProperty
                .Changed
                .AddClassHandler<FloatingWindow>((dw, e) => dw.OnDockManagerChanged(e));
        }

        private readonly IDisposable _subscription;
        public FloatingWindow() 
        {
            DragOnBeginMove = false;
            HasCustomWindowFeatures = true;
            Content = TheDockGroup;
            IsDockWindow = true;

            this.Closing += FloatingWindow_Closing;

            this.Closed += FloatingWindow_Closed;

            TheDockGroup.IsDockVisibleChangedEvent += 
                TheDockGroup_IsDockVisibleChangedEvent;

            _subscription = 
                TheDockGroup.DockChangedWithin.Subscribe(OnDockChangedWithin);

            this.GetObservable(WindowStateProperty).Subscribe(OnWindowStateChanged);
        }

        private void OnWindowStateChanged(WindowState windowState)
        {
            if (windowState == WindowState.Normal)
            {
                if (SavedPosition != null)
                {
                    this.Position = SavedPosition.ToPixelPoint();
                    SavedPosition = null;
                }

                if (SavedSize != null)
                {

                    this.Width = SavedSize.X;
                    this.Height = SavedSize.Y;
                    SavedSize = null;
                }
            }
        }

        #region
        // the following functionality is used in 
        private bool _isCloseAllowed = true;

        internal void SetCloseIsNotAllowed()
        {
            _isCloseAllowed = false;
        }

        internal void ResetIsCloseAllowed()
        {
            _isCloseAllowed = true;
        }
        #endregion

        public string? GroupOnlyById => TheDockGroup.GroupOnlyById;

        private void OnDockChangedWithin(Unit obj)
        {
            CanClose = !TheDockGroup.HasStableDescendant;

            CloseIfAllowed();

            CanReattachToDefaultGroup =
                TheDockGroup.GetGroupsWithoutLockParts()
                    .Where(item => item.IsAllowedToReattachToDefaultGroup())
                    .Any();
        }

        private void FloatingWindow_Closed(object? sender, EventArgs e)
        {
            TheDockManager = null;
            var allGroups = TheDockGroup.GetDockGroupSelfAndDescendants().Reverse().ToList();

            foreach (var group in allGroups)
            {
                group.RemoveItselfFromParent();
                group.TheDockManager = null;
            }
        }

        private bool _isInsideClose = false;
        internal void CloseIfAllowed(bool closeOrHide = true)
        {
            if (CanClose && this.LeafItems.Count() == 0 && _isCloseAllowed)
            {
                if (closeOrHide)
                {
                    if (!_isInsideClose)
                    {
                        // to prevent calling Close() recursively
                        _isInsideClose = true;
                        this.Close();
                        _isInsideClose = false;
                    }
                }
                else
                {
                    this.Hide();
                }
            }
        }

        private void TheDockGroup_IsDockVisibleChangedEvent(IDockGroup obj)
        {

        }

        public void DoInvalidateStyles()
        {
            this.InvalidateArrange();
            this.InvalidateStyles();
        }

        protected virtual void BeforeClosing(CancelEventArgs e)
        {
        }

        private void SetPosition()
        {
            if (this.WindowState != WindowState.Normal)
            {
                return;
            }

            SavedPosition = Position.ToPoint2D();
            SavedSize = new Point2D(this.Bounds.Width, this.Bounds.Height);
        }

        public override void Minimize()
        {
            SetPosition();
            base.Minimize();
        }

        public override void Maximize()
        {
            SetPosition();
            base.Maximize();
        }

        private void FloatingWindow_Closing(object? sender, CancelEventArgs e)
        {
            BeforeClosing(e);
        }

        public void SetMovePtr()
        {
            SetInitialPosition();
            this.Activated += CustomWindow_Activated!;
        }

        private async void CustomWindow_Activated(object sender, EventArgs e)
        {
            this.Activated -= CustomWindow_Activated!;
            SetInitialPosition();
            SetDragOnMovePointer();

            await Task.Delay(200);

            TheDockManager?.SetGroups();
        }

        private void SetInitialPosition()
        {
            StartPointerPosition = CurrentScreenPointBehavior.CurrentScreenPointValue.ToPixelPoint();
            StartWindowPosition = StartPointerPosition - new PixelPoint(60, 10);
            Position = StartWindowPosition;
        }

        protected override void SetDragOnMovePointer()
        {
            TheDockManager!.DraggedWindow = this;

            CurrentScreenPointBehavior.Capture(HeaderControl);

            base.SetDragOnMovePointer();
        }

        protected override void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(sender, e);

            TheDockManager?.CompleteDragDropAction();
        }

        public IEnumerable<DockItem> LeafItems => TheDockGroup.LeafItems;

        public IEnumerable<DockItem> LeafItemsWithDefaultPosition =>
            TheDockGroup.GetLeafGroupsWithoutLock()
                        .OfType<DockItem>()
                        .Where(item => !item.DefaultDockGroupId.IsNullOrEmpty());

        #region CanReattachToDefaultGroup Styled Avalonia Property
        public bool CanReattachToDefaultGroup
        {
            get { return GetValue(CanReattachToDefaultGroupProperty); }
            private set { SetValue(CanReattachToDefaultGroupProperty, value); }
        }

        public static readonly StyledProperty<bool> CanReattachToDefaultGroupProperty =
            AvaloniaProperty.Register<FloatingWindow, bool>
            (
                nameof(CanReattachToDefaultGroup)
            );
        #endregion CanReattachToDefaultGroup Styled Avalonia Property

        public void ReattachToDefaultGroup()
        {
            TheDockGroup.GetGroupsWithoutLockParts()
                .Where(item => item.IsAllowedToReattachToDefaultGroup())
                .ToList()
                .DoForEach(item => item.ReattachToDefaultGroup());
        }

        public IEnumerable<IDockGroup> GetLeafGroupsIncludingGroupsWithLock()
        {
            return TheDockGroup.GetLeafGroupsIncludingGroupsWithLock();
        }

        public IEnumerable<IDockGroup> GetLeafGroupsWithoutLock()
        {
            return TheDockGroup.GetLeafGroupsWithoutLock();
        }
    }
}
