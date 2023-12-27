using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace NP.ReloadingProblemSample
{
    public partial class ReloadingUserControl : UserControl, IDisposable
    {
        private static int _staticCount = 0;

        int _count;
        #region Count Styled Avalonia Property
        public int Count
        {
            get { return GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public static readonly StyledProperty<int> CountProperty =
            AvaloniaProperty.Register<ReloadingUserControl, int>
            (
                nameof(Count)
            );
        #endregion Count Styled Avalonia Property


        public ReloadingUserControl()
        {
            _staticCount++;

            _count = _staticCount;
            Count = _staticCount;

            InitializeComponent();
        }

        ~ReloadingUserControl()
        {

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void Dispose()
        {
            
        }
    }
}
