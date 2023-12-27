using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml.Templates;
using NP.Avalonia.UniDockService;
using NP.Concepts.Behaviors;
using NP.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace NP.Avalonia.UniDock
{
    public class DataItemsViewModelBehavior : VMBase
    {
        internal event Action<DockItemViewModelBase> DockItemAddedEvent;

        internal event Action<DockItemViewModelBase> DockItemRemovedEvent;

        internal event Action<DockItemViewModelBase> DockItemSelectionChangedEvent;

        private IDisposable? _viewModelsBehavior;
        #region DockItemsViewModels Property
        private ObservableCollection<DockItemViewModelBase>? _dockItemsViewModels;
        public ObservableCollection<DockItemViewModelBase>? DockItemsViewModels
        {
            get
            {
                return this._dockItemsViewModels;
            }
            set
            {
                if (this._dockItemsViewModels == value)
                {
                    return;
                }

                _viewModelsBehavior?.Dispose();

                this._dockItemsViewModels = value;

                _viewModelsBehavior =
                    _dockItemsViewModels?.AddBehavior
                    (
                        OnGroupViewModelAdded,
                        OnGroupViewModelRemoved);

                this.OnPropertyChanged(nameof(DockItemsViewModels));
            }
        }
        #endregion DockItemsViewModels Property



        private DockItem CreateDockItemFromVm(IDockItemViewModel vm)
        {
            DockItem? dockItem =
                _dockManager.FindGroupById(vm.DockId) as DockItem;

            if (dockItem != null)
            {
                vm.IsDockVisible = (dockItem as IDockGroup).IsDockVisible;
            }
            else
            {
                dockItem = new DockItem
                {
                    DockId = vm.DockId!,
                    DefaultDockGroupId = vm.DefaultDockGroupId!,
                    DefaultDockOrderInGroup = vm.DefaultDockOrderInGroup,
                    GroupOnlyById = vm.GroupOnlyById,
                    CanFloat = vm.CanFloat,
                    CanClose = vm.CanClose,
                    IsPredefined = vm.IsPredefined
                };
            }

            dockItem.DataContext = vm;

            if (vm.Header != null)
            {
                dockItem.Header = vm.Header;
            }

            if (vm.Content != null)
            {
                dockItem.Content = vm.Content;
            }

            void AddBind<T>
            (
                AvaloniaProperty<T> prop,
                string propName,
                BindingMode bindingMode = BindingMode.TwoWay)
            {
                Binding binding = new Binding(propName);
                binding.Mode = bindingMode;
                dockItem.Bind(prop, binding);
            }

            AddBind(DockAttachedProperties.IsDockVisibleProperty, "IsDockVisible");
            AddBind(DockItem.IsSelectedProperty, "IsSelected");
            AddBind(DockItem.IsActiveProperty, "IsActive");

            if (dockItem.TheDockManager == null)
            {
                dockItem.TheDockManager = _dockManager;
            }

            return dockItem;
        }

        private void SetDockItemFromVm
        (
            IDockItemViewModel vm, 
            IDockGroup dockGroup)
        {
            DockItem? dockItem =
                _dockManager.FindGroupById(vm.DockId) as DockItem;

            if (dockItem == null)
            {
                return;
            }

            dockItem.ProducingUserDefinedWindowGroup = dockGroup.ProducingUserDefinedWindowGroup;

            if (vm.HeaderContentTemplateResourceKey != null)
            {
                dockItem.HeaderTemplate =
                    dockGroup.GetResource<DataTemplate>(vm.HeaderContentTemplateResourceKey)!;
            }

            if (vm.ContentTemplateResourceKey != null)
            {
                dockItem.ContentTemplate =
                    dockGroup.GetResource<DataTemplate>(vm.ContentTemplateResourceKey)!;
            }

            if (dockItem.DockParent == null)
            {
                dockItem.SetCanReattachToDefaultGroup();
                dockItem.ReattachToDefaultGroup();
            }
        }

        private void ItemVm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DockItemViewModelBase.IsSelected))
            {
                DockItemSelectionChangedEvent?.Invoke((DockItemViewModelBase)sender!);
            }
        }

        private void OnGroupViewModelAdded(DockItemViewModelBase vm)
        {
            DockItem dockItem = CreateDockItemFromVm(vm);

            string groupId = vm.DefaultDockGroupId!;

            vm.PropertyChanged += ItemVm_PropertyChanged;

            DockItemAddedEvent?.Invoke(vm);

            IDockGroup? dockGroup =
                _dockManager.AllGroupsBehavior.Result.FirstOrDefault(g => g.DockId == groupId);

            if (dockGroup != null)
            {
                SetDockItemFromVm(vm, dockGroup);
            }
        }

        private void OnGroupViewModelRemoved(DockItemViewModelBase dockItemViewModel)
        {
            var dockItem = _dockManager.FindGroupById(dockItemViewModel.DockId);

            dockItem?.Remove();

            dockItemViewModel.IsSelected = false;
            DockItemRemovedEvent?.Invoke(dockItemViewModel);

            dockItemViewModel.PropertyChanged -= ItemVm_PropertyChanged;
        }

        internal void OnGroupItemAdded(IDockGroup addedGroup)
        {
            var dockItems =
                _dockManager.AllGroupsBehavior
                            .Result
                            .OfType<DockItem>()
                            .Where(item => item.DefaultDockGroupId == addedGroup.DockId)
                            .ToList();

            if (dockItems != null)
            {
                foreach(var dockItem in dockItems)
                {
                    dockItem.ReattachToDefaultGroup();
                }
            }
        }


        internal void OnGroupItemRemoved(IDockGroup removedGroup)
        {
            string? dockId = removedGroup.DockId;

            var dockItemViewModel = _dockItemsViewModels?.SingleOrDefault(item => item.DockId == dockId);

            if (dockItemViewModel != null)
            {
                _dockItemsViewModels?.Remove(dockItemViewModel);
            }
        }

        private DockManager _dockManager;
        public DataItemsViewModelBehavior(DockManager dockManager)
        {
            _dockManager = dockManager;

            _dockManager.AfterGroupItemAdded = OnGroupItemAdded;
            _dockManager.AfterGroupItemRemoved = OnGroupItemRemoved;
        }
    }
}
