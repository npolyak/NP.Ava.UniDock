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
using Avalonia.Layout;
using System.Linq;
using NP.Utilities;
using System.Collections.Generic;
using System;
using Avalonia.Metadata;
using NP.Concepts.Behaviors;
using System.Collections;
using System.Collections.ObjectModel;
using NP.Avalonia.UniDock.Factories;
using Avalonia.VisualTree;
using NP.Avalonia.Visuals.Behaviors;

namespace NP.Avalonia.UniDock
{
    public class StackGroup<T> : Control
        where T : IControl
    {
        internal IDockSeparatorFactory? TheDockSeparatorFactory { get; set; }

        private ObservableCollection<T> _items = new ObservableCollection<T>();
        /// <summary>
        /// Defines the <see cref="Items"/> property.
        /// </summary>
        public static readonly DirectProperty<StackGroup<T>, ObservableCollection<T>> ItemsProperty =
            AvaloniaProperty.RegisterDirect<StackGroup<T>, ObservableCollection<T>>
            (
                nameof(Items),
                o => o.Items,
                (o, v) => o.Items = v);


        /// <summary>
        /// Gets or sets the items to display.
        /// </summary>
        [Content]
        public ObservableCollection<T> Items
        {
            get
            {
                return _items;
            }

            set
            {
                DisposeBehavior();

                SetAndRaise(ItemsProperty, ref _items, value);

                SetBehavior();
            }
        }


        protected virtual void BeforeItemsSet()
        {

        }

        protected virtual void AfterItemsSet()
        {

        }

        public int Count => Items.Count;

        #region TheOrientation Styled Avalonia Property
        public Orientation TheOrientation
        {
            get { return GetValue(TheOrientationProperty); }
            set { SetValue(TheOrientationProperty, value); }
        }

        public static readonly StyledProperty<Orientation> TheOrientationProperty =
            AvaloniaProperty.Register<StackGroup<T>, Orientation>
            (
                nameof(TheOrientation)
            );
        #endregion TheOrientation Styled Avalonia Property

        public int NumberChildren =>
            this.GridChildren.Count;

        public void UpdateLayout()
        {
            var children =
                this.Items.ToList();

            this.Items.RemoveAllOneByOne();

            foreach (T child in children)
            {
                AddLastItem(child);
            }
        }

        public T? FirstItem =>
            (T?)this.GridChildren.FirstOrDefault();


        public T? LastItem =>
            (T?)this.GridChildren.LastOrDefault();

        private static int NextIdx(int idx, bool isAfter)
        {
            if (isAfter)
            {
                return idx + 1;
            }
            else
            {
                return idx;
            }
        }

        private void InsertBeforeOrAfterItem(T? currentItem, T itemToInsert, bool isAfter)
        {
            if (currentItem == null)
            {
                if (NumberChildren > 0)
                {
                    throw new Exception("Programming errorin StackGroup.AddFirstChild - the item collection is not empty.");
                }

                AddDefinition(0, false);
                this.GridChildren.Insert(0, itemToInsert);
            }
            else
            {
                int currentChildIdx = this.GridChildren.IndexOf(currentItem);

                int insertSeparatorIdx = NextIdx(currentChildIdx, isAfter);
                int insertChildIdx = NextIdx(insertSeparatorIdx, isAfter);

                DockSeparator separator = GetSeparator();

                AddDefinition(insertSeparatorIdx, true);
                AddDefinition(insertChildIdx, false);

                this.GridChildren.Insert(insertSeparatorIdx, separator);
                this.GridChildren.Insert(insertChildIdx, itemToInsert);

                ResetChildIndexes();
            }
        }

        private void ResetChildIndexes()
        {
            Action<Control, int> idxSetter =
                TheOrientation == Orientation.Horizontal ? Grid.SetColumn : Grid.SetRow;

            int i = 0;
            foreach (Control child in GridChildren)
            {
                idxSetter(child, i);
                i++;
            }
        }

        private void InsertChildAtIdx(int idx, T itemToInsert)
        {
            if (idx == 0)
            {
                AddChildAtIndexZero(itemToInsert);
            }
            else
            {
                InsertAfterItem(Items[idx - 1], itemToInsert);
            }
        }

        private void AddDefinition(int idx, bool isSeparator)
        {
            if (TheOrientation == Orientation.Horizontal)
            {
                ColumnDefinition definition =
                    new ColumnDefinition(isSeparator ? GridLength.Auto : GridLength.Parse("*"));

                if (!isSeparator)
                {
                    definition.MinWidth = 50;
                }

                this.GridColumnDefinitions.Insert(idx, definition);
            }
            else
            {
                RowDefinition definition =
                    new RowDefinition(isSeparator ? GridLength.Auto : GridLength.Parse("*"));

                if (!isSeparator)
                {
                    definition.MinHeight = 50;
                }

                this.GridRowDefinitions.Insert(idx, definition);
            }
        }

        private void InsertBeforeItem(T? currentItem, T itemToInsert)
        {
            InsertBeforeOrAfterItem(currentItem, itemToInsert, false);
        }

        private void InsertAfterItem(T? currentItem, T itemToInsert)
        {
            InsertBeforeOrAfterItem(currentItem, itemToInsert, true);
        }

        private DockSeparator GetSeparator()
        {
            DockSeparator gridSplitter = TheDockSeparatorFactory?.GetDockSeparator(TheOrientation)!;

            if (TheOrientation == Orientation.Horizontal)
            {
                gridSplitter.TheOrientation = Orientation.Vertical;
            }
            else //TheOrientation == Orientation.Vertical
            {
                gridSplitter.TheOrientation = Orientation.Horizontal;
            }

            return gridSplitter;
        }

        private void ClearItem(IControl item)
        {
            AvaloniaProperty propToClear =
                TheOrientation == Orientation.Horizontal ? Grid.ColumnProperty : Grid.RowProperty;

            item.ClearValue(propToClear);
        }

        private void RemoveItem(T item)
        {
            item.ClearValue(DockAttachedProperties.SizeGridLengthProperty);

            if (NumberChildren == 1)
            {
                this.GridChildren.Remove(item);

                ClearItem(item);

                Definitions.DeleteAllOneByOne();
            }
            else
            {
                int idx = GridChildren.IndexOf(item);

                RemoveChildAt(idx);

                if (idx == 0)
                {
                    RemoveChildAt(0);
                }
                else
                {
                    RemoveChildAt(idx - 1);
                }

                ResetChildIndexes();
            }
        }

        private IList Definitions => TheOrientation == Orientation.Horizontal ? this.GridColumnDefinitions : this.GridRowDefinitions;

        private void RemoveChildAt(int idx)
        {
            IControl item = GridChildren.ElementAt(idx);

            GridChildren.RemoveAt(idx);

            ClearItem(item);

            Definitions.RemoveAt(idx);
        }

        private void AddChildAtIndexZero(T itemToInsert)
        {
            InsertBeforeItem(FirstItem, itemToInsert);
        }

        private void AddLastItem(T item)
        {
            this.InsertAfterItem(LastItem, item);
        }

        Grid _grid = new Grid();
        IDisposable? _disposableBehavior;

        private Controls GridChildren => _grid.Children;
        private ColumnDefinitions GridColumnDefinitions => _grid.ColumnDefinitions;
        private RowDefinitions GridRowDefinitions => _grid.RowDefinitions;

        public StackGroup()
        {
            this.VisualChildren.Add(_grid);
            this.LogicalChildren.Add(_grid);
            SetBehavior();
            AfterItemsSet();

            GridPartsResizeBehavior.SetTarget(_grid, this);
        }

        private void DisposeBehavior()
        {
            _disposableBehavior?.Dispose();
            _disposableBehavior = null;
        }

        private void SetBehavior()
        {
            _disposableBehavior = Items?.AddDetailedBehavior(OnItemAdded, OnItemRemoved)!;
        }

        private void OnItemAdded(IEnumerable<T> controls, T item, int idx)
        {
            InsertChildAtIdx(idx, item);
        }

        private void OnItemRemoved(IEnumerable<T> controls, T item, int idx)
        {
            RemoveItem(item);
        }

        private int GetGridIdx(T item)
        {
            Func<Control, int> idxGetter =
                TheOrientation == Orientation.Horizontal ?
                    Grid.GetColumn : Grid.GetRow;

            return idxGetter((Control)(object)item);
        }

        private int GetGridIdx(int itemIdx)
        {
            return GetGridIdx(Items[itemIdx]);
        }

        private List<int> GetGridIndexes()
        {
            return Items.Select(item => GetGridIdx(item)).ToList(); ;
        }

        public GridLength GetSizeCoefficient(int itemIdx)
        {
            GridLength result = new GridLength(1, GridUnitType.Star);
            if (Items.Count <= 1)
                return result;

            int gridIdx = GetGridIdx(itemIdx);

            if (TheOrientation == Orientation.Horizontal)
            {
                result = _grid.ColumnDefinitions[gridIdx].Width;
            }
            else
            {
                result = _grid.RowDefinitions[gridIdx].Height;
            }

            return result;
        }

        public void SetSizeCoefficient(int itemIdx, GridLength coeff)
        {
            if (Items.Count < 1)
                return;

            int gridIdx = GetGridIdx(itemIdx);

            if (TheOrientation == Orientation.Horizontal)
            {
                _grid.ColumnDefinitions[gridIdx].Width = coeff;
            }
            else
            {
                _grid.RowDefinitions[gridIdx].Height = coeff;
            }
        }
    }

    public class StackGroup : StackGroup<IControl>
    {

    }
}
