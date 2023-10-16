using AnyCAD.NX.Layer;
using AnyCAD.NX.Plugin;
using AnyCAD.NX.ViewModel;
using ControlzEx.Theming;
using Fluent;
using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace AnyDraw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IRibbonWindow
    {
        LayerListViewModel _layerListViewModel;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += this.MahMetroWindow_Loaded;
            this.Closed += this.MahMetroWindow_Closed;
            var viewModel = new MainViewModelImpl(mView3d);
            this.DataContext = viewModel;

            _layerListViewModel = new LayerListViewModel(viewModel);
            mLayerBrower.ItemsSource = _layerListViewModel.Items;
        }

        public MainRibbonViewModel ViewModel { get => (MainRibbonViewModel)this.DataContext; }

        private void MahMetroWindow_Loaded(object? sender, System.Windows.RoutedEventArgs e)
        {
            this.TitleBar = this.FindChild<RibbonTitleBar>("RibbonTitleBar");
            this.TitleBar.InvalidateArrange();
            this.TitleBar.UpdateLayout();

            // We need this inside this window because MahApps.Metro is not loaded globally inside the Fluent.Ribbon Showcase application.
            // This code is not required in an application that loads the MahApps.Metro styles globally.
            var theme = ThemeManager.Current.DetectTheme(Application.Current);
            if (theme != null)
            {
                ThemeManager.Current.ChangeTheme(this, theme);
            }

            ThemeManager.Current.ThemeChanged += this.SyncThemes;

            PluginConfig.Load(ViewModel);
        }

        void MView3d_ViewerReady()
        {
            ViewModel.Initialize(mView3d);
        }

        private void HelpWebsite(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://www.anycad.cn",
                // UseShellExecute is default to false on .NET Core while true on .NET Framework.
                // Only this value is set to true, the url link can be opened.
                UseShellExecute = true,
            });
        }

        private void SyncThemes(object? sender, ThemeChangedEventArgs e)
        {
            if (e.Target == this)
            {
                return;
            }

            ThemeManager.Current.ChangeTheme(this, e.NewTheme);
        }

        private void MetroWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized || this.WindowState == WindowState.Normal)
                mView3d.ForceUpdate();
        }

        private void MahMetroWindow_Closed(object? sender, EventArgs e)
        {
            ThemeManager.Current.ThemeChanged -= this.SyncThemes;
        }

        #region TitelBar

        /// <summary>
        /// Gets ribbon titlebar
        /// </summary>
        public RibbonTitleBar TitleBar
        {
            get { return (RibbonTitleBar)this.GetValue(TitleBarProperty); }
            private set { this.SetValue(TitleBarPropertyKey, value); }
        }

        // ReSharper disable once InconsistentNaming
        private static readonly DependencyPropertyKey TitleBarPropertyKey = DependencyProperty.RegisterReadOnly(nameof(TitleBar), typeof(RibbonTitleBar), typeof(MainWindow), new PropertyMetadata());

#pragma warning disable WPF0060
        /// <summary>Identifies the <see cref="TitleBar"/> dependency property.</summary>
        public static readonly DependencyProperty TitleBarProperty = TitleBarPropertyKey.DependencyProperty;
#pragma warning restore WPF0060

        #endregion
    }
}
