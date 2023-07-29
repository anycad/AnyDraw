using AnyCAD.Foundation;
using AnyCAD.IO.DXF;
using System.Windows;

namespace dxfViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void mViewer_ViewerReady()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".dxf",
                Filter = "DXF Files (*.dxf)|*.dxf"
            };
            var result = dlg.ShowDialog();
            if (result != true)
                return;

            try
            {
                var edges = DxfIO.Load(dlg.FileName, 1);
                foreach (var edge in edges)
                {
                    mViewer.ShowShape(edge, ColorTable.Black);
                }
            }
            catch
            {
            
            }



            mViewer.ZoomAll();

            mViewer.ViewContext.SetOrbitButton(EnumMouseButton.Zero);
        }
    }
}
