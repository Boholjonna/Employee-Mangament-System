using System.Configuration;
using System.Data;
using System.Windows;

namespace SOFTDEV
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DatabaseHelper.EnsureDatabaseInitialized();
            base.OnStartup(e);
        }
    }

}
