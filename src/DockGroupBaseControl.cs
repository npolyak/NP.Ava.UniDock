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
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Subjects;

namespace NP.Avalonia.UniDock
{
    public class DockGroupBaseControl : TemplatedControl, IDockDataContextContainer
    {
        public event Action<IDockGroup>? DockIdChanged;

        #region DockId Styled Avalonia Property
        public string DockId
        {
            get { return GetValue(DockIdProperty); }
            set { SetValue(DockIdProperty, value); }
        }

        public static readonly StyledProperty<string> DockIdProperty =
            AvaloniaProperty.Register<DockGroupBaseControl, string>
            (
                nameof(DockId)
            );
        #endregion Id Styled Avalonia Property

        private void FireDockIdChanged()
        {
            DockIdChanged?.Invoke((IDockGroup) this);
        }

        protected void OnDockIdChanged(AvaloniaPropertyChangedEventArgs e)
        {
            FireDockIdChanged();
        }

        public override string ToString() => DockId;

        #region ShowCompass Styled Avalonia Property
        public bool ShowCompass
        {
            get { return GetValue(ShowCompassProperty); }
            set { SetValue(ShowCompassProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowCompassProperty =
            DockAttachedProperties.ShowCompassProperty.AddOwner<TabbedDockGroup>();
        #endregion ShowCompass Styled Avalonia Property

        public IDictionary<IDockGroup, IDisposable> ChildSubscriptions { get; } =
            new Dictionary<IDockGroup, IDisposable>();

        public Subject<Unit> DockChangedWithin { get; } = new Subject<Unit>();


        #region HasStableChild Direct Avalonia Property
        private bool _HasStableChild = default;

        public static readonly DirectProperty<DockGroupBaseControl, bool> HasStableChildProperty =
            AvaloniaProperty.RegisterDirect<DockGroupBaseControl, bool>
            (
                nameof(HasStableDescendant),
                o => o.HasStableDescendant,
                (o, v) => o.HasStableDescendant = v
            );

        public virtual bool HasStableDescendant
        {
            get => _HasStableChild;
            protected set
            {
                SetAndRaise(HasStableChildProperty, ref _HasStableChild, value);
            }
        }

        #endregion HasStableChild Direct Avalonia Property


        #region CanReattachToDefaultGroup Styled Avalonia Property
        public bool CanReattachToDefaultGroup
        {
            get { return GetValue(CanReattachToDefaultGroupProperty); }
            set { SetValue(CanReattachToDefaultGroupProperty, value); }
        }

        public static readonly AttachedProperty<bool> CanReattachToDefaultGroupProperty =
            DockAttachedProperties.CanReattachToDefaultGroupProperty.AddOwner<DockGroupBaseControl>();
        #endregion CanReattachToDefaultGroup Styled Avalonia Property


        #region GroupOnlyById Styled Avalonia Property
        public string? GroupOnlyById
        {
            get { return GetValue(GroupOnlyByIdProperty); }
            set { SetValue(GroupOnlyByIdProperty, value); }
        }

        public static readonly StyledProperty<string?> GroupOnlyByIdProperty =
            AvaloniaProperty.Register<DockGroupBaseControl, string?>
            (
                nameof(GroupOnlyById)
            );
        #endregion GroupOnlyById Styled Avalonia Property


        #region DockParent Styled Avalonia Property
        public virtual IDockGroup? DockParent
        {
            get { return GetValue(DockParentProperty); }
            set { SetValue(DockParentProperty, value); }
        }

        public static readonly StyledProperty<IDockGroup?> DockParentProperty =
            AvaloniaProperty.Register<DockGroupBaseControl, IDockGroup?>
            (
                nameof(DockParent)
            );
        #endregion DockParent Styled Avalonia Property

        #region ProducingUserDefinedWindowGroup Styled Avalonia Property
        public RootDockGroup? ProducingUserDefinedWindowGroup
        {
            get { return GetValue(ProducingUserDefinedWindowGroupProperty); }
            set { SetValue(ProducingUserDefinedWindowGroupProperty, value); }
        }

        public static readonly StyledProperty<RootDockGroup?> ProducingUserDefinedWindowGroupProperty =
            AvaloniaProperty.Register<DockGroupBaseControl, RootDockGroup?>
            (
                nameof(ProducingUserDefinedWindowGroup)
            );
        #endregion ProducingUserDefinedWindowGroup Styled Avalonia Property


        // transient value only needed to pass size coefficient during the drop
        // into a StackDockPanel
        public GridLength? InitialSizeCoeff { get; set; }

        static DockGroupBaseControl()
        {
            DockIdProperty
                .Changed
                .AddClassHandler<DockGroupBaseControl>((g, e) => g.OnDockIdChanged(e));
        }

        private IDisposable _subscription;
        public DockGroupBaseControl()
        {
            _subscription = DockChangedWithin.Subscribe(OnDockChangedWithin);

            this.GetObservable(DockParentProperty).Subscribe(OnDockParentChangedImpl!);

            this.GetObservable(ProducingUserDefinedWindowGroupProperty)
                .Subscribe(OnProducingUserDefinedWindowGroupPropertyChanged!);
        }

        private void OnProducingUserDefinedWindowGroupPropertyChanged(RootDockGroup obj)
        {
            this.SetDockDataContextBinding();
        }

        private void OnDockParentChangedImpl(IDockGroup dockParent)
        {
            OnDockParentChanged();
            ((IDockGroup)this).SetCanReattachToDefaultGroup();
        }

        protected virtual void OnDockParentChanged()
        {
            
        }

        private void OnDockChangedWithin(Unit _)
        {
            HasStableDescendant = ((IDockGroup)this).HasStableGroup();
        }


        private IDisposable? _dockDataContextSubscription = null;
        private Binding? _dockDataContextBinding;
        public Binding? DockDataContextBinding
        {
            get => _dockDataContextBinding;

            set
            {
                if (_dockDataContextBinding == value)
                {
                    return;
                }
                BreakDockDataContextBinding();
                _dockDataContextBinding = value;

                SetDockDataContextBinding();
            }
        }

        private void BreakDockDataContextBinding()
        {
            _dockDataContextSubscription?.Dispose();
            _dockDataContextSubscription = null;
        }


        #region DockDataContext Styled Avalonia Property
        public object? DockDataContext
        {
            get { return GetValue(DockDataContextProperty); }
            internal set { SetValue(DockDataContextProperty, value); }
        }

        public static readonly StyledProperty<object?> DockDataContextProperty =
            AvaloniaProperty.Register<DockGroupBaseControl, object?>
            (
                nameof(DockDataContext)
            );
        #endregion DockDataContext Styled Avalonia Property

        private void SetDockDataContextBinding()
        {
            BreakDockDataContextBinding();
            if ((_dockDataContextBinding == null) ||
                (ProducingUserDefinedWindowGroup == null))
            {
                return;
            }

            _dockDataContextSubscription =
                this.Bind
                (
                    DockDataContextProperty,
                    _dockDataContextBinding,
                    ProducingUserDefinedWindowGroup);
        }
    }
}
