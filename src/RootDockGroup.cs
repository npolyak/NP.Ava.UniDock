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
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.VisualTree;
using NP.Avalonia.UniDockService;
using NP.Avalonia.Visuals.Behaviors;
using NP.Concepts.Behaviors;
using NP.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NP.Avalonia.UniDock
{
    public class RootDockGroup : DockGroupBaseControl, IDockGroup, IDisposable
    {
        public event Action<IDockGroup>? IsDockVisibleChangedEvent;

        public bool AutoInvisible { get; set; }

        public Point FloatingSize { get; set; } = new Point(700, 400);

        void IDockGroup.FireIsDockVisibleChangedEvent()
        {
            IsDockVisibleChangedEvent?.Invoke(this);

            this.FireChangeWithin();
        }

        public bool IsStableGroup
        {
            get => this.GetDockGroupDescendants().Any(g => g.IsStableGroup);
            set
            {

            }
        }

        public event Action<RootDockGroup>? HasNoChildrenEvent;

        private SingleActiveBehavior<DockItem> _singleActiveBehavior = new SingleActiveBehavior<DockItem>();

        public DropPanelWithCompass? DropPanel
        {
            get
            {
                Panel? root = this.GetVisualDescendants().OfType<Panel>().FirstOrDefault(p => p.Name == "PART_RootPanel");

                if (root == null)
                    return null;

                DropPanelWithCompass? dropPanel =
                    OverlayWindowBehavior.GetOverlayWindow(root)?.GetVisualDescendants()?.OfType<DropPanelWithCompass>()?.FirstOrDefault();

                return dropPanel;
            }
        }

        public DockKind? CurrentGroupDock => DropPanel?.DockSide;


        #region ActiveDockItem Direct Avalonia Property
        private DockItem? _ActiveDockItem = default;

        public static readonly DirectProperty<RootDockGroup, DockItem?> ActiveDockItemProperty =
            AvaloniaProperty.RegisterDirect<RootDockGroup, DockItem?>
            (
                nameof(ActiveDockItem),
                o => o.ActiveDockItem
            );

        public DockItem? ActiveDockItem
        {
            get => _ActiveDockItem;
            private set
            {
                SetAndRaise(ActiveDockItemProperty, ref _ActiveDockItem, value);
            }
        }

        #endregion ActiveDockItem Direct Avalonia Property


        #region NumberDockChildren Direct Avalonia Property
        public static readonly DirectProperty<RootDockGroup, int> NumberDockChildrenProperty =
            AvaloniaProperty.RegisterDirect<RootDockGroup, int>
            (
                nameof(NumberDockChildren),
                o => o.NumberDockChildren,
                (o, c) => o.NumberDockChildren = c
            );
        #endregion NumberDockChildren Direct Avalonia Property

        private int _numChildren = 0;
        public int NumberDockChildren
        {
            get => _numChildren;
            private set
            {
                SetAndRaise(NumberDockChildrenProperty, ref _numChildren,   value);
            }
        }

        public event Action<IRemovable>? RemoveEvent;

        public void Remove()
        {
            RemoveEvent?.Invoke(this);
        }

        public GroupKind TheGroupKind => GroupKind.Simple;

        public DockManager? TheDockManager
        {
            get => DockAttachedProperties.GetTheDockManager(this);
            set => DockAttachedProperties.SetTheDockManager(this, value);
        }


        #region ShowCompassCenter Styled Avalonia Property
        public bool ShowCompassCenter
        {
            get { return GetValue(ShowCompassCenterProperty); }
            set { SetValue(ShowCompassCenterProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowCompassCenterProperty =
            AvaloniaProperty.Register<RootDockGroup, bool>
            (
                nameof(ShowCompassCenter),
                true
            );
        #endregion ShowCompassCenter Styled Avalonia Property

        public override IDockGroup? DockParent
        {
            get => null;
            set => throw new NotImplementedException();
        }

        [Content]
        public IDockGroup? TheChild
        {
            get => DockChildren?.FirstOrDefault();
            set
            {
                DockChildren.RemoveAllOneByOne();

                if (value != null)
                {
                    DockChildren.Add(value);
                }
            }
        }


        public IList<IDockGroup> DockChildren { get; } = 
            new ObservableCollection<IDockGroup>();


        #region FloatingWindows Direct Avalonia Property
        private IList<FloatingWindowContainer>? _floatingWindows = 
            new ObservableCollection<FloatingWindowContainer>();

        public static readonly DirectProperty<RootDockGroup, IList<FloatingWindowContainer>?> FloatingWindowsProperty =
            AvaloniaProperty.RegisterDirect<RootDockGroup, IList<FloatingWindowContainer>?>
            (
                nameof(FloatingWindows),
                o => o.FloatingWindows
            );

        public IList<FloatingWindowContainer>? FloatingWindows
        {
            get => _floatingWindows;
        }

        #endregion FloatingWindows Direct Avalonia Property


        private IDisposable? _addRemoveChildBehavior;
        private SetDockGroupBehavior? _setBehavior;

        private readonly AttachedPropToCollectionBindingBehavior<DockManager, FloatingWindowContainer>? _floatingWindowDockManagerSettingBehavior;
        private readonly AttachedPropToCollectionBindingBehavior<Window, FloatingWindowContainer>? _floatingWindowParentWindowSettingBehavior;


        Grid _panel = new Grid();
        public RootDockGroup()
        {
            AffectsMeasure<RootDockGroup>(NumberDockChildrenProperty);

            _setBehavior = new SetDockGroupBehavior(this, DockChildren!);

            _addRemoveChildBehavior = 
                DockChildren.AddBehavior(OnChildAdded, OnChildRemoved);

            DockStaticEvents.PossibleDockChangeHappenedInsideEvent +=
                DockStaticEvents_PossibleDockChangeHappenedInsideEvent;

            _singleActiveBehavior.ActiveItemChangedEvent +=
                _singleActiveBehavior_ActiveItemChangedEvent;

            _floatingWindowDockManagerSettingBehavior =
                new AttachedPropToCollectionBindingBehavior<DockManager, FloatingWindowContainer>
                (
                    this, 
                    DockAttachedProperties.TheDockManagerProperty,
                    _floatingWindows,
                    (w, dm) => w.TheDockManager = dm
                );

            this.AttachedToVisualTree += SimpleDockGroup_AttachedToVisualTree;


            FloatingWindows.AddBehavior(OnFloatingWindowAdded);

            _floatingWindowParentWindowSettingBehavior = new AttachedPropToCollectionBindingBehavior<Window, FloatingWindowContainer>
            (
                this,
                OwningWindowProperty!,
                FloatingWindows,
                (floatingWindowContainer, ownerWindow) => floatingWindowContainer.ParentWindow = ownerWindow
            );

            this.AttachedToVisualTree += RootDockGroup_AttachedToVisualTree;
        }

        private void RootDockGroup_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            FloatingWindow? floatingWindow = this.GetGroupWindow();

            if (floatingWindow?.IsDockWindow != true)
            {
                this.ProducingUserDefinedWindowGroup = this; // this is a root group within user defined window

                foreach(FloatingWindowContainer floatingWindowContainer in this.FloatingWindows.NullToEmpty())
                {
                    floatingWindowContainer.ProducingUserDefinedWindowGroup = this;
                }
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            Panel rootPanel = e.NameScope.Find<Panel>("PART_RootPanel");

            _panel.RemoveFromParentPanel();

            rootPanel.Children.Insert(0, _panel);
        }

        private void OnFloatingWindowAdded(FloatingWindowContainer floatingWindowContainer)
        {
            floatingWindowContainer.ProducingUserDefinedWindowGroup = this.ProducingUserDefinedWindowGroup;
        }

        private void SimpleDockGroup_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            OwningWindow = this.GetVisualAncestors().OfType<Window>().FirstOrDefault();
        }

        #region OwningWindow Direct Avalonia Property
        private Window? _owningWindow = default;

        public static readonly DirectProperty<RootDockGroup, Window?> OwningWindowProperty =
            AvaloniaProperty.RegisterDirect<RootDockGroup, Window?>
            (
                nameof(OwningWindow),
                o => o.OwningWindow
            );

        public Window? OwningWindow
        {
            get => _owningWindow;
            private set
            {
                SetAndRaise(OwningWindowProperty, ref _owningWindow, value);
            }
        }
        #endregion OwningWindow Direct Avalonia Property


        private void _singleActiveBehavior_ActiveItemChangedEvent()
        {
            ActiveDockItem = _singleActiveBehavior.TheActiveItem;
        }

        private void DockStaticEvents_PossibleDockChangeHappenedInsideEvent(IDockGroup group)
        {
            if (group != this)
                return;

            _singleActiveBehavior.TheCollection = LeafItems.ToList();
        }

        private Control FindVisualChild(IDockGroup dockChild)
        {
            IControl control = dockChild;
            if (dockChild is ILeafDockObj leafDockChild)
            {
                control = leafDockChild.GetVisual();
            }

            return (Control) control;
        }


        private void OnChildAdded(IDockGroup newChildToInsert)
        {
            // have to remove previous children before adding the new one.
            // Only one child is allowed
            var childrenToRemove = DockChildren.Except(newChildToInsert.ToCollection()).ToList();

            childrenToRemove.DoForEach(child => DockChildren.Remove(child));

            IControl newVisualChildToInsert =
                TheDockManager!.TheDockVisualItemGenerator!.Generate(newChildToInsert)!;

            newVisualChildToInsert.RemoveFromParentPanel();

            _panel.Children.Add(newVisualChildToInsert);

            NumberDockChildren = DockChildren?.Count() ?? 0;
            newChildToInsert.IsDockVisibleChangedEvent += OnChildIsDockVisibleChanged;
            this.SetIsDockVisible();

            this.FireChangeWithin();
            this.SubscribeToChildChange(newChildToInsert);
        }

        private void OnChildIsDockVisibleChanged(IDockGroup obj)
        {
            this.SetIsDockVisible();
        }

        private void OnChildRemoved(IDockGroup childToRemove)
        {
            this.UnsubscribeFromChildChange(childToRemove);
            this.FireChangeWithin();
            childToRemove.IsDockVisibleChangedEvent -= OnChildIsDockVisibleChanged;
            Control visualChildToRemove = FindVisualChild(childToRemove);

            _panel.Children.Remove(visualChildToRemove);

            NumberDockChildren = DockChildren?.Count() ?? 0;
            this.SetIsDockVisible();
        }

        public void Dispose()
        {
            _setBehavior?.Dispose();
            _setBehavior = null;

            _addRemoveChildBehavior?.Dispose();
            _addRemoveChildBehavior = null;
        }

        void IDockGroup.SimplifySelf()
        {
            if (NumberDockChildren == 0)
            {
                HasNoChildrenEvent?.Invoke(this);
            }    
        }

        public bool AutoDestroy { get; set; } = true;

        public IEnumerable<DockItem> LeafItems => this.GetLeafItems();
    }
}
