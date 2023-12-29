using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using NP.Utilities;
using System;
using System.Drawing.Printing;

namespace NP.Avalonia.UniDock
{
    // this is the stuff containing required implementations from ContentPresenter superclass
    partial class DockContentPresenter
    {
        public Control? TheChild
        {
            get => Child;

            private set => this.SetFieldValue("_child", value, true, typeof(ContentPresenter));
        }

        public bool CreatedChild
        {
            get => this.GetFieldValue<bool>("_createdChild", true, typeof(ContentPresenter));
            set => this.SetFieldValue("_createdChild", value, true, typeof(ContentPresenter));
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            switch (change.Property.Name)
            {
                case nameof(Content):
                case nameof(ContentTemplate):
                    ContentChanged(change);
                    break;
                case nameof(TemplatedParent):
                    TemplatedParentChanged(change);
                    break;
                case nameof(UseLayoutRounding):
                case nameof(BorderThickness):
                    this.SetFieldValue("_layoutThickness", null!, true);
                    break;
            }
        }

        private void TemplatedParentChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var host = e.NewValue as IContentPresenterHost;
            IContentPresenterHost? newHost = 
                host?.RegisterContentPresenter(this) == true ? host : null;

            this.SetPropValue("Host", newHost, true, typeof(ContentPresenter));
        }


        /// <summary>
        /// Ensures neither component of a <see cref="Size"/> is negative.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns>The non-negative size.</returns>
        private static Size NonNegative(Size size)
        {
            return new Size(Math.Max(size.Width, 0), Math.Max(size.Height, 0));
        }

        /// <summary>
        /// The default implementation of the control's measure pass.
        /// </summary>
        /// <param name="availableSize">The size available to the control.</param>
        /// <returns>The desired size for the control.</returns>
        /// <remarks>
        /// This method calls <see cref="MeasureOverride(Size)"/> which is probably the method you
        /// want to override in order to modify a control's arrangement.
        /// </remarks>
        protected override Size MeasureCore(Size availableSize)
        {
            if (IsVisible)
            {
                var margin = Margin;
                var useLayoutRounding = UseLayoutRounding;
                var scale = 1.0;

                if (useLayoutRounding)
                {
                    scale = LayoutHelper.GetLayoutScale(this);
                    margin = LayoutHelper.RoundLayoutThickness(margin, scale, scale);
                }

                ApplyStyling();
                CustomApplyTemplate();

                var constrained = LayoutHelper.ApplyLayoutConstraints(
                    this,
                    availableSize.Deflate(margin));
                var measured = MeasureOverride(constrained);

                var width = measured.Width;
                var height = measured.Height;

                {
                    double widthCache = Width;

                    if (!double.IsNaN(widthCache))
                    {
                        width = widthCache;
                    }
                }

                width = Math.Min(width, MaxWidth);
                width = Math.Max(width, MinWidth);

                {
                    double heightCache = Height;

                    if (!double.IsNaN(heightCache))
                    {
                        height = heightCache;
                    }
                }

                height = Math.Min(height, MaxHeight);
                height = Math.Max(height, MinHeight);

                if (useLayoutRounding)
                {
                    (width, height) = LayoutHelper.RoundLayoutSizeUp(new Size(width, height), scale, scale);
                }

                width = Math.Min(width, availableSize.Width);
                height = Math.Min(height, availableSize.Height);

                return NonNegative(new Size(width, height).Inflate(margin));
            }
            else
            {
                return new Size();
            }
        }

        /// Called when the <see cref="Content"/> property changes.
        /// </summary>
        /// <param name="e">The event args.</param>
        private void ContentChanged(AvaloniaPropertyChangedEventArgs e)
        {
            CreatedChild = false;

            if (((ILogical)this).IsAttachedToLogicalTree)
            {
                if (e.Property.Name == nameof(Content))
                {
                    UpdateChild(e.NewValue);
                }
                else
                {
                    UpdateChild();
                }
            }
            else if (Child != null)
            {
                VisualChildren.Remove(Child);
                GetEffectiveLogicalChildren().Remove(Child);
                ((ISetInheritanceParent)Child).SetParent(Child.Parent);
                TheChild = null;
                RecyclingDataTemplate = null;
            }

            UpdatePseudoClasses();
            InvalidateMeasure();
        }

        private void CustomApplyTemplate()
        {
            if (!CreatedChild && ((ILogical)this).IsAttachedToLogicalTree)
            {
                UpdateChild();
            }
        }

        public void CustomUpdateChild()
        {
            var content = Content;
            UpdateChild(content);
        }

        private void UpdateChild(object? content)
        {
            var contentTemplate = ContentTemplate;
            var oldChild = Child;
            var newChild = CreateChild(content, contentTemplate);
            var logicalChildren = GetEffectiveLogicalChildren();

            // Remove the old child if we're not recycling it.
            if (newChild != oldChild)
            {

                if (oldChild != null)
                {
                    VisualChildren.Remove(oldChild);
                    logicalChildren.Remove(oldChild);
                    ((ISetInheritanceParent)oldChild).SetParent(oldChild.Parent);
                }
            }

            // Set the DataContext if the data isn't a control.
            if (contentTemplate is { } || !(content is Control))
            {
                DataContext = content;
            }
            else
            {
                ClearValue(DataContextProperty);
            }

            // Update the Child.
            if (newChild == null)
            {
                TheChild = null;
            }
            else if (newChild != oldChild)
            {
                ((ISetInheritanceParent)newChild).SetParent(this);
                TheChild = newChild;

                if (!logicalChildren.Contains(newChild))
                {
                    logicalChildren.Add(newChild);
                }

                VisualChildren.Add(newChild);
            }

            CreatedChild = true;

        }
        private void UpdatePseudoClasses()
        {
            PseudoClasses.Set(":empty", Content is null);
        }

        private IAvaloniaList<ILogical> GetEffectiveLogicalChildren()
            => Host?.LogicalChildren ?? LogicalChildren;
    }
}
