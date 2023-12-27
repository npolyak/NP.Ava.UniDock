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
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using NP.Avalonia.UniDockService;
using NP.Avalonia.Visuals.Behaviors;
using NP.Concepts.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NP.Avalonia.UniDock
{
    public class DockItem :
        DockGroupBaseControl, 
        ILeafDockObj,
        ISelectableItem<DockItem>,
        IActiveItem<DockItem>,
        IRecyclingTemplateContainer
    {
        public event Action<IDockGroup>? IsDockVisibleChangedEvent;
        
        IDictionary<IDockGroup, IDisposable> IDockGroup.ChildSubscriptions => 
            throw new NotImplementedException();

        // IsActive in current top level (root) group (or in floating window) changed
        public event Action<DockItem>? IsActiveChanged;

        public GroupKind TheGroupKind => GroupKind.DockItem;

        public override bool HasStableDescendant => false;

        #region IsActive Styled Avalonia Property
        /// <summary>
        /// IsActive in current top level (root) group (or in floating window)
        /// </summary>
        public bool IsActive
        {
            get { return GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<DockItem, bool>
            (
                nameof(IsActive)
            );
        #endregion IsActive Styled Avalonia Property

        public IRecyclingDataTemplate? RecyclingDataTemplate { get; set; }


        IControl? _oldChild = null;
        public IControl? OldChild 
        { 
            get => _oldChild; 
            set
            {
                if (_oldChild == value)
                    return;

                var oldChild = _oldChild;
                _oldChild = value;

                if (oldChild is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        void IDockGroup.FireIsDockVisibleChangedEvent()
        {
            IsDockVisibleChangedEvent?.Invoke(this);

            IDockGroup topDockGroup = this.GetDockGroupRoot();
            DockStaticEvents.FirePossibleDockChangeHappenedInsideEvent(topDockGroup);

            this.FireChangeWithin();
        }


        public bool IsPredefined { get; set; } = true;

        public bool AutoInvisible { get => false; set { } }

        #region CanFloat Styled Avalonia Property
        public bool CanFloat
        {
            get { return GetValue(CanFloatProperty); }
            set { SetValue(CanFloatProperty, value); }
        }

        public static readonly StyledProperty<bool> CanFloatProperty =
            DockAttachedProperties.CanFloatProperty.AddOwner<DockItem>();
        #endregion CanFloat Styled Avalonia Property


        #region CanClose Styled Avalonia Property
        public bool CanClose
        {
            get { return GetValue(CanCloseProperty); }
            set { SetValue(CanCloseProperty, value); }
        }

        public static readonly StyledProperty<bool> CanCloseProperty =
            DockAttachedProperties.CanCloseProperty.AddOwner<DockItem>();
        #endregion CanClose Styled Avalonia Property

        static DockItem()
        {
            IsSelectedProperty
                .Changed
                .Subscribe(OnIsSelectedChanged);
        }

        private DockManager? _prevDockManager = null;

        public DockItem()
        {
            this.GetObservable(IsActiveProperty)
                .Subscribe(OnIsActiveInWindowChanged);

            this.GetObservable(DockAttachedProperties.IsDockVisibleProperty)
                .Subscribe(OnIsDockVisibleChanged);

            this.GetObservable(ContentTemplateResourceKeyProperty)
                .Subscribe(OnContentTemplateResourceKeyChanged!);

            this.GetObservable(HeaderContentTemplateResourceKeyProperty)
                .Subscribe(OnHeaderContentTemplateResourceKeyChanged!);

            this.GetObservable(DockAttachedProperties.TheDockManagerProperty)
                .Subscribe(OnDockManagerChanged);
        }

        public event Action<DockItem> DockItemDestroyedEvent;

        private void OnDockManagerChanged(DockManager dockManager)
        {
            if (dockManager == null && _prevDockManager != null)
            {
                DockItemDestroyedEvent?.Invoke(this);
            }

            _prevDockManager = dockManager;
        }

        ~DockItem()
        {
            RecyclingDataTemplate = null;

            OldChild = null;
        }

        private void OnHeaderContentTemplateResourceKeyChanged(string newResourceKey)
        {
            if (DockParent != null)
            {
                if (DockParent?.IsAttachedToLogicalTree == true)
                {
                    TrySetHeaderContentTemplate();
                }
            }
        }

        private void OnContentTemplateResourceKeyChanged(string newResourceKey)
        {
            if (DockParent != null)
            {
                if (DockParent?.IsAttachedToLogicalTree == true)
                {
                    TrySetContentTemplate();
                }
            }
        }


        #region HeaderContentTemplateResourceKey Styled Avalonia Property
        public string? HeaderContentTemplateResourceKey
        {
            get { return GetValue(HeaderContentTemplateResourceKeyProperty); }
            set { SetValue(HeaderContentTemplateResourceKeyProperty, value); }
        }

        public static readonly StyledProperty<string?> HeaderContentTemplateResourceKeyProperty =
            AvaloniaProperty.Register<DockItem, string?>
            (
                nameof(HeaderContentTemplateResourceKey)
            );
        #endregion HeaderContentTemplateResourceKey Styled Avalonia Property


        #region ContentTemplateResourceKey Styled Avalonia Property
        public string? ContentTemplateResourceKey
        {
            get { return GetValue(ContentTemplateResourceKeyProperty); }
            set { SetValue(ContentTemplateResourceKeyProperty, value); }
        }

        public static readonly StyledProperty<string?> ContentTemplateResourceKeyProperty =
            AvaloniaProperty.Register<DockItem, string?>
            (
                nameof(ContentTemplateResourceKey)
            );
        #endregion ContentTemplateResourceKey Styled Avalonia Property


        private void OnIsDockVisibleChanged(bool isDockVisible)
        {
            if (!isDockVisible)
            {
                this.DropPanel?.FinishPointerDetection();
            }
        }

        private void OnIsActiveInWindowChanged(bool isActiveInWindow)
        {
            // I think no need to make selection follow IsActive. Everything seems to be workign without it.
            //if (IsActive)
            //{
            //    this.Select();
            //}

            IsActiveChanged?.Invoke(this);
        }

        #region IsSelectedProperty Direct Avalonia Property
        public static readonly DirectProperty<DockItem, bool> IsSelectedProperty =
            AvaloniaProperty.RegisterDirect<DockItem, bool>
            (
                nameof(IsSelected),
                o => o.IsSelected, 
                (o, v) => o.IsSelected = v
            );
        #endregion IsSelectedProperty Direct Avalonia Property

        public Point FloatingSize { get; set; } = new Point(700, 400);

        public DockItemPresenter? TheVisual { get; internal set; }

        public IControl GetVisual() => (TheVisual as IControl) ?? this;

        public IDockGroup? GetContainingGroup() => DockParent;

        private bool _isSelected = false;
        public bool IsSelected 
        {
            get => _isSelected;
            set
            {
                SetAndRaise(IsSelectedProperty, ref _isSelected, value);
            }
        }

        public DropPanelWithCompass? DropPanel =>
            this?.TheVisual?.GetDropPanel();

        public DockKind? CurrentGroupDock =>
            DropPanel?.DockSide;

        public DockManager? TheDockManager
        {
            get => DockAttachedProperties.GetTheDockManager(this);
            set => DockAttachedProperties.SetTheDockManager(this, value!);
        }

        private bool _isAtContentInitStage = true;

        private bool _isAtHeaderContentInitStage = true;

        private bool IsAtContentInitState =>
            _isAtContentInitStage &&
            DockParent != null &&
            ContentTemplateResourceKey != null &&
            ContentTemplate == null;

        private bool IsAtHeaderContentInitState =>
            _isAtHeaderContentInitStage &&
            DockParent != null &&
            HeaderContentTemplateResourceKey != null &&
            HeaderTemplate == null;

        protected override void OnDockParentChanged()
        {
            base.OnDockParentChanged();
            if ((this as ILogical).IsAttachedToLogicalTree)
            {
                if (IsAtHeaderContentInitState)
                {
                    TrySetHeaderContentTemplate();
                }
                if (IsAtContentInitState)
                {
                    TrySetContentTemplate();
                }
            }
            else if (DockParent != null)
            {
                DockParent!.AttachedToLogicalTree += _dockParent_AttachedToLogicalTree;
            }
        }

        private void TrySetContentTemplate()
        {
            if (IsAtContentInitState)
            {
                var dataTemplate = 
                    DockParent.GetResource<DataTemplate>(ContentTemplateResourceKey!);
                
                if (dataTemplate != null)
                {
                    ContentTemplate = dataTemplate;

                    _isAtContentInitStage = false;
                }
            }
        }

        private void TrySetHeaderContentTemplate()
        {
            if (IsAtHeaderContentInitState)
            {
                var dataTemplate =
                    DockParent.GetResource<DataTemplate>(HeaderContentTemplateResourceKey!);

                if (dataTemplate != null)
                {
                    HeaderTemplate = dataTemplate;

                    _isAtHeaderContentInitStage = false;
                }
            }
        }

        private void _dockParent_AttachedToLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            TrySetHeaderContentTemplate();
            TrySetContentTemplate();

            if (DockParent != null)
            {
                DockParent.AttachedToLogicalTree -= _dockParent_AttachedToLogicalTree;
                DockParent.AttachedToLogicalTree -= _dockParent_AttachedToLogicalTree;
            }
        }


        #region Content Styled Avalonia Property
        [DependsOn(nameof(ContentTemplate))]
        [Content]
        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly StyledProperty<object> ContentProperty =
            AvaloniaProperty.Register<DockItem, object>
            (
                nameof(Content)
            );
        #endregion Content Styled Avalonia Property


        #region ContentTemplate Styled Avalonia Property
        public DataTemplate ContentTemplate
        {
            get { return GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }

        public static readonly StyledProperty<DataTemplate> ContentTemplateProperty =
            AvaloniaProperty.Register<DockItem, DataTemplate>
            (
                nameof(ContentTemplate)
            );
        #endregion ContentTemplate Styled Avalonia Property


        #region Header Styled Avalonia Property
        [DependsOn(nameof(HeaderTemplate))]
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly StyledProperty<object> HeaderProperty =
            AvaloniaProperty.Register<DockItem, object>
            (
                nameof(Header)
            );
        #endregion Header Styled Avalonia Property


        #region HeaderTemplate Styled Avalonia Property
        public DataTemplate HeaderTemplate
        {
            get { return GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        public static readonly StyledProperty<DataTemplate> HeaderTemplateProperty =
            AvaloniaProperty.Register<DockItem, DataTemplate>
            (
                nameof(HeaderTemplate)
            );
        #endregion HeaderTemplate Styled Avalonia Property


        // dock item is the end item, so it has no dock children.
        public IList<IDockGroup>? DockChildren => null;

        public event Action<IRemovable>? RemoveEvent;
        public event Action<DockItem>? IsSelectedChanged;

        public bool AutoDestroy
        {
            get => false;
            set
            {

            }
        }

        public void Remove()
        {
            IDockGroup? parent = DockParent;

            IDockGroup topDockGroup = this.GetDockGroupRoot();

            RemoveEvent?.Invoke(this);

            parent?.Simplify();

            DockStaticEvents.FirePossibleDockChangeHappenedInsideEvent(topDockGroup);
        }

        public void Select()
        {
            IsSelected = true;
        }


        const string NO_HEADER = "NO_HEADER";

        public override string ToString() =>
            $"TheDockItem: {DockId} DockDataContext:{DockDataContext} {Header?.ToString()} Active?{IsActive} Selected?{IsSelected} { HeaderTemplate?.ToString()?? NO_HEADER} {Content?.ToString()}";

        private void FireSelectionChanged()
        {
            IsSelectedChanged?.Invoke(this);
        }

        private static void OnIsSelectedChanged(AvaloniaPropertyChangedEventArgs<bool> change)
        {
            DockItem dockItem = (DockItem)change.Sender;

            dockItem.OnIsSelectedChanged();
        }

        private void OnIsSelectedChanged()
        {
            if (IsSelected)
            {
                IsActive = true;
            }

            this.FireSelectionChanged();
        }


        public void CleanSelfOnRemove()
        {
            this.DropPanel?.FinishPointerDetection();

            if (this.DropPanel != null)
            {
                this.DropPanel.CanStartPointerDetection = false;
            }

            if (Header is IControl headerControl)
            {
                headerControl.DisconnectVisualParentContentPresenter();
            }

            if (Content is IControl contentControl)
            {
                contentControl.DisconnectVisualParentContentPresenter();
            }

            //this.TheDockManager = null;

            if (this.TheVisual != null)
            {
                TheVisual.DockContext = null;
                this.TheVisual = null;
            }

            IsSelected = false;
        }

        void IDockGroup.SimplifySelf()
        {

        }

        #region DefaultDockOrderInGroup Styled Avalonia Property
        public double DefaultDockOrderInGroup
        {
            get { return GetValue(DefaultDockOrderInGroupProperty); }
            set { SetValue(DefaultDockOrderInGroupProperty, value); }
        }

        public static readonly StyledProperty<double> DefaultDockOrderInGroupProperty =
            AvaloniaProperty.Register<DockItem, double>
            (
                nameof(DefaultDockOrderInGroup)
            );
        #endregion DefaultDockOrderInGroup Styled Avalonia Property

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

        #region AllowCenterDocking Styled Avalonia Property
        public bool AllowCenterDocking
        {
            get { return GetValue(AllowCenterDockingProperty); }
            set { SetValue(AllowCenterDockingProperty, value); }
        }

        public static readonly AttachedProperty<bool> AllowCenterDockingProperty =
            DockAttachedProperties.AllowCenterDockingProperty.AddOwner<DockItem>();
        #endregion AllowCenterDocking Styled Avalonia Property
    }
}
