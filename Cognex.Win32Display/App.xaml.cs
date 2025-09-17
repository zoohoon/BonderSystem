//using LogModule;
using System;
using System.Windows;

namespace Cognex.Win32Display
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                if (e.Args.Length > 0)
                {
                    //==> cognex.win32display.exe�� ����ɶ� ó�� ���� �Ǵ� �ڵ�
                    //==> ���ڴ� ProberSystem.exe�� ����ϱ� ���� ProberSystem.exe�� Process Handle
                    var hostHandle = (IntPtr)Convert.ToInt32(e.Args[0]);

                    // Create main application window, starting minimized if specified
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.HostHandle = hostHandle;
                    mainWindow.Show();
                }
                else
                {

                }

            }
#pragma warning disable 0168 
            catch (Exception err)
            {
                //LoggerManager.Exception(err);
                throw;
            }
#pragma warning restore 0168
        }
    }
}
