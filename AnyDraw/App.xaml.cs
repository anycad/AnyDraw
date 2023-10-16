using System;
using System.Windows;

namespace AnyDraw
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void Application_Startup(object sender, StartupEventArgs e)
        {            
            AnyCAD.Foundation.Application.Startup();

            AnyCAD.Foundation.DocumentManager.Instance().SetDocType("Drawing");

            AnyCAD.UX.Icons.SetTheme(AnyCAD.UX.EnumTheme.Dark);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            AnyCAD.Foundation.Application.Exit();
        }
    }
}
