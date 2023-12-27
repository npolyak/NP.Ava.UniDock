using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using NP.Utilities;
using System.ComponentModel;

namespace NP.Avalonia.UniDock
{
    public class FloatingWindowContainer : IDockManagerContainer
    {
        private void TrySetWindow()
        {
            if ( (_dockManager == null) ||
                 (_windowSize == null) ||
                 (_windowRelativePosition == null) ||
                 (_windowId == null) || 
                 (ParentWindow == null) )
            {
                return;
            }

            _floatingWindow = _dockManager.FloatingWindowFactory.CreateFloatingWindow();

            SetProducingUserDefinedWindowGroup();

            _floatingWindow.Position = WindowRelativePosition!.Value + ParentWindow.Position;
            _floatingWindow.Width = _windowSize.Value.X;
            _floatingWindow.Height = _windowSize.Value.Y;
            DockAttachedProperties.SetWindowId(_floatingWindow, WindowId);
            _floatingWindow.TheDockManager = _dockManager;
            SetExtras();

            Window ownerWindow = DockAttachedProperties.GetDockChildWindowOwner(ParentWindow);

            if (ownerWindow != null)
            {
                DockAttachedProperties.SetDockChildWindowOwner(_floatingWindow, ownerWindow);

                _floatingWindow.Show(ownerWindow);
            }
            else
            {
                _floatingWindow.Show();
            }
        }

        private RootDockGroup? _producingUserDefinedWindowGroup;
        public RootDockGroup? ProducingUserDefinedWindowGroup 
        { 
            get => _producingUserDefinedWindowGroup; 
            internal set
            {
                if (_producingUserDefinedWindowGroup == value)
                {
                    return;
                }

                _producingUserDefinedWindowGroup = value;
                SetProducingUserDefinedWindowGroup();
            }
        }

        private void SetProducingUserDefinedWindowGroup()
        {
            if (_floatingWindow != null && ProducingUserDefinedWindowGroup != null)
            {
                _floatingWindow.ProducingUserDefinedWindowGroup = ProducingUserDefinedWindowGroup;
            }
        }

        DockManager? _dockManager;
        public DockManager? TheDockManager 
        {
            get => _dockManager;
            set
            {
                if (_dockManager == value)
                    return;

                _dockManager = value;
                TrySetWindow();
            }
        }

        private FloatingWindow? _floatingWindow;

        private PixelPoint? _windowRelativePosition;
        /// <summary>
        /// relative to the parent window
        /// </summary>
        public PixelPoint? WindowRelativePosition 
        { 
            get => _windowRelativePosition;
            set
            {
                if (_windowRelativePosition.ObjEquals(value))
                {
                    return;
                }

                _windowRelativePosition = value;
                TrySetWindow();
            }
        }

        private PixelPoint? _windowSize;
        public PixelPoint? WindowSize 
        {
            get => _windowSize; 
            set
            {
                if (_windowSize.ObjEquals(value))
                {
                    return;
                }

                _windowSize = value;
                TrySetWindow();
            }
        }

        private string? _windowId;
        public string? WindowId
        {
            get => _windowId;
            set
            {
                if (_windowId == value)
                {
                    return;
                }

                _windowId = value;
                TrySetWindow();
            }
        }

        private IDockGroup? _dockContent;
        [Content]
        public IDockGroup? DockContent
        {
            get => _dockContent;

            set
            {
                if (_dockContent == value)
                    return;

                _dockContent = value;

                SetDockContent();
            }
        }

        private void SetDockContent()
        {
            if (_floatingWindow != null)
            {
                if (_dockContent != null)
                {
                    _floatingWindow.TheDockGroup.TheChild = _dockContent;
                }
            }
        }

        #region Title Property
        private string? _title;
        public string? Title
        {
            get
            {
                return this._title;
            }
            set
            {
                if (this._title == value)
                {
                    return;
                }

                this._title = value;

                if (_floatingWindow != null && Title != null)
                {
                    _floatingWindow.Title = Title;
                }
            }
        }
        #endregion Title Property

        Window? _parentWindow;
        internal Window? ParentWindow 
        {
            get => _parentWindow;
            set
            {
                if (_parentWindow == value)
                    return;

                _parentWindow = value;

                TrySetWindow();
            }
        }

        private void SetExtras()
        {
            if (_floatingWindow == null)
                return;

            if (Title != null)
            {
                _floatingWindow.Title = Title;
            }
            SetDockContent();
        }
    }
}
