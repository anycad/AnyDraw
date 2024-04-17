using AnyCAD.Foundation;
using AnyDraw.IO;
using System.Diagnostics;
using System.Windows;

namespace AnyDraw.Nest
{
    /// <summary>
    /// NestConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class NestConfigView : Window
    {
        Document _Document;
        public NestConfigView(Document doc)
        {
            InitializeComponent();
            Owner = System.Windows.Application.Current.MainWindow;

            var config = NestConfig.Load(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "anynest_config.json"));
            if(config == null )
            {
                config = new NestConfig();
            }

            _Document = doc;
            btnOK.IsEnabled = System.IO.File.Exists(config.EnginePath);

            this.DataContext = config;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "AnyNest.exe",
                DefaultExt = ".exe",
                Filter = "AnyNest Files (*.exe)|*.exe"
            };
            var result = dlg.ShowDialog();
            if (result != true)
                return;
            var config = (NestConfig)DataContext;
            config.EnginePath = dlg.FileName;
            btnOK.IsEnabled = System.IO.File.Exists(config.EnginePath);
        }

        void Apply()
        {
            var config = (NestConfig)DataContext;
            if (!System.IO.File.Exists(config.EnginePath))
                return;

            var db = new AnyCAD.IO.Drawing.DrawingDb();
            string input = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "anynest.json");
            db.Save(_Document, input);       

            string configFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "anynest_config.json");
            config.Save(configFile);

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = config.EnginePath;
            startInfo.Arguments = $"--input {input} --output {input} --config {configFile}";

            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = false;

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                int exitCode = process.ExitCode;
            }

            var output = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "anynest_result.json");
            if (System.IO.File.Exists(output))
            {
                var nr = AnyDraw.IO.NestResult.Load(output);
                nr.Apply(_Document);
            }
        }
        private void Button_Click_OK(object sender, RoutedEventArgs e)
        {            
            this.Close();
            Apply();
        }

        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
