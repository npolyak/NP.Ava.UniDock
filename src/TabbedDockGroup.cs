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
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using NP.Avalonia.UniDockService;
using NP.Avalonia.Visuals.Behaviors;
using NP.Concepts.Behaviors;
using NP.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;

namespace NP.Avalonia.UniDock
{
    public class TabbedDockGroup : DockGroupBaseControl, ILeafDockObj
    {
        public event Action<IDockGroup>? IsDockVisibleChangedEvent;

        public GroupKind TheGroupKind => GroupKind.Tab;

        void IDockGroup.FireIsDockVisibleChangedEvent()
        {
            IsDockVisibleChangedEvent?.Invoke(this);

            DockChangedWithin?.OnNext(Unit.Default);
        }

        private bool _isStableGroup = false;
        public bool IsStableGroup 
        { 
            get => _isStableGroup; 
            set
            {
                if (_isStableGroup == value)
                    return;

                _isStableGroup = value;
            }
        }

        public Point FloatingSize { get; set; } = new Point(700, 400);

        #region HeaderBackground Styled Avalonia Property
        public IBrush HeaderBackground
        {
            get { return GetValue(HeaderBackgroundProperty); }
            set { SetValue(HeaderBackgroundProperty, value); }
        }

        public static readonly StyledProperty<IBrush> HeaderBackgroundProperty =
            AvaloniaProperty.Register<TabbedDockGroup, IBrush>
            (
                nameof(HeaderBackground)
            );
        #endregion HeaderBackground Styled Avalonia Property


        #region HeaderForeground Styled Avalonia Property
        public IBrush HeaderForeground
        {
            get { return GetValue(HeaderForegroundProperty); }
            set { SetValue(HeaderForegroundProperty, value); }
        }

        public static readonly StyledProperty<IBrush> HeaderForegroundProperty =
            AvaloniaProperty.Register<TabbedDockGroup, IBrush>
            (
                nameof(HeaderForeground)
            );
        #endregion HeaderForeground Styled Avalonia Property


        #region ShowHeader Styled Avalonia Property
        public bool ShowHeader
        {
            get { return GetValue(ShowHeaderProperty); }
            set { SetValue(ShowHeaderProperty, value); }
        }

        public static readonly AttachedProperty<bool> ShowHeaderProperty =
            DockAttachedProperties.ShowHeaderProperty.AddOwner<TabbedDockGroup>();
        #endregion ShowHeader Styled Avalonia Property


        public bool AutoInvisible { get; set; } = false;

        #region IsActive Styled Avalonia Property
        public bool IsActive
        {
            get { return GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<TabbedDockGroup, bool>
            (
                nameof(IsActive)
            );
        #endregion IsActive Styled Avalonia Property


        #region IsFullyActive Styled Avalonia Property
        public bool IsFullyActive
        {
            get { return GetValue(IsFullyActiveProperty); }
            set { SetValue(IsFullyActiveProperty, value); }
        }

        public static readonly StyledProperty<bool> IsFullyActiveProperty =
            AvaloniaProperty.Register<TabbedDockGroup, bool>
            (
                nameof(IsFullyActive)
            );
        #endregion IsFullyActive Styled Avalonia Property


        string? _defaultDockGroupId;
        public string? DefaultDockGroupId
        {
            get => _defaultDockGroupId;
            set
            {
                if (_defaultDockGroupId == value)
                    return;

                _defaultDockGroupId = value;

                this.SetCanReattachToDefaultGroup();
            }
        }

        static TabbedDockGroup()
        {
            TabStripPlacementProperty
                .Changed
                .AddClassHandler<TabbedDockGroup>((sender, e) => sender.OnTabStringPlacementChanged(e));
        }

        private void OnTabStringPlacementChanged(AvaloniaPropertyChangedEventArgs e)
        {
            OnTabStringPlacementChanged();
        }

        private void OnTabStringPlacementChanged()
        {
            this.TabOrientation =
                (TabStripPlacement == Dock.Top || TabStripPlacement == Dock.Bottom) 
                    ? 
                    Orientation.Horizontal : Orientation.Vertical;
        }

        public DockManager? TheDockManager
        {
            get => DockAttachedProperties.GetTheDockManager(this);
            set => DockAttachedProperties.SetTheDockManager(this, value);
        }

        IDisposable? _setItemsBehavior;

        public event Action<IRemovable>? RemoveEvent;

        public void Remove()
        {
            RemoveEvent?.Invoke(this);
        }

        /// <summary>
        /// Defines the <see cref="Items"/> property.
        /// </summary>
        public static readonly DirectProperty<TabbedDockGroup, IList<IDockGroup>?> ItemsProperty =
            AvaloniaProperty.RegisterDirect<TabbedDockGroup, IList<IDockGroup>?>
            (
                nameof(Items),
                o => o.Items,
                (o, v) => o.Items = v);

        private IList<IDockGroup>? _items = new ObservableCollection<IDockGroup>();
        /// <summary>
        /// Gets or sets the items to display.
        /// </summary>
        [Content]
        public IList<IDockGroup>? Items
        {
            get
            {
                return _items;
            }

            set
            {
                DisposeBehavior();

                SetAndRaise(ItemsProperty!, ref _items, value);

                SetBehavior();
            }
        }

        public IList<IDockGroup>? DockChildren => Items;

        public DockKind? CurrentGroupDock => this.GetCurrentGroupDock();

        private readonly SingleSelectionBehavior<DockItem> _singleSelectionBehavior =
            new SingleSelectionBehavior<DockItem>();

        private readonly MimicCollectionBehavior<IDockGroup, DockItem, ObservableCollection<DockItem>> _mimicCollectionBehavior =
            new MimicCollectionBehavior<IDockGroup, DockItem, ObservableCollection<DockItem>>(dockGroup => (DockItem)dockGroup);

        public TabbedDockGroup()
        {
            AffectsMeasure<DockTabsPresenter>(TabStripPlacementProperty);
            AffectsMeasure<TabbedDockGroup>(DockAttachedProperties.IsDockVisibleProperty);

            SetBehavior();

            _singleSelectionBehavior.PropertyChanged += 
                _singleSelectionBehavior_PropertyChanged;

            SetSelectedItem();

            OnTabStringPlacementChanged();

            this.AddHandler(InputElement.PointerPressedEvent, OnTabbedDockGroupPressed, RoutingStrategies.Bubble, true);
        }

        private void OnTabbedDockGroupPressed(object? sender, PointerPressedEventArgs e)
        {
            if (SelectedItem is IActiveItem<DockItem> dockItem)
            {
                dockItem.MakeActive();
            }
        }

        private void _singleSelectionBehavior_PropertyChanged
        (
            object? sender, 
            PropertyChangedEventArgs e)
        {
            SetSelectedItem();
        }

        private IDisposable? _isActiveBinding = null;
        private void SetSelectedItem()
        {
            _isActiveBinding?.Dispose();

            SelectedItem = _singleSelectionBehavior.TheSelectedItem;

            if (SelectedItem is DockItem dockItem)
            {
                Binding binding = new Binding();
                binding.Source = dockItem;
                binding.Path = "IsActive";
                binding.Mode = BindingMode.OneWay;
                _isActiveBinding = this.Bind(IsActiveProperty, binding);
            }
        }

        public void ClearSelectedItem()
        {
            _singleSelectionBehavior.TheSelectedItem = null!;
        }

        public void ClearSelfOnRemove()
        {
            ClearSelectedItem();
            this.TheDockManager = null;
        }

        public void SelectFirst()
        {
            SelectFirstVisibleChild();
        }


        #region SelectedItem Styled Avalonia Property
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            private set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly StyledProperty<object> SelectedItemProperty =
            AvaloniaProperty.Register<TabbedDockGroup, object>
            (
                nameof(SelectedItem)
            );
        #endregion SelectedItem Styled Avalonia Property

        #region TabStripPlacement Styled Avalonia Property
        public Dock TabStripPlacement
        {
            get { return GetValue(TabStripPlacementProperty); }
            set { SetValue(TabStripPlacementProperty, value); }
        }

        public static readonly StyledProperty<Dock> TabStripPlacementProperty =
            AvaloniaProperty.Register<TabbedDockGroup, Dock>
            (
                nameof(TabStripPlacement),
                Dock.Top
            );
        #endregion TabStripPlacement Styled Avalonia Property


        #region TabOrientation Direct Avalonia Property
        private Orientation _TabOrientation = Orientation.Vertical;

        public static readonly DirectProperty<TabbedDockGroup, Orientation> TabOrientationProperty =
            AvaloniaProperty.RegisterDirect<TabbedDockGroup, Orientation>
            (
                nameof(TabOrientation),
                o => o.TabOrientation,
                (o, v) => o.TabOrientation = v
            );

        public Orientation TabOrientation
        {
            get => _TabOrientation;
            private set
            {
                SetAndRaise(TabOrientationProperty, ref _TabOrientation, value);
            }
        }

        #endregion TabOrientation Direct Avalonia Property


        #region HorizontalContentAlignment Styled Avalonia Property
        public HorizontalAlignment HorizontalContentAlignment
        {
            get { return GetValue(HorizontalContentAlignmentProperty); }
            set { SetValue(HorizontalContentAlignmentProperty, value); }
        }

        public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
            AvaloniaProperty.Register<TabbedDockGroup, HorizontalAlignment>
            (
                nameof(HorizontalContentAlignment)
            );
        #endregion HorizontalContentAlignment Styled Avalonia Property


        #region VerticalContentAlignment Styled Avalonia Property
        public VerticalAlignment VerticalContentAlignment
        {
            get { return GetValue(VerticalContentAlignmentProperty); }
            set { SetValue(VerticalContentAlignmentProperty, value); }
        }

        public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
            AvaloniaProperty.Register<TabbedDockGroup, VerticalAlignment>
            (
                nameof(VerticalContentAlignment)
            );
        #endregion VerticalContentAlignment Styled Avalonia Property

        #region DefaultDockOrderInGroup Styled Avalonia Property
        public double DefaultDockOrderInGroup
        {
            get { return GetValue(DefaultDockOrderInGroupProperty); }
            set { SetValue(DefaultDockOrderInGroupProperty, value); }
        }

        public static readonly StyledProperty<double> DefaultDockOrderInGroupProperty =
            AvaloniaProperty.Register<TabbedDockGroup, double>
            (
                nameof(DefaultDockOrderInGroup)
            );
        #endregion DefaultDockOrderInGroup Styled Avalonia Property


        IDisposable? _behavior;
        private void SetBehavior()
        {
            if (Items != null)
            {
                _setItemsBehavior = new SetDockGroupBehavior(this, Items!);
            }

            _mimicCollectionBehavior.InputCollection = Items;
            _singleSelectionBehavior.TheCollection = _mimicCollectionBehavior.OutputCollection;

            _behavior = Items?.AddBehavior(OnItemAdded, OnItemRemoved);
        }


        private void OnItemAdded(IDockGroup child)
        {
            this.SetIsDockVisible();
            child.IsDockVisibleChangedEvent += OnChildDockVisibleChanged;

            SelectFirstVisibleChildIfNoSelection();

            this.FireChangeWithin();
            this.SubscribeToChildChange(child);
        }

        private void OnChildDockVisibleChanged(IDockGroup child)
        {
            this.FireChangeWithin();
            if (child is DockItem dockItemChild)
            {
                if (!child.IsDockVisible && dockItemChild.IsSelected)
                {
                    dockItemChild.IsSelected = false;
                }

                SelectFirstVisibleChildIfNoSelection();
            }

            this.SetIsDockVisible();
        }

        private void SelectFirstVisibleChild()
        {
            DockItem? firstVisibleChild =
                Items?.OfType<DockItem>()?.FirstOrDefault(item => item.GetIsDockVisible());

            if (firstVisibleChild != null)
            {
                firstVisibleChild.IsSelected = true;
            }
        }

        private void SelectFirstVisibleChildIfNoSelection()
        {
            if (SelectedItem == null)
            {
                SelectFirstVisibleChild();
            }    
        }

        private void OnItemRemoved(IDockGroup child)
        {
            this.UnsubscribeFromChildChange(child);
            this.FireChangeWithin();
            if (child is DockItem dockItemChild )
            {
                if (dockItemChild.IsSelected)
                {
                    dockItemChild.IsSelected = false;
                }
            }

            SelectFirstVisibleChildIfNoSelection();
            child.IsDockVisibleChangedEvent -= OnChildDockVisibleChanged;
            this.SetIsDockVisible();
        }

        private void DisposeBehavior()
        {
            _setItemsBehavior?.Dispose();

            _setItemsBehavior = null;

            _singleSelectionBehavior.TheCollection = null;

            _mimicCollectionBehavior.InputCollection = null;
        }

        public IDockGroup? GetContainingGroup() => this;

        public bool AutoDestroy
        {
            get; set;
        } = true;

        void IDockGroup.SimplifySelf()
        {
            this.SimplifySelfImpl();
        }

        #region TabSeparatorBackground Styled Avalonia Property
        public IBrush TabSeparatorBackground
        {
            get { return GetValue(TabSeparatorBackgroundProperty); }
            set { SetValue(TabSeparatorBackgroundProperty, value); }
        }

        public static readonly StyledProperty<IBrush> TabSeparatorBackgroundProperty =
            AvaloniaProperty.Register<TabbedDockGroup, IBrush>
            (
                nameof(TabSeparatorBackground)
            );
        #endregion TabSeparatorBackground Styled Avalonia Property


        #region AllowCenterDocking Styled Avalonia Property
        public bool AllowCenterDocking
        {
            get { return GetValue(AllowCenterDockingProperty); }
            set { SetValue(AllowCenterDockingProperty, value); }
        }

        public static readonly AttachedProperty<bool> AllowCenterDockingProperty =
            DockAttachedProperties.AllowCenterDockingProperty.AddOwner<TabbedDockGroup>();
        #endregion AllowCenterDocking Styled Avalonia Property


        #region AllowTabDocking Styled Avalonia Property
        public bool AllowTabDocking
        {
            get { return GetValue(AllowTabDockingProperty); }
            set { SetValue(AllowTabDockingProperty, value); }
        }

        public static readonly StyledProperty<bool> AllowTabDockingProperty =
            AvaloniaProperty.Register<TabbedDockGroup, bool>
            (
                nameof(AllowTabDocking),
                true
            );
        #endregion AllowTabDocking Styled Avalonia Property


        #region CanFloat Styled Avalonia Property
        public bool CanFloat
        {
            get { return GetValue(CanFloatProperty); }
            set { SetValue(CanFloatProperty, value); }
        }

        public static readonly StyledProperty<bool> CanFloatProperty =
            DockAttachedProperties.CanFloatProperty.AddOwner<TabbedDockGroup>();
        #endregion CanFloat Styled Avalonia Property


        #region CanClose Styled Avalonia Property
        public bool CanClose
        {
            get { return GetValue(CanCloseProperty); }
            set { SetValue(CanCloseProperty, value); }
        }

        public static readonly StyledProperty<bool> CanCloseProperty =
            DockAttachedProperties.CanCloseProperty.AddOwner<TabbedDockGroup>();
        #endregion CanClose Styled Avalonia Property
    }
}
