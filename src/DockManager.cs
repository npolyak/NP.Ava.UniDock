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
using NP.Avalonia.Visuals.Behaviors;
using System.Collections.Generic;
using System.Linq;
using System;
using NP.Avalonia.Visuals;
using NP.Utilities;
using Avalonia.VisualTree;
using Avalonia.Layout;
using System.Collections.ObjectModel;
using NP.Concepts.Behaviors;
using System.ComponentModel;
using System.IO;
using NP.Avalonia.UniDock.Serialization;
using NP.Avalonia.UniDock.Factories;
using NP.Avalonia.UniDockService;
using NP.DependencyInjection.Attributes;

namespace NP.Avalonia.UniDock
{
    public class DockManager : VMBase, IUniDockService
    {
        public event Action<DockItemViewModelBase> DockItemAddedEvent;

        public event Action<DockItemViewModelBase> DockItemRemovedEvent;

        public event Action<DockItemViewModelBase> DockItemSelectionChangedEvent;

        // To be used in the future when multiple DockManagers become available
        public string? Id { get; set; }

        public bool ResizePreview
        {
            get => TheDockSeparatorFactory!.ResizePreview;
            set => TheDockSeparatorFactory!.ResizePreview = value;   
        }

        [Inject]
        private IStackGroupFactory StackGroupFactory { get; set; } =
            new StackGroupFactory();

        [Inject]
        private ITabbedGroupFactory TabbedGroupFactory { get; set; } =
            new TabbedGroupFactory();

        [Inject]
        internal IFloatingWindowFactory FloatingWindowFactory { get; set; } =
            new FloatingWindowFactory();

        [Inject]
        internal IDockVisualItemGenerator? TheDockVisualItemGenerator { get; set; } =
            new DockVisualItemGenerator();

        [Inject]
        internal IDockSeparatorFactory? TheDockSeparatorFactory { get; set; } =
            new DockSeparatorFactory();


        #region IsInEditableState Property
        private bool _isInEditableState = false;
        public bool IsInEditableState
        {
            get
            {
                return this._isInEditableState;
            }
            set
            {
                if (this._isInEditableState == value)
                {
                    return;
                }

                this._isInEditableState = value;
                this.OnPropertyChanged(nameof(IsInEditableState));
            }
        }
        #endregion IsInEditableState Property

        private DataItemsViewModelBehavior _dataItemsViewModelBehavior;

        public ObservableCollection<DockItemViewModelBase>? DockItemsViewModels
        {
            get => _dataItemsViewModelBehavior.DockItemsViewModels;
            set
            {
                if (DockItemsViewModels == value)
                    return;

                _dataItemsViewModelBehavior.DockItemsViewModels = value;

                OnPropertyChanged(nameof(DockItemsViewModels));
            }
        }

        public void SaveViewModelsToFile(string filePath)
        {
            if (DockItemsViewModels == null)
            {
                return;
            }

            Type[] types =
                DockItemsViewModels.Select(item => item.GetType()).Distinct().ToArray();

            DockItemsViewModels?.SerializeToFile(filePath, types);
        }

        public void SaveViewModelsToStream(Stream stream)
        {
            if (DockItemsViewModels == null)
            {
                return;
            }

            Type[] types =
                DockItemsViewModels.Select(item => item.GetType()).Distinct().ToArray();

            DockItemsViewModels?.SerializeToStream(stream, types);
        }


        public void RestoreViewModelsFromFile(string filePath, params Type[] extraTypes)
        {
            this.DockItemsViewModels =
                XmlSerializationUtils
                    .DeserializeFromFile<ObservableCollection<DockItemViewModelBase>>(filePath, false, extraTypes);

            this.SelectTabsInTabbedGroupsWithoutSelection();
        }

        public void RestoreViewModelsFromStream(Stream stream, params Type[] extraTypes)
        {
            this.DockItemsViewModels =
                XmlSerializationUtils
                    .DeserializeFromStream<ObservableCollection<DockItemViewModelBase>>(stream, false, extraTypes);

            this.SelectTabsInTabbedGroupsWithoutSelection();
        }

        private readonly IList<Window> _predefinedWindows = new ObservableCollection<Window>();
        public IEnumerable<Window> PredefinedWindows => _predefinedWindows;

        private readonly IList<FloatingWindow> _floatingWindows = new ObservableCollection<FloatingWindow>();
        public IEnumerable<FloatingWindow> FloatingWindows => _floatingWindows;

        private UnionBehavior<Window> _allWindowsBehavior;
        internal void AddWindow(Window window)
        {
            if (window is FloatingWindow floatingWindow)
            {
                _floatingWindows.Add(floatingWindow);
            }
            else
            {
                _predefinedWindows.Add(window);
            }
        }

        internal void RemoveWindow(Window window)
        {
            if (window is FloatingWindow floatingWindow)
            {
                _floatingWindows.Remove(floatingWindow);
            }
            else
            {
                _predefinedWindows.Remove(window);
            }
        }


        private IList<IDockGroup> _connectedGroups = new ObservableCollection<IDockGroup>();
        public IEnumerable<IDockGroup> ConnectedGroups => _connectedGroups;
        internal void AddConnectedGroup(IDockGroup group)
        {
            if (group.IsPredefined)
            {
                _disconnectedGroups.Remove(group);
            }

            _connectedGroups.Add(group);
        }

        internal void RemoveConnectedGroup(IDockGroup group)
        {
            _connectedGroups.Remove(group);

            if (group.IsPredefined)
            {
                _disconnectedGroups.Add(group);
            }
        }

        public DockObjectInfo? GetParentGroupInfo(string? dockId)
        {
            IDockGroup? dockGroup = AllGroups.FirstOrDefault(item => item.DockId == dockId);

            IDockGroup? parentDockGroup = dockGroup?.DockParent;

            return parentDockGroup?.ToDockObjectInfo();
        }

        private IList<IDockGroup> _disconnectedGroups = new ObservableCollection<IDockGroup>();

        public IEnumerable<IDockGroup> DisconnectedGroups => _disconnectedGroups;

        public IEnumerable<IDockGroup> AllGroups => _connectedGroups.Union(_disconnectedGroups);

        public UnionBehavior<IDockGroup> AllGroupsBehavior { get; }

        public IList<ILeafDockObj> DockLeafObjs { get; } =
            new List<ILeafDockObj>();

        public IEnumerable<ILeafDockObj> DockLeafObjsWithoutLeafParents =>
            DockLeafObjs.Where(leaf => !leaf.HasLeafAncestor()).ToList();

        private static bool IsGroupOperating(IDockGroup group)
        {
            var v = group.GetVisual();

            return v.IsVisible && v.IsAttachedToLogicalTree && v.GetControlsWindow<Window>().IsVisible;
        }

        public IEnumerable<RootDockGroup> AllOperatingRootDockGroups => 
            AllGroups.OfType<RootDockGroup>()
            .Where(g => IsGroupOperating(g)).ToList();

        private IEnumerable<IDockGroup> AllOperatingLeafGroupsWOLeafParents =>
            AllGroups
                .Where(g => !g.HasLeafAncestor()&& !g.HasLockedAncestor())
                .Where(g => IsGroupOperating(g))
                .Where(g => (g is TabbedDockGroup) || g.GetNumberChildren() == 0 || g.IsGroupLocked)
                .ToList();

        public void ClearGroups()
        {
            foreach(IDockGroup group in _connectedGroups.ToList())
            {
                if (!group.IsRoot)
                {
                    group.RemoveItselfFromParent();

                    if (!group.IsStableGroup)
                    {
                        group.TheDockManager = null;
                    }
                }
            }
        }

        FloatingWindow? _draggedWindow;
        public FloatingWindow? DraggedWindow
        {
            get => _draggedWindow;
            internal set
            {
                if (ReferenceEquals(_draggedWindow, value))
                    return;

                _draggedWindow = value;

                if (_draggedWindow != null)
                {
                    BeginDragAction();
                }
            }
        }

        private IList<(IDockGroup Group, Rect2D Rect)>? _currentDockGroups;
        private IList<(RootDockGroup Group, Rect2D Rect)>? _rootGroups;

        private static (T Group, Rect2D Rect) GroupToPair<T>(T group)
            where T : IDockGroup
        {
            return (group, group.GetVisual().GetScreenBounds());
        }

        IDisposable? _pointerMovedSubscription;
        private void BeginDragAction()
        {
            if (_draggedWindow == null)
                return;

            SetGroups();

            _pointerMovedSubscription = 
                CurrentScreenPointBehavior.CurrentScreenPoint
                                          .Subscribe(OnPointerMoved);
        }

        internal void SetGroups()
        {
            if (_draggedWindow == null)
            {
                return;
            }

            _currentDockGroups =
                AllOperatingLeafGroupsWOLeafParents
                    .Except(_draggedWindow.TheDockGroup.GetDockGroupSelfAndDescendants())
                    .Where(g => g.GroupOnlyById == _draggedWindow.GroupOnlyById)
                    .Select(g => GroupToPair(g)).ToList();

            bool hasAnyNonLockedLeafItems = _draggedWindow.GetLeafGroupsWithoutLock().Any();
            _currentDockGroups.DoForEach(g => g.Group.AllowCenterDocking = hasAnyNonLockedLeafItems);

            _rootGroups =
                AllOperatingRootDockGroups
                    .Except(_draggedWindow.TheDockGroup.ToCollection())
                    .Except(_currentDockGroups.OfType<RootDockGroup>())
                    .Where(g => g.GroupOnlyById == _draggedWindow.GroupOnlyById)
                    .Select(g => GroupToPair(g)).ToList();
        }

        /// <summary>
        /// group into which we insert dragged item(s)
        /// </summary>
        private IDockGroup? _currentLeafObjToInsertWithRespectTo = null;
        private IDockGroup? CurrentLeafObjToInsertWithRespectTo
        {
            get => _currentLeafObjToInsertWithRespectTo;

            set
            {
                if (ReferenceEquals(_currentLeafObjToInsertWithRespectTo, value))
                    return;

                if (_currentLeafObjToInsertWithRespectTo != null)
                {
                    _currentLeafObjToInsertWithRespectTo.ShowCompass = false;
                }

                _currentLeafObjToInsertWithRespectTo = value;

                if (_currentLeafObjToInsertWithRespectTo != null)
                {
                    _currentLeafObjToInsertWithRespectTo.ShowCompass = true;
                }
            }
        }

        private RootDockGroup? _currentRootDockGroup = null;
        private RootDockGroup? CurrentRootDockGroup
        {
            get => _currentRootDockGroup;

            set
            {
                if (_currentRootDockGroup == value)
                {
                    return;
                }

                if (_currentRootDockGroup != null)
                {
                    _currentRootDockGroup.ShowCompassCenter = true;
                    _currentRootDockGroup.ShowCompass = false;
                }

                _currentRootDockGroup = value;

                if (_currentRootDockGroup != null)
                {
                    _currentRootDockGroup.ShowCompassCenter = false;
                    _currentRootDockGroup.ShowCompass = true;
                }
            }
        }

        private void OnPointerMoved(Point2D pointerScreenLocation)
        {
            if (_currentDockGroups == null)
            {
                return;
            }

            var pointerAboveGroups =
                _currentDockGroups
                    .Where(gr => gr.Group.IsVisible && gr.Rect.ContainsPoint(pointerScreenLocation))
                    .Select(gr => gr.Group);

            CurrentLeafObjToInsertWithRespectTo = pointerAboveGroups.FirstOrDefault();

            var rootDockGroup = CurrentLeafObjToInsertWithRespectTo?.GetDockGroupRoot() as RootDockGroup; 
            if ((CurrentLeafObjToInsertWithRespectTo != null) &&
                (CurrentLeafObjToInsertWithRespectTo is not RootDockGroup) &&
                rootDockGroup?.GroupOnlyById == DraggedWindow?.GroupOnlyById)
            {
                CurrentRootDockGroup = rootDockGroup;
            }
            else
            {
                CurrentRootDockGroup = null;
            }
        }

        private void DropWithOrientation(IDockGroup? currentDockGroupToInsertWithRespectTo, DockKind dock, IDockGroup draggedGroup)
        {
            if (dock == DockKind.Center)
            {
                throw new Exception("Programming ERROR: dock should be one of Left, Top, Right, Bottom");
            }

            draggedGroup.RemoveItselfFromParent();

            IDockGroup? parentGroup = null;
            IDockGroup? childGroup = null;

            if (currentDockGroupToInsertWithRespectTo is RootDockGroup rootDockGroup)
            {
                parentGroup = currentDockGroupToInsertWithRespectTo;
                childGroup = rootDockGroup.TheChild!;

            }
            else
            {
                parentGroup = currentDockGroupToInsertWithRespectTo!.DockParent!;
                childGroup = currentDockGroupToInsertWithRespectTo!;
            }

            SetOrientationParentChildRelationship(parentGroup, childGroup, dock, draggedGroup);

            (draggedGroup.GetLeafItems().FirstOrDefault() as IActiveItem<DockItem>)?.MakeActive();

            DraggedWindow?.Close();
        }

        private void SetOrientationParentChildRelationship(IDockGroup parentGroup, IDockGroup childGroup, DockKind? dock, IDockGroup draggedGroup)
        {
            int childIdx =
                parentGroup
                    .DockChildren.IndexOf(childGroup);
            
            Orientation orientation = (Orientation)dock.ToOrientation()!;

            if (parentGroup is StackDockGroup parentDockStackGroup && parentDockStackGroup.TheOrientation == orientation)
            {
                GridLength childSizeCoeff = parentGroup.GetSizeCoeff(childIdx);

                // half of original child size
                GridLength newChildSizeCoeff = new GridLength(childSizeCoeff.Value/2d, childSizeCoeff.GridUnitType);
                
                // set it for the event handler that sets the child size
                draggedGroup.InitialSizeCoeff = newChildSizeCoeff;

                // set to the current child
                parentGroup.SetSizeCoeff(childIdx, newChildSizeCoeff);
                int insertIdx = childIdx.ToInsertIdx(dock);

                // insert the new child
                parentDockStackGroup.DockChildren.Insert(insertIdx, draggedGroup);
            }
            else // create an extra DockStackGroup insert the dragged object and the 
                 // the object it is dropped on (drop object) into that group and insert
                 // this new group in place of the drop object.
            {
                GridLength sizeCoeff = parentGroup.GetSizeCoeff(childIdx);

                childGroup.RemoveItselfFromParent();
                StackDockGroup insertGroup = StackGroupFactory.Create();

                insertGroup.GroupOnlyById = parentGroup.GroupOnlyById;

                insertGroup.TheOrientation = orientation;

                insertGroup.ProducingUserDefinedWindowGroup = parentGroup.ProducingUserDefinedWindowGroup;

                parentGroup.DockChildren.Insert(childIdx, insertGroup);

                int originalChildIdx = 0;

                insertGroup.DockChildren?.Insert(originalChildIdx, childGroup);

                int insertIdx = 0.ToInsertIdx(dock);

                insertGroup.DockChildren?.Insert(insertIdx, draggedGroup);

                if (insertIdx == 0)
                {
                    originalChildIdx = 1;
                }

                GridLength originalChildCoeff = insertGroup.GetSizeCoefficient(originalChildIdx);

                insertGroup.SetSizeCoeff(insertIdx, originalChildCoeff);

                parentGroup.SetSizeCoeff(childIdx, sizeCoeff);
            }
        }

        public ObservableCollection<Window> AllWindows => _allWindowsBehavior.Result;

        private readonly IDisposable? _groupsBehavior;
        private readonly IDisposable? _windowsBehavior;
        public DockManager()
        {
            _dataItemsViewModelBehavior = new DataItemsViewModelBehavior(this);
            _dataItemsViewModelBehavior.DockItemAddedEvent += _dataItemsViewModelBehavior_DockItemAddedEvent;
            _dataItemsViewModelBehavior.DockItemRemovedEvent += _dataItemsViewModelBehavior_DockItemRemovedEvent;
            _dataItemsViewModelBehavior.DockItemSelectionChangedEvent += _dataItemsViewModelBehavior_DockItemSelectionChangedEvent;

            DockGroupHelper.SetIsDockVisibleChangeSubscription();

            _allWindowsBehavior = new UnionBehavior<Window>(_predefinedWindows, _floatingWindows);

            _groupsBehavior = 
                ConnectedGroups.AddBehavior(OnGroupItemAdded, OnGroupItemRemoved);

            _windowsBehavior =
                _allWindowsBehavior.Result.AddBehavior(OnWindowItemAdded, OnWindowItemRemoved);

            AllGroupsBehavior = new UnionBehavior<IDockGroup>(_disconnectedGroups, _connectedGroups);
        }

        private void _dataItemsViewModelBehavior_DockItemAddedEvent(DockItemViewModelBase obj)
        {
            DockItemAddedEvent?.Invoke(obj);
        }

        private void _dataItemsViewModelBehavior_DockItemSelectionChangedEvent(DockItemViewModelBase obj)
        {
            DockItemSelectionChangedEvent?.Invoke(obj);
        }

        private void _dataItemsViewModelBehavior_DockItemRemovedEvent(DockItemViewModelBase obj)
        {
            DockItemRemovedEvent?.Invoke(obj);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // OnWindowClosed(sender as Window);
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            Window window = (Window)sender!;

            ClearWindowsGroups(window);
        }

        private void ClearWindowsGroups(Window window)
        {
            IEnumerable<IDockGroup> dockGroups;

            if (window is FloatingWindow dockWindow)
            {
                dockGroups = dockWindow.TheDockGroup.ToCollection();
            }
            else
            {
                dockGroups =
                    window.GetVisualDescendants()
                      .OfType<IDockGroup>()
                      .Where(group => group.IsRoot).ToList();
            }

            foreach (var group in dockGroups)
            {
                if (!group.IsStableGroup)
                {
                    group.TheDockManager = null;
                }
            }
        }

        private void OnWindowItemAdded(Window window)
        {
            //window.Closing += Window_Closing!;
            window.Closed += Window_Closed;

            string? windowId = DockAttachedProperties.GetWindowId(window);

            if (windowId == null)
            {
                string prefix = window.GetType().Name;
                windowId =
                    _windowIdGenerator.GetUniqueName
                    (
                        FloatingWindows.Except(window.ToCollection()).Select(w => DockAttachedProperties.GetWindowId(w)), prefix);

                DockAttachedProperties.SetWindowId(window, windowId);
            }
        }

        private void OnWindowItemRemoved(Window window)
        {
            window.Closing -= Window_Closing!;
        }

        private void VerifyDockIdUnique(IDockGroup group)
        {
            if (AllGroups.Count(g => g.DockId == group.DockId) > 1)
            {
                throw new Exception($"Programming Error - two or more groups cannot have the same Id {group.DockId}. Id should be unique within the DockManager.");
            }
        }

        private void AddedGroup_DockIdChanged(IDockGroup group)
        {
            VerifyDockIdUnique(group);
        }

        UniqueNameGeneratorWithMaxMemory _dockIdGenerator = new UniqueNameGeneratorWithMaxMemory();
        UniqueNameGeneratorWithMaxMemory _windowIdGenerator = new UniqueNameGeneratorWithMaxMemory();
        private void OnGroupItemAdded(IDockGroup addedGroup)
        {
            if (addedGroup.DockId == null)
            {
                string prefix = addedGroup.GetType().Name;

                addedGroup.DockId = _dockIdGenerator.GetUniqueName(AllGroups.Except(addedGroup.ToCollection()).Select(group => group.DockId), prefix);
            }
            else
            {
                VerifyDockIdUnique(addedGroup);
            }

            addedGroup.DockIdChanged += AddedGroup_DockIdChanged;

            AfterGroupItemAdded?.Invoke(addedGroup);
        }

        internal Action<IDockGroup>? AfterGroupItemAdded { get; set; }

        internal Action<IDockGroup>? AfterGroupItemRemoved { get; set; }

        private void OnGroupItemRemoved(IDockGroup removedGroup)
        {
            AfterGroupItemRemoved?.Invoke(removedGroup);

            removedGroup.DockIdChanged -= AddedGroup_DockIdChanged;
        }

        public void CompleteDragDropAction()
        {
            FloatingWindow? currentWindowToDropInto =  CurrentLeafObjToInsertWithRespectTo?.GetGroupWindow(); 

            try
            {
                _pointerMovedSubscription?.Dispose();
                _pointerMovedSubscription = null;

                IDockGroup? draggedGroup = DraggedWindow?.TheDockGroup?.TheChild;
                
                currentWindowToDropInto?.SetCloseIsNotAllowed();

                DraggedWindow?.SetCloseIsNotAllowed();

                IDockGroup? currentDockGroupToInsertWithRespectTo = CurrentLeafObjToInsertWithRespectTo;

                DockKind ? currentDock = CurrentLeafObjToInsertWithRespectTo?.CurrentGroupDock;

                if (currentDock == null)
                {
                    currentDockGroupToInsertWithRespectTo = CurrentRootDockGroup;

                    currentDock = currentDockGroupToInsertWithRespectTo?.CurrentGroupDock;
                }

                if (draggedGroup != null)
                {
                    switch (currentDock)
                    {
                        case DockKind.Center:
                        {
                            if (currentDockGroupToInsertWithRespectTo is RootDockGroup ||
                                currentDockGroupToInsertWithRespectTo is StackDockGroup)
                            {
                                draggedGroup.RemoveItselfFromParent();
                                currentDockGroupToInsertWithRespectTo.DockChildren.Add(draggedGroup);

                                var firstLeafItem = DraggedWindow?.LeafItems?.FirstOrDefault();

                                firstLeafItem?.Select();
                            }
                            else
                            {
                                var leafItems = 
                                    DraggedWindow?.GetLeafGroupsIncludingGroupsWithLock()
                                                  .Where(group => !group.IsGroupLocked)
                                                  .SelectMany(g => g.LeafItems).ToList();

                                if (!leafItems.IsNullOrEmptyCollection())
                                {
                                    leafItems.DoForEach(item => item.RemoveItselfFromParent());

                                    var firstLeafItem = leafItems?.FirstOrDefault();

                                    IDockGroup currentGroup =
                                        currentDockGroupToInsertWithRespectTo?.GetContainingGroup()!;

                                    var groupToInsertItemsInto = currentGroup as TabbedDockGroup;

                                    if (groupToInsertItemsInto == null)
                                    {
                                        groupToInsertItemsInto = TabbedGroupFactory.Create();

                                        int currentLeafObjIdx = currentGroup.DockChildren.IndexOf(currentDockGroupToInsertWithRespectTo!);
                                        GridLength sizeCoeff = currentGroup.GetSizeCoeff(currentLeafObjIdx);

                                        currentGroup.DockChildren?.Remove(currentDockGroupToInsertWithRespectTo!);

                                        currentGroup.DockChildren?.Insert(currentLeafObjIdx, groupToInsertItemsInto);
                                        groupToInsertItemsInto.GroupOnlyById = currentGroup.GroupOnlyById;
                                        groupToInsertItemsInto.ProducingUserDefinedWindowGroup = currentGroup.ProducingUserDefinedWindowGroup;

                                        currentGroup.SetSizeCoeff(currentLeafObjIdx, sizeCoeff);

                                        currentDockGroupToInsertWithRespectTo?.CleanSelfOnRemove();

                                        var additionaLeafItems = currentDockGroupToInsertWithRespectTo?.LeafItems;

                                        additionaLeafItems?.DoForEach(item => item.RemoveItselfFromParent());

                                        if (additionaLeafItems != null)
                                        {
                                            leafItems = leafItems?.Union(additionaLeafItems).ToList();
                                        }

                                        groupToInsertItemsInto.ApplyTemplate();
                                    }

                                    groupToInsertItemsInto.DockChildren.InsertCollectionAtStart(leafItems);

                                    firstLeafItem?.Select();
                                }
                            }

                            DraggedWindow?.CloseIfAllowed();

                         break;
                        }
                        case DockKind.Left:
                        case DockKind.Top:
                        case DockKind.Right:
                        case DockKind.Bottom:
                        {
                            DropWithOrientation(currentDockGroupToInsertWithRespectTo, currentDock.Value, draggedGroup);

                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                }
            }
            finally
            {
                if (CurrentLeafObjToInsertWithRespectTo != null)
                {
                    CurrentLeafObjToInsertWithRespectTo = null;
                }

                if (CurrentRootDockGroup != null)
                {
                    CurrentRootDockGroup = null;
                }

                currentWindowToDropInto?.ResetIsCloseAllowed();
                DraggedWindow?.ResetIsCloseAllowed();
                DraggedWindow?.CloseIfAllowed();

                DraggedWindow = null;
            }
        }

        public void SaveDockManagerParamsToStream(Stream stream)
        {
            var dockManagerParams = this.ToParams();

            string serializationStr =
                XmlSerializationUtils.Serialize(dockManagerParams);

            using StreamWriter writer = new StreamWriter(stream);

            writer.Write(serializationStr);

            writer.Flush();
        }

        public void SaveToFile(string filePath)
        {
            using FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            SaveDockManagerParamsToStream(fileStream);
        }

        public void RestoreFromFile
        (
            string filePath, 
            bool restorePredefinedWindowsPositionParams = false)
        {

            if (!File.Exists(filePath))
            {
                return;
            }

            using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            RestoreDockManagerParamsFromStream(fileStream, restorePredefinedWindowsPositionParams);
        }

        public void RestoreDockManagerParamsFromStream(Stream stream, bool restorePredefinedWindowsPositionParams = false)
        {
            using StreamReader reader = new StreamReader(stream);

            string serializationStr = reader.ReadToEnd();

            DockManagerParams dmp =
                XmlSerializationUtils.Deserialize<DockManagerParams>(serializationStr);

            this.SetDockManagerFromParams(dmp, restorePredefinedWindowsPositionParams);
        }

        public IDockGroup? FindGroupById(string? dockId)
        {
            var result = this.AllGroups.FirstOrDefault(g => g.DockId == dockId);

            return result;
        }

        public DockObjectInfo? GetGroupByDockId(string? dockId)
        {
            IDockGroup? dockGroup = FindGroupById(dockId);

            return dockGroup?.ToDockObjectInfo();
        }

        public void SelectTabsInTabbedGroupsWithoutSelection()
        {
            foreach(var group in ConnectedGroups.OfType<TabbedDockGroup>())
            {
                if (group.SelectedItem == null)
                {
                    group.SelectFirst();
                }
            }
        }
    }
}
