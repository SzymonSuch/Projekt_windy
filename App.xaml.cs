using System.Configuration;
using System.Data;
using System.Windows;

namespace Winda2._0
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow();
            var pietro0Window = new pietro0(mainWindow);
            pietro0Window.Show();
        }
    }
}
