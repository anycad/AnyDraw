using AnyCAD.Foundation;
using System.Windows;

namespace dxfViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            GlobalInstance.Destroy();
        }
    }
}
