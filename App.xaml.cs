using AISIGA.Program;
using AISIGA.Program.Experiments;
using AISIGA.Program.Tests;
using System.Configuration;
using System.Data;
using System.Windows;
using AISIGA.UI;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp.Views.Desktop;

namespace AISIGA;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    //Start point of the code
    //Select the experiment to run here
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        #if DEBUG
                //Tests.Run();
                System.Diagnostics.Trace.WriteLine("Done With Tests, Now starting program");
        #endif

    }
}

