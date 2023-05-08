using System;
using System.Windows.Forms;
using ImageAcquisitionApp.Controllers;
using ImageAcquisitionApp.Views.WebCam;
using Microsoft.Extensions.DependencyInjection;

namespace ImageAcquisitionApp;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        // Set up the DI container
        var serviceProvider = new ServiceCollection()
            .AddTransient<ScanController>()
            .BuildServiceProvider();

        // Create the UserForm and pass in the Controllers as arguments
        var scanController = serviceProvider.GetService<ScanController>();
        var webCamForm = new WebCamForm(scanController);

        Application.Run(webCamForm);
    }
}