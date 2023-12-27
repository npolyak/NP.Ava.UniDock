using NP.Concepts.Behaviors;
using NP.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace NP.Avalonia.UniDock
{
    public class SingleActiveBehavior<TActiveItem> : VMBase
        where TActiveItem : class, IActiveItem<TActiveItem>, IDockGroup
    {
        IDisposable? _behaviorDisposable = null;
        public event Action? ActiveItemChangedEvent = null;


        #region TheCollection Property
        private IEnumerable<TActiveItem>? _collection;
        public IEnumerable<TActiveItem>? TheCollection
        {
            get => _collection;
            set
            {
                if (ReferenceEquals(_collection, value))
                    return;

                _behaviorDisposable?.Dispose();

                _collection = value;

                _behaviorDisposable =
                    _collection?.AddBehavior(OnItemAdded, OnItemRemoved);

                OnCollectionSet();
            }
        }
        #endregion TheCollection Property

        protected virtual void OnCollectionSet()
        {
            MakeFirstItemActiveIfNoActive();
        }

        protected virtual void BeforeItemAdded(TActiveItem item)
        {

        }

        private void OnItemAdded(TActiveItem item)
        {
            BeforeItemAdded(item);

            if (item.IsActive)
            {
                Item_IsActiveChanged(item);
            }

            item.IsActiveChanged += Item_IsActiveChanged;
        }

        private void OnItemRemoved(TActiveItem item)
        {
            item.IsActiveChanged -= Item_IsActiveChanged;

            if (item.ObjEquals(TheActiveItem) && TheActiveItem != null)
            {
                DoOnActiveItemRemoved();
            }
        }

        protected virtual void DoOnActiveItemRemoved()
        {
            TheActiveItem = null;
        }

        public SingleActiveBehavior()
        {

        }

        public SingleActiveBehavior(IEnumerable<TActiveItem> collection)
        {
            TheCollection = collection;
        }

        private void Item_IsActiveChanged(IActiveItem<TActiveItem> item)
        {
            if (item.IsActive)
            {
                this.TheActiveItem = (TActiveItem)item;
            }
            else
            {
                TheActiveItem = null;
            }
        }

        public bool HasActiveItem => TheActiveItem != null;

        #region TheActiveItem Property
        private TActiveItem? _activeItem;
        [XmlIgnore]
        public TActiveItem? TheActiveItem
        {
            get
            {
                return this._activeItem;
            }
            set
            {
                if (ReferenceEquals(_activeItem, value))
                {
                    return;
                }

                if (_activeItem != null)
                {
                    _activeItem.IsActive = false;
                }

                this._activeItem = value;

                if (_activeItem != null)
                {
                    _activeItem.IsActive = true;
                }

                ActiveItemChangedEvent?.Invoke();
                this.OnPropertyChanged(nameof(TheActiveItem));
            }
        }
        #endregion TheActiveItem Property

        public void MakeFirstItemActive()
        {
            TheCollection?.Where(item => item.IsDockVisible).FirstOrDefault()?.MakeActive();
        }

        public void MakeFirstItemActiveIfNoActive()
        {
            if (!HasActiveItem)
            {
                MakeFirstItemActive();
            }
        }
    }
}
