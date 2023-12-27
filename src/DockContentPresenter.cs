using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using NP.Utilities;

namespace NP.Avalonia.UniDock
{
    /// <summary>
    /// we do not use Content and ContentTemplate any longer, instead we get them from the OwningDockItem!!!
    /// Very awkward fromt the development point of view, but we are doing it for the same of not refreshing every time
    /// a pane is pulled out of a tab or dock pane or added to a tab or dock pane. 
    /// </summary>
    public class DockContentPresenter : ContentPresenter, IDockDataContextContainer, IRecyclingTemplateContainer
    {
        #region OwningDockItem Styled Avalonia Property
        public DockItem OwningDockItem
        {
            get { return GetValue(OwningDockItemProperty); }
            set { SetValue(OwningDockItemProperty, value); }
        }

        public static readonly StyledProperty<DockItem> OwningDockItemProperty =
            AvaloniaProperty.Register<DockContentPresenter, DockItem>
            (
                nameof(OwningDockItem)
            );
        #endregion OwningDockItem Styled Avalonia Property

        public IRecyclingDataTemplate? RecyclingDataTemplate
        {
            get => OwningDockItem?.RecyclingDataTemplate;

            set
            {
                if (RecyclingDataTemplate == value)
                {
                    return;
                }

                if (OwningDockItem != null)
                {
                    OwningDockItem.RecyclingDataTemplate = value;
                }
            }
        }

        public IControl? OldChild
        {
            get => OwningDockItem?.OldChild;
            set
            {
                if (OldChild == value)
                    return;

                if (OwningDockItem != null)
                {
                    OwningDockItem.OldChild = value;
                }
            }
        }

        public bool IsHeader { get; set; } = false;

        #region DockDataContext Styled Avalonia Property
        public object? DockDataContext
        {
            get { return GetValue(DockDataContextProperty); }
            set { SetValue(DockDataContextProperty, value); }
        }

        public static readonly StyledProperty<object?> DockDataContextProperty =
            AvaloniaProperty.Register<DockContentPresenter, object?>
            (
                nameof(DockDataContext)
            );
        #endregion DockDataContext Styled Avalonia Property

        /// <summary>
        /// Creates the child control.
        /// </summary>
        /// <returns>The child control or null.</returns>
        protected override IControl? CreateChild()
        {
            if (OwningDockItem == null && !IsHeader)
                return null;

            if (OwningDockItem?.ContentTemplate == null && !IsHeader)
            {
                return CreateChild(Content, null);
            }

            object? content = IsHeader ? Content : OwningDockItem?.Content;
            IDataTemplate? contentTemplate = IsHeader ? ContentTemplate : OwningDockItem?.ContentTemplate;

            return CreateChild(content, contentTemplate);
        }

        private IControl? CreateChild(object? content, IDataTemplate? template)
        {
            var newChild = content as IControl;

            // We want to allow creating Child from the Template, if Content is null.
            // But it's important to not use DataTemplates, otherwise we will break content presenters in many places,
            // otherwise it will blow up every ContentPresenter without Content set.
            if ((newChild == null
                && (content != null || template != null)) || (newChild is { } && template is { }))
            {
                var dataTemplate = this.FindDataTemplate(content, template) ??
                    (
                        RecognizesAccessKey
                            ? FuncDataTemplate.Access
                            : FuncDataTemplate.Default
                    );

                if (dataTemplate is IRecyclingDataTemplate rdt && !IsHeader)
                {
                    if (OldChild != null && OldChild.Parent != this)
                    {
                        ContentPresenter? oldChildsParent = OldChild.Parent as ContentPresenter; ;

                        if (oldChildsParent != null)
                        {
                            oldChildsParent.Content = null;
                            oldChildsParent.ContentTemplate = null;
                            //parent.SetPropValue(nameof(Child), null, true); // not working any more
                        }
                    }

                    var toRecycle = rdt == RecyclingDataTemplate ? OldChild : null;
                    newChild = rdt.Build(content, toRecycle);

                    RecyclingDataTemplate = rdt;
                }
                else
                {
                    newChild = dataTemplate.Build(content);
                }
                if (!IsHeader)
                {
                    OldChild = newChild;
                }
            }
            else
            {
                if (IsHeader)
                {
                    //OldChild = this.Child;
                    RecyclingDataTemplate = null;
                }
            }

            return newChild;
        }

        public DockContentPresenter()
        {

        }
    }
}
